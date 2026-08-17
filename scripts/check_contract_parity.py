#!/usr/bin/env python3
"""C# ve Python arasındaki paylaşılan sözleşmelerin senkron kaldığını doğrular.

İki yığın arasında derleyicinin koruyamadığı iki sözleşme var:

1. **Alan beyaz listesi** — kural motorunun tanıdığı firma alanları.
   `CompanyFieldResolver.SupportedFields` (C#) ile `ALLOWED_FIELDS` (Python) aynı olmalıdır.
   Python tarafına bir alan eklenip C# tarafına eklenmezse, AI o alanla kural üretir ve
   kural motoru onu sessizce `Unknown` sayar — skor bozulur ama hiçbir hata görünmez.

2. **Kuyruk adları** — `QueueNames` (C#) ile `RoutingKeys` (Python).
   Uyuşmazlık, mesajların hiçbir tüketiciye ulaşmadan kaybolmasına yol açar; yine sessizdir.

Bu betik ikisini de karşılaştırır ve fark bulursa sıfırdan farklı kodla çıkar.
CI'da her push'ta çalışır.

Kullanım:  python scripts/check_contract_parity.py
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent

CS_RESOLVER = REPO_ROOT / "src/GovAI.Domain/Eligibility/CompanyFieldResolver.cs"
CS_QUEUES = REPO_ROOT / "src/GovAI.Application/Abstractions/Services/IServices.cs"
PY_EXTRACTOR = REPO_ROOT / "workers/govai_workers/parser/rule_extractor.py"
PY_MESSAGING = REPO_ROOT / "workers/govai_workers/messaging.py"


def read(path: Path) -> str:
    if not path.exists():
        raise SystemExit(f"HATA: beklenen dosya yok: {path.relative_to(REPO_ROOT)}")
    return path.read_text(encoding="utf-8")


def between(text: str, start_marker: str, end_marker: str, source: str) -> str:
    if start_marker not in text:
        raise SystemExit(f"HATA: '{start_marker}' işareti {source} içinde bulunamadı.")
    tail = text.split(start_marker, 1)[1]
    if end_marker not in tail:
        raise SystemExit(f"HATA: '{end_marker}' kapanışı {source} içinde bulunamadı.")
    return tail.split(end_marker, 1)[0]


def csharp_fields() -> set[str]:
    block = between(read(CS_RESOLVER), "SupportedFields =", "};", "CompanyFieldResolver.cs")
    return set(re.findall(r'\["([\w.]+)"\]', block))


def python_fields() -> set[str]:
    block = between(
        read(PY_EXTRACTOR), "ALLOWED_FIELDS: dict[str, str] = {", "\n}", "rule_extractor.py"
    )
    return set(re.findall(r'"([\w.]+)":', block))


def csharp_queues() -> set[str]:
    block = between(read(CS_QUEUES), "class QueueNames", "\n}", "IServices.cs")
    return set(re.findall(r'=\s*"([\w.]+)"', block))


def python_queues() -> set[str]:
    block = between(read(PY_MESSAGING), "class RoutingKeys:", "\ndef ", "messaging.py")
    return set(re.findall(r'=\s*"([\w.]+)"', block))


def compare(label: str, csharp: set[str], python: set[str]) -> bool:
    only_cs = sorted(csharp - python)
    only_py = sorted(python - csharp)

    if not only_cs and not only_py:
        print(f"  OK  {label}: {len(csharp)} kayıt, iki tarafta da aynı")
        return True

    print(f"  HATA  {label}: taraflar ayrışmış")
    for name in only_cs:
        print(f"        yalnızca C# tarafında: {name}")
    for name in only_py:
        print(f"        yalnızca Python tarafında: {name}")
    return False


def main() -> int:
    print("GOVAI sözleşme denetimi (C# <-> Python)")

    checks = [
        compare("Alan beyaz listesi", csharp_fields(), python_fields()),
        compare("Kuyruk adları", csharp_queues(), python_queues()),
    ]

    if all(checks):
        print("Tüm sözleşmeler senkron.")
        return 0

    print(
        "\nDüzeltme: C# tarafı kaynak doğrudur. Python listesini ona göre güncelleyin "
        "(rule_extractor.py / messaging.py)."
    )
    return 1


if __name__ == "__main__":
    sys.exit(main())
