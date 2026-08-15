"""Worker yapılandırması.

Tüm ayarlar ortam değişkenlerinden okunur; docker-compose ve .env dosyası aynı adları kullanır.
"""

from __future__ import annotations

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_prefix="GOVAI_",
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    # ---- GOVAI API ----
    api_base_url: str = Field(default="http://localhost:8080")
    api_email: str = Field(default="admin@govai.local")
    api_password: str = Field(default="")
    api_timeout_seconds: float = Field(default=60.0)

    # ---- RabbitMQ ----
    rabbitmq_host: str = Field(default="localhost")
    rabbitmq_port: int = Field(default=5672)
    rabbitmq_user: str = Field(default="govai")
    rabbitmq_password: str = Field(default="govai_dev")
    rabbitmq_vhost: str = Field(default="/")
    rabbitmq_exchange: str = Field(default="govai.events")

    # ---- Tarama davranışı ----
    crawl_user_agent: str = Field(
        default="GovAI-Collector/0.1 (+https://talenthubik.com; iletisim: info@talenthubik.com)"
    )
    crawl_delay_seconds: float = Field(default=1.5)
    crawl_max_pages: int = Field(default=50)
    crawl_max_document_bytes: int = Field(default=15 * 1024 * 1024)
    respect_robots_txt: bool = Field(default=True)

    # ---- AI kural çıkarımı ----
    openai_api_key: str = Field(default="")
    openai_extraction_model: str = Field(default="gpt-4.1")
    rule_extraction_enabled: bool = Field(default=True)

    # ---- Çalışma zamanı ----
    log_level: str = Field(default="INFO")
    log_json: bool = Field(default=True)
    prefetch_count: int = Field(default=4)

    @property
    def api_url(self) -> str:
        return self.api_base_url.rstrip("/")


settings = Settings()
