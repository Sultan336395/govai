# ADR-0001: Onion mimarisi, MediatR kullanılmaması

- **Durum:** Kabul edildi
- **Tarih:** 2026-08-15

## Bağlam

GOVAI'nin ticari değeri kural motoru ve skorlama metodolojisindedir; proje dosyasında bunlar
fikri mülkiyet kapsamında korunması planlanan bileşenler olarak sayılmıştır. Bu mantığın
veritabanına, HTTP katmanına veya AI sağlayıcısına bağımlı olmaması gerekir.

Ayrıca ürünün ERP sağlayıcılarına gömülü modül olarak da satılması hedefleniyor — bu, çekirdek
mantığın taşınabilir olmasını gerektirir.

## Karar

**Onion mimarisi** benimsenmiştir. Bağımlılıklar daima içe doğrudur:

```
Api → Infrastructure / Persistence → Application → Domain
```

`Domain` hiçbir NuGet paketine bağımlı değildir. `Application` dış dünyayı yalnızca arayüzlerle
tanır; somut uygulamalar dış katmanlardadır.

**MediatR kullanılmaz.** Her use-case düz bir servis sınıfıdır ve controller onu doğrudan çağırır.

## Gerekçe

Onion için:
- Kural motoru saf C# olarak test edilebilir — 29 domain testi veritabanı veya HTTP olmadan çalışır.
- AI sağlayıcısı değişirse yalnızca `Infrastructure` katmanı etkilenir.
- ERP modülü senaryosunda `Domain` + `Application` ayrı bir kütüphane olarak paketlenebilir.

MediatR'a karşı:
- Bu ölçekte handler dolaylılığı, "bu isteği kim işliyor" sorusunu IDE'de tek tıkla
  cevaplanabilir olmaktan çıkarır.
- Pipeline behavior'ların sağladığı çapraz kesitler (loglama, doğrulama, denetim) ASP.NET Core'un
  kendi filtreleriyle zaten karşılanıyor (`AuditActionFilter`, `GlobalExceptionHandler`).
- Ekip küçük ve yığın izlerinin okunabilir olması hata ayıklama süresini doğrudan etkiliyor.

## Sonuçlar

**Olumlu:** Test edilebilirlik, taşınabilirlik, düz ve okunabilir çağrı zinciri.

**Olumsuz:** Repository arayüzleri elle yazılıyor; `IQueryable` sızdırılmadığı için karmaşık
filtreler repository metotlarına taşınıyor (`OpportunityQuery`, `AssessmentQuery` gibi sorgu nesneleri).
Bu, esneklik karşılığında ödenen bilinçli bedeldir.
