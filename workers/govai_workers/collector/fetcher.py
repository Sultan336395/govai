"""HTTP indirme katmanı.

Resmî kaynaklara karşı nazik davranmak zorunludur: robots.txt'e uyulur, istekler arasında
gecikme bırakılır ve gövde boyutu sınırlanır. Bu, hem etik hem de kaynağın bizi engellememesi
için operasyonel bir gerekliliktir.
"""

from __future__ import annotations

import time
from dataclasses import dataclass
from urllib.parse import urljoin, urlparse
from urllib.robotparser import RobotFileParser

import httpx
from tenacity import retry, retry_if_exception_type, stop_after_attempt, wait_exponential

from govai_workers.config import settings
from govai_workers.logging_setup import get_logger

log = get_logger(__name__)


@dataclass(slots=True)
class FetchedDocument:
    url: str
    content: bytes
    media_type: str
    status_code: int

    @property
    def is_pdf(self) -> bool:
        return "pdf" in self.media_type.lower() or self.url.lower().endswith(".pdf")


class PoliteFetcher:
    """robots.txt'e uyan, hız sınırlı HTTP istemcisi."""

    def __init__(self) -> None:
        self._client = httpx.Client(
            follow_redirects=True,
            timeout=settings.api_timeout_seconds,
            headers={"User-Agent": settings.crawl_user_agent},
        )
        self._robots: dict[str, RobotFileParser | None] = {}
        self._last_request_at: float = 0.0

    def can_fetch(self, url: str) -> bool:
        if not settings.respect_robots_txt:
            return True

        parsed = urlparse(url)
        origin = f"{parsed.scheme}://{parsed.netloc}"

        if origin not in self._robots:
            self._robots[origin] = self._load_robots(origin)

        parser = self._robots[origin]
        if parser is None:
            # robots.txt okunamadıysa engellenmediğimizi varsayarız (RFC 9309 davranışı).
            return True

        return parser.can_fetch(settings.crawl_user_agent, url)

    def _load_robots(self, origin: str) -> RobotFileParser | None:
        try:
            response = self._client.get(urljoin(origin, "/robots.txt"))
            if response.status_code != httpx.codes.OK:
                return None

            parser = RobotFileParser()
            parser.parse(response.text.splitlines())
            return parser
        except httpx.HTTPError:
            return None

    def _throttle(self) -> None:
        elapsed = time.monotonic() - self._last_request_at
        if elapsed < settings.crawl_delay_seconds:
            time.sleep(settings.crawl_delay_seconds - elapsed)
        self._last_request_at = time.monotonic()

    @retry(
        retry=retry_if_exception_type(httpx.TransportError),
        stop=stop_after_attempt(3),
        wait=wait_exponential(multiplier=2, min=2, max=30),
        reraise=True,
    )
    def fetch(self, url: str) -> FetchedDocument | None:
        if not self.can_fetch(url):
            log.info("robots_disallow", url=url)
            return None

        self._throttle()

        with self._client.stream("GET", url) as response:
            if response.status_code != httpx.codes.OK:
                log.warning("fetch_non_ok", url=url, status=response.status_code)
                return None

            chunks: list[bytes] = []
            total = 0
            for chunk in response.iter_bytes():
                total += len(chunk)
                if total > settings.crawl_max_document_bytes:
                    log.warning("document_too_large", url=url, bytes=total)
                    return None
                chunks.append(chunk)

            content_type = response.headers.get("content-type", "application/octet-stream")

            return FetchedDocument(
                url=str(response.url),
                content=b"".join(chunks),
                media_type=content_type.split(";")[0],
                status_code=response.status_code,
            )

    def close(self) -> None:
        self._client.close()

    def __enter__(self) -> PoliteFetcher:
        return self

    def __exit__(self, *_: object) -> None:
        self.close()
