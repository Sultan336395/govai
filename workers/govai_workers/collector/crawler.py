"""Kaynak tarayıcı.

Her kaynağın yapısı farklıdır; bu nedenle seçiciler kaynak kaydındaki `configurationJson`
alanından okunur. Yeni bir kurum eklemek kod değişikliği değil, konfigürasyon değişikliğidir:

    {
      "listUrl": "/duyurular",
      "linkSelector": "a.duyuru-link",
      "titleSelector": "h1.baslik",
      "contentSelector": "div.icerik",
      "maxPages": 3,
      "urlPattern": "duyuru|ilan|cagri"
    }
"""

from __future__ import annotations

import json
import re
from dataclasses import dataclass, field
from urllib.parse import urljoin, urlparse

from bs4 import BeautifulSoup

from govai_workers.collector.fetcher import FetchedDocument, PoliteFetcher
from govai_workers.config import settings
from govai_workers.logging_setup import get_logger
from govai_workers.parser.extractors import extract_text

log = get_logger(__name__)


@dataclass(slots=True)
class SourceConfig:
    """Kaynak kaydındaki `configurationJson` alanının tipli hâli."""

    list_url: str = ""
    link_selector: str = "a"
    title_selector: str = "h1"
    content_selector: str = ""
    url_pattern: str = ""
    max_pages: int = 1

    @classmethod
    def parse(cls, raw: str | None) -> SourceConfig:
        if not raw:
            return cls()

        try:
            data = json.loads(raw)
        except json.JSONDecodeError:
            log.warning("source_config_invalid_json")
            return cls()

        return cls(
            list_url=data.get("listUrl", ""),
            link_selector=data.get("linkSelector", "a"),
            title_selector=data.get("titleSelector", "h1"),
            content_selector=data.get("contentSelector", ""),
            url_pattern=data.get("urlPattern", ""),
            max_pages=int(data.get("maxPages", 1)),
        )


@dataclass(slots=True)
class CrawlResult:
    collected: int = 0
    skipped: int = 0
    failed: int = 0
    errors: list[str] = field(default_factory=list)

    def summary(self) -> str:
        return f"{self.collected} doküman alındı, {self.skipped} atlandı, {self.failed} başarısız"


class SourceCrawler:
    """Tek bir kaynağı tarar ve bulduğu ilan sayfalarını API'ye bırakır."""

    def __init__(self, fetcher: PoliteFetcher, ingest) -> None:  # noqa: ANN001 - callable protokolü
        self._fetcher = fetcher
        self._ingest = ingest

    def crawl(self, source: dict) -> CrawlResult:
        config = SourceConfig.parse(source.get("configurationJson"))
        base_url = source["baseUrl"]
        list_url = urljoin(base_url, config.list_url) if config.list_url else base_url

        result = CrawlResult()

        listing = self._safe_fetch(list_url, result)
        if listing is None:
            return result

        links = self._discover_links(listing, base_url, config)
        log.info("links_discovered", source=source["name"], count=len(links))

        for link in links[: settings.crawl_max_pages]:
            document = self._safe_fetch(link, result)
            if document is None:
                result.skipped += 1
                continue

            try:
                title, content = self._extract(document, config)
                if not content.strip():
                    result.skipped += 1
                    continue

                response = self._ingest(
                    source_id=source["id"],
                    url=document.url,
                    title=title,
                    raw_content=content,
                    media_type=document.media_type,
                )

                if response and response.get("contentChanged", True):
                    result.collected += 1
                else:
                    result.skipped += 1

            except Exception as exc:  # noqa: BLE001 - tek doküman hatası taramayı durdurmamalı
                result.failed += 1
                result.errors.append(f"{link}: {exc}")
                log.exception("document_ingest_failed", url=link)

        return result

    def _safe_fetch(self, url: str, result: CrawlResult) -> FetchedDocument | None:
        try:
            return self._fetcher.fetch(url)
        except Exception as exc:  # noqa: BLE001
            result.failed += 1
            result.errors.append(f"{url}: {exc}")
            log.warning("fetch_failed", url=url, error=str(exc))
            return None

    @staticmethod
    def _discover_links(listing: FetchedDocument, base_url: str, config: SourceConfig) -> list[str]:
        if listing.is_pdf:
            return [listing.url]

        soup = BeautifulSoup(listing.content, "lxml")
        pattern = re.compile(config.url_pattern, re.IGNORECASE) if config.url_pattern else None
        base_host = urlparse(base_url).netloc

        links: list[str] = []
        seen: set[str] = set()

        for anchor in soup.select(config.link_selector):
            href = anchor.get("href")
            if not href or href.startswith(("#", "mailto:", "javascript:")):
                continue

            absolute = urljoin(listing.url, href)

            # Kaynak dışı alan adlarına çıkma; tarayıcı kendi sitesinde kalmalı.
            if urlparse(absolute).netloc != base_host:
                continue

            if pattern is not None and not pattern.search(absolute):
                continue

            if absolute in seen:
                continue

            seen.add(absolute)
            links.append(absolute)

        return links

    @staticmethod
    def _extract(document: FetchedDocument, config: SourceConfig) -> tuple[str, str]:
        if document.is_pdf:
            text = extract_text(document.content, "application/pdf")
            title = text.strip().splitlines()[0][:300] if text.strip() else document.url
            return title, text

        soup = BeautifulSoup(document.content, "lxml")

        title_node = soup.select_one(config.title_selector) if config.title_selector else None
        title = (title_node.get_text(strip=True) if title_node else None) or (
            soup.title.get_text(strip=True) if soup.title else document.url
        )

        if config.content_selector:
            content_node = soup.select_one(config.content_selector)
            html = str(content_node) if content_node else str(soup)
        else:
            html = str(soup)

        return title[:300], html
