# workers/ dizininden derlenir:  docker build -t govai-worker workers/
FROM python:3.12-slim AS base

ENV PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    PIP_NO_CACHE_DIR=1

WORKDIR /app

# lxml ve pypdf için gerekli derleme/çalışma bağımlılıkları.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libxml2 libxslt1.1 \
    && rm -rf /var/lib/apt/lists/*

COPY pyproject.toml ./
COPY govai_workers/ ./govai_workers/

RUN pip install --upgrade pip && pip install .

RUN useradd --uid 10001 --create-home --shell /usr/sbin/nologin govai
USER govai

# Varsayılan olarak collector çalışır; compose içinde her servis kendi komutunu verir.
CMD ["govai-collector"]
