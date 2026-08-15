# ADR-0002: Yapay zekâ karar verici değil, yardımcı katman

- **Durum:** Kabul edildi
- **Tarih:** 2026-08-15

## Bağlam

Proje dosyası şunu açıkça belirtiyor: *"GOVAI'de yapay zekâ, sistemin tamamının yerine geçen kara
kutu bir karar verici olarak değil; kurumsal veriyi anlamlandıran ve kural motorunu güçlendiren
yardımcı katman olarak tasarlanmalıdır."*

Bu yalnızca bir tercih değil, ürün gereksinimidir: teşvik ve ihale başvuruları hukuki sonuç doğurur.
"Model böyle dedi" savunulabilir bir gerekçe değildir.

## Karar

AI iki noktada, **karar dışında** kullanılır:

1. **Kural çıkarımı** — serbest formatlı resmî metinden yapılandırılmış koşul *taslağı* üretir.
   Çıktı danışman onayına düşer; onaylanmamış kuralların skor güveni düşüktür.
2. **Yönetici özeti** — *zaten hesaplanmış* skoru Türkçe anlatıya çevirir. Sayıları değiştiremez.

Skoru ve kararı her zaman `EligibilityEngine` üretir: deterministik, dış çağrısız, tekrarlanabilir.

## Uygulama korumaları

- **Alan beyaz listesi.** Model yalnızca `CompanyFieldResolver.SupportedFields` içindeki alan
  adlarını kullanabilir; listede olmayan alan üreten kurallar sessizce elenir ve loglanır.
- **JSON şeması zorunlu.** `response_format: json_object` ve `temperature: 0`.
- **Güven değeri saklanır.** Her kural kendi `Confidence` değerini, çağrı ise toplam
  `RuleExtractionConfidence` değerini taşır.
- **Kaynak alıntısı zorunlu.** Her kural `SourceExcerpt` ile metindeki dayanağını saklar.
- **Elle düzeltme korunur.** `IsManuallyOverridden` işaretli kurallar sonraki otomatik
  çıkarımlarda ezilmez.
- **AI olmadan çalışır.** Anahtar yoksa kural çıkarımı deterministik kalıplara, özet üretimi
  kural tabanlı metne düşer. Skorlar hiç etkilenmez.

## Sonuçlar

**Olumlu:** Denetlenebilirlik, tekrarlanabilirlik, AI sağlayıcısından bağımsızlık,
maliyet öngörülebilirliği (skorlama AI çağrısı yapmaz).

**Olumsuz:** Kural çıkarımı, uçtan uca LLM yaklaşımına kıyasla daha fazla mühendislik gerektirir —
kalıp kütüphanesi, beyaz liste bakımı ve danışman onay ekranı. Bu, ürünün ana savunma hattı
olduğu için kabul edilen bir maliyettir.
