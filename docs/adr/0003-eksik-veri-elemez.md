# ADR-0003: Eksik veri firmayı elemez

- **Durum:** Kabul edildi
- **Tarih:** 2026-08-15

## Bağlam

Kural motoru geliştirilirken bir test bu davranışı ortaya çıkardı: hiç doldurulmamış bir firma
profili, "asgari 10 çalışan" kuralı karşısında `EmployeeCount = 0` okuyup `NotEligible` üretiyordu.

Bu yanlıştır. `0` burada "sıfır çalışanı var" değil, "henüz girilmedi" anlamına gelir. Sistem,
sadece profili eksik olduğu için firmayı gerçekte uygun olduğu fırsatlardan mahrum bırakırdı —
üstelik kullanıcı bunun sebebini göremezdi.

## Karar

Eksik veri ile sıfır ayrı ele alınır. Doldurulmamış alan `Unknown` döner; `Unknown` sonuçlanan
kural firmayı elemez, 0.5 kısmi kredi alır ve **veri boşluğu** olarak raporlanır.

Bir çağrının kurallarının %40'ından fazlası veri eksikliğinden değerlendirilemiyorsa karar
`NotEligible` değil `Indeterminate` olur.

### Alan bazında kural

| Alan | Boş değer | Yorum |
|---|---|---|
| `Financials.*` | `0` | Girilmemiş → `Unknown` |
| `Workforce.*` | `EmployeeCount = 0` | Tüm personel alanları `Unknown` |
| `Company.NaceCodes` | boş küme | `Unknown` — her firmanın NACE kodu vardır |
| `Company.Cities`, `Nuts2Codes` | boş küme | `Unknown` — her firmanın adresi vardır |
| `Company.Certificates` | boş küme | **Bilinen değer** — "belgemiz yok" geçerli bir cevaptır |

Sertifika istisnası bilinçlidir: onu da `Unknown` saymak "ISO 9001 eksik, temin edin"
aksiyonunu yok ederdi — oysa bu ürünün ürettiği en somut çıktılardan biridir.

## Sonuçlar

**Olumlu:** Kullanıcı "neden uygun değilim" yerine "hangi veriyi girmeliyim" cevabını alır.
`CompanyProfileService.CalculateCompleteness` profil doluluğunu doğrudan gösterir; panelde
uyarı olarak sunulur.

**Olumsuz:** Boş profilli bir firma çok sayıda `Indeterminate` sonuç görür. Bu, sessiz bir
yanlış eleme yerine kullanıcıyı profili tamamlamaya yönlendiren açık bir sinyal olduğu için
tercih edilmiştir.

**Korunuyor:** `Eksik_firma_verisi_karari_belirsiz_yapar_ve_veri_boslugu_raporlanir` testi.
