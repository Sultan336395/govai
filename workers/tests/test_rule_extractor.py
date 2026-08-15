"""Deterministik kural çıkarımının davranış sözleşmesi.

Bu testler LLM gerektirmez; ürünün AI olmadan da anlamlı kural üretebildiğini garanti eder.
"""

from __future__ import annotations

from govai_workers.parser.extractors import find_deadline, normalize
from govai_workers.parser.rule_extractor import (
    ALLOWED_FIELDS,
    _is_valid_rule,
    extract_deterministic,
)


def test_asgari_calisan_sayisi_cikarilir() -> None:
    text = "Başvuru sahibinin son bordroya göre en az 10 çalışanı bulunmalıdır."

    rules = extract_deterministic(text)
    rule = next(r for r in rules if r.field == "Workforce.EmployeeCount")

    assert rule.operator == "GreaterThanOrEqual"
    assert rule.value == "10"
    assert rule.severity == "Blocking"
    assert rule.confidence == 1.0
    assert "en az 10" in (rule.sourceExcerpt or "")


def test_azami_calisan_sayisi_cikarilir() -> None:
    text = "Programa en fazla 249 sigortalı çalıştıran işletmeler başvurabilir."

    rules = extract_deterministic(text)
    rule = next(r for r in rules if r.operator == "LessThanOrEqual")

    assert rule.field == "Workforce.EmployeeCount"
    assert rule.value == "249"


def test_kadin_istihdam_orani_ondaliga_cevrilir() -> None:
    text = "Kadın çalışan oranı en az %30 olan işletmeler önceliklendirilir."

    rules = extract_deterministic(text)
    rule = next(r for r in rules if r.field == "Workforce.WomenEmployeeRate")

    assert rule.value == "0.30"
    assert rule.dimension == "Employment"


def test_nuts2_bolge_kodlari_toplanir() -> None:
    text = "Program TR62 ve TR61 Düzey 2 bölgelerinde uygulanacaktır."

    rules = extract_deterministic(text)
    rule = next(r for r in rules if r.field == "Company.Nuts2Codes")

    assert rule.operator == "ContainsAny"
    assert set(rule.value.split(",")) == {"TR61", "TR62"}
    assert rule.severity == "Blocking"


def test_iso_belgeleri_yakalanir() -> None:
    text = "ISO 9001 ve ISO-14001 belgesine sahip başvurular ilave puan alır."

    rules = extract_deterministic(text)
    rule = next(r for r in rules if r.field == "Company.Certificates")

    assert set(rule.value.split(",")) == {"ISO9001", "ISO14001"}


def test_kosul_icermeyen_metin_kural_uretmez() -> None:
    text = "Bu duyuru yalnızca bilgilendirme amaçlıdır."
    assert extract_deterministic(text) == []


def test_son_basvuru_tarihi_sayisal_formattan_okunur() -> None:
    text = "Son başvuru tarihi: 15/09/2026 saat 23:59'dur."
    assert find_deadline(text) == "2026-09-15T23:59:59+03:00"


def test_son_basvuru_tarihi_ay_adindan_okunur() -> None:
    text = "Başvurular 30 Eylül 2026 tarihine kadar alınacak olup son başvuru saati 17:00'dir."
    assert find_deadline(text) == "2026-09-30T23:59:59+03:00"


def test_gecersiz_alan_adi_elenir() -> None:
    assert not _is_valid_rule({
        "field": "Company.UydurmaAlan",
        "operator": "Equals",
        "value": "1",
        "dimension": "Sector",
        "severity": "Blocking",
    })


def test_gecerli_kural_kabul_edilir() -> None:
    assert _is_valid_rule({
        "field": "Workforce.EmployeeCount",
        "operator": "GreaterThanOrEqual",
        "value": "10",
        "dimension": "Employment",
        "severity": "Blocking",
    })


def test_alan_beyaz_listesi_dotnet_ile_ayni_isimlendirmeyi_kullanir() -> None:
    # .NET CompanyFieldResolver.SupportedFields ile senkron kalmalı.
    assert "Workforce.EmployeeCount" in ALLOWED_FIELDS
    assert "Financials.AnnualRevenue" in ALLOWED_FIELDS
    assert "Company.Nuts2Codes" in ALLOWED_FIELDS
    assert all("." in name for name in ALLOWED_FIELDS)


def test_normalize_bosluk_ve_satirlari_sadelestirir() -> None:
    assert normalize("Satır  bir\r\n\n\n\nSatır   iki  ") == "Satır bir\n\nSatır iki"
