# GOVAI

**Kurumsal Teşvik, Hibe ve İhale Uygunluk Analizi ve Yapay Zekâ Destekli Karar Destek Platformu**

Şirketin ERP, İK, muhasebe ve operasyon verisini resmî teşvik/hibe/ihale metinleriyle eşleştiren;
uygunluğu, eksik koşulları ve başvuru yapılabilirliğini **açıklanabilir** biçimde skorlayan modüler platform.

> Hazırlayan: TalentHub İnsan Kaynakları Danışmanlık ve Sistem Tasarımı
> Proje takvimi: 12 ay (01.06.2026 – 31.05.2027)

---

## Neden "sadece bir ilan takip sistemi" değil

| Klasik ilan portalı | GOVAI |
|---|---|
| Herkese aynı listeyi gösterir | Firmanın kendi verisiyle eşleşenleri seçer |
| "Uygun / değil" der | **Neden** uygun, **hangi koşul** eksik, eksik kapanırsa skor **ne kadar** artar |
| Manuel veri girişi ister | ERP/İK/muhasebe verisini otomatik alır |
| Sonuç üretir | **Aksiyon** üretir: eksik belge listesi, kapatılacak koşullar, öncelik sırası |

---

## Hızlı başlangıç

### Docker ile (önerilen)

```bash
cd deploy && cp .env.example .env && docker compose up -d --build
```

`.env` içinde en az `POSTGRES_PASSWORD`, `RABBITMQ_PASSWORD` ve `JWT_SIGNING_KEY` doldurulmalıdır.
İlk kurulumda demo verisi için `SEED_ENABLED=true` ve `SEED_ADMIN_PASSWORD=...` verin.

Ardından:

| Servis | Adres |
|---|---|
| Yönetim paneli | http://localhost:5180 |
| API + Swagger | http://localhost:8080/swagger |
| Sağlık kontrolü | http://localhost:8080/health |
| RabbitMQ yönetimi | http://localhost:15672 |

### Yerel geliştirme

Yalnızca altyapı servislerini Docker'da çalıştırıp uygulamaları yerelde ayağa kaldırın:

```bash
docker compose -f deploy/docker-compose.yml up -d postgres redis rabbitmq
```

Backend:

```bash
dotnet run --project src/GovAI.Api
```

Web paneli:

```bash
cd web && npm install && npm run dev
```

Python worker'ları:

```bash
cd workers && python -m venv .venv && .venv/Scripts/pip install -e ".[dev]" && .venv/Scripts/govai-collector --once
```

---

## Depo yapısı

```
GOVAI/
├── src/
│   ├── GovAI.Domain/          Kural motoru, skorlama, entity'ler — hiçbir dış bağımlılık yok
│   ├── GovAI.Application/     Use-case servisleri, DTO'lar, port arayüzleri (MediatR YOK)
│   ├── GovAI.Persistence/     EF Core, PostgreSQL, repository'ler, migration'lar
│   ├── GovAI.Infrastructure/  OpenAI, Redis, RabbitMQ, JWT, PDF/Excel
│   └── GovAI.Api/             REST API: 8 endpoint grubu, Swagger, Serilog, audit
├── tests/                     Domain ve Application testleri (xUnit)
├── workers/                   Python: collector, parser, rule extractor, scheduler
├── web/                       React 19 + TypeScript + Vite yönetim paneli
├── deploy/                    docker-compose, .env örneği, Postgres init
└── docs/                      Mimari, veri modeli, API, skorlama, ADR'ler, yol haritası
```

Mimari kararların gerekçeleri için [docs/architecture.md](docs/architecture.md) ve
[docs/adr/](docs/adr/) dizinine bakın.

---

## Katmanlı mimari (Onion)

```
                 ┌──────────────────────────────┐
                 │        GovAI.Api             │  controller, auth, Swagger
                 ├──────────────────────────────┤
   ┌─────────────┤   Persistence  Infrastructure├─────────────┐
   │             ├──────────────────────────────┤             │
   │             │      GovAI.Application       │             │
   │             ├──────────────────────────────┤             │
   │             │        GovAI.Domain          │             │
   │             └──────────────────────────────┘             │
   │                                                          │
   └── PostgreSQL / Redis / RabbitMQ / OpenAI ────────────────┘
```

Bağımlılıklar **daima içe** doğrudur. `Domain` hiçbir şeye bağımlı değildir;
`Application` yalnızca `Domain`'e bağlıdır ve dış dünyayı arayüzlerle (port) tanır.
`Persistence` ve `Infrastructure` bu arayüzleri uygular.

**MediatR kullanılmaz.** Her use-case düz bir servis sınıfıdır (`CompanyProfileService`,
`EligibilityService`, …) ve controller onu doğrudan çağırır.

---

## Skorlama

Nihai fırsat skoru yedi boyutun ağırlıklı toplamıdır:

```
Final Opportunity Score =
    0.25 × sectorMatch            (sektörel / NACE eşleşmesi)
  + 0.20 × financialFit           (ciro, bilanço, özkaynak)
  + 0.15 × employeeFit            (çalışan sayısı ve personel yapısı)
  + 0.15 × documentReadiness      (belge hazır olma düzeyi)
  + 0.10 × regionalCompliance     (lokasyon / NUTS-2 uygunluğu)
  + 0.10 × technicalQualification (teknik yeterlilik, deneyim)
  + 0.05 × timingScore            (başvuru takvimi)
```

Ağırlıklar destek türüne göre değişir — ihalede teknik yeterlilik, istihdam teşvikinde
personel yapısı öne çıkar (`ScoreWeights.For(category)`).

Ayrıntı ve tasarım gerekçeleri: [docs/scoring.md](docs/scoring.md).

### Üç davranış garantisi

1. **Deterministik.** Aynı firma + aynı çağrı her zaman aynı skoru üretir. Kararı AI vermez.
2. **Açıklanabilir.** Her boyut kendi puanını, ağırlığını, katkısını ve gerekçesini taşır;
   her kural, çağrı metnindeki dayanağını (`sourceExcerpt`) saklar.
3. **Eksik veri elemez.** Doldurulmamış bir profil firmayı "uygun değil" yapmaz;
   "belirsiz" sonucu üretir ve hangi alanın eksik olduğunu söyler.

---

## Yapay zekânın rolü

AI, sistemin karar vericisi **değil**, iki noktada yardımcı katmandır:

| Nerede | Ne yapar | Ne yapmaz |
|---|---|---|
| Kural çıkarımı | Serbest formatlı resmî metinden koşul taslağı üretir | Kuralı üretime sokmaz — danışman onayına düşer |
| Yönetici özeti | Hesaplanmış skoru Türkçe anlatıya çevirir | Skoru veya kararı değiştirmez |

Alan adları beyaz listeyle kısıtlıdır; model listede olmayan bir alan üretirse o kural sessizce elenir.
**OpenAI anahtarı olmadan da sistem çalışır**: kural çıkarımı deterministik kalıplara düşer,
özetler kural tabanlı metinle üretilir.

---

## Teknoloji yığını

| Katman | Teknoloji | Not |
|---|---|---|
| Backend | C# / .NET 10 (LTS) | Proje dosyasında .NET 8 yazıyordu; kurulu SDK ve LTS takvimi nedeniyle 10'a alındı ([ADR-0004](docs/adr/0004-net10-hedefi.md)) |
| Veri tabanı | PostgreSQL 17 | jsonb alanları, snake_case şema, `govai` şeması |
| Önbellek | Redis 7 | Erişilemezse sistem çalışmaya devam eder |
| Kuyruk | RabbitMQ 4 | Topic exchange + dead-letter kuyrukları |
| Worker | Python 3.12 | Tarama, PDF/HTML ayrıştırma, kural çıkarımı |
| Ön yüz | React 19 + TypeScript + Vite | TanStack Query, Recharts |
| AI | OpenAI API | İsteğe bağlı; olmadan da çalışır |
| Rapor | QuestPDF + ClosedXML | PDF ve Excel çıktıları |

---

## Testler ve doğrulama

```bash
dotnet test
```

```bash
cd workers && .venv/Scripts/python -m pytest -q && .venv/Scripts/python -m ruff check .
```

```bash
cd web && npm run lint && npm run typecheck && npm run build
```

CI aynı üç adımı ve Docker imaj derlemelerini `.github/workflows/ci.yml` içinde çalıştırır.

---

## Veritabanı migration'ları

```bash
dotnet dotnet-ef migrations add <Ad> --project src/GovAI.Persistence --startup-project src/GovAI.Api
```

```bash
dotnet dotnet-ef database update --project src/GovAI.Persistence --startup-project src/GovAI.Api
```

Geliştirmede `Database:AutoMigrate=true` ile uygulama açılışında otomatik uygulanır.
Üretimde bu ayar kapalı tutulup migration ayrı bir dağıtım adımı olarak çalıştırılmalıdır.

---

## Güvenlik ve KVKK

- JWT tabanlı kimlik doğrulama; kurumsal SSO (OIDC) için `AppUser.ExternalSubjectId` hazır.
- Beş rol: `SuperAdmin`, `CompanyManager`, `OperationUser`, `Consultant`, `ReadOnly`.
- Danışman rolü belirli firmalarla sınırlandırılabilir (`scoped_companies` claim'i).
- Parolalar PBKDF2-HMAC-SHA256 (210.000 tur) ile saklanır; 5 hatalı denemede 15 dakika kilit.
- Silme işlemleri fiziksel değil mantıksaldır (soft delete) — izlenebilirlik korunur.
- Her değişiklik `govai.audit_log` tablosuna kullanıcı, IP, korelasyon kimliği ve zaman damgasıyla yazılır.
- Worker'lar veritabanına doğrudan erişmez; yalnızca REST API üzerinden konuşur.

---

## Yol haritası

12 aylık iş-zaman planı: [docs/roadmap.md](docs/roadmap.md).
