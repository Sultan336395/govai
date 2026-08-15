# ADR-0004: .NET 8 yerine .NET 10 hedeflenmesi

- **Durum:** Kabul edildi
- **Tarih:** 2026-08-15

## Bağlam

Proje dosyası teknoloji yığınında **C# / .NET 8 / ASP.NET Core** belirtiyor. Ancak:

- Geliştirme makinesinde kurulu tek SDK **10.0.302**.
- .NET 8'in destek süresi **Kasım 2026**'da bitiyor — projenin 6. ayında.
- .NET 10 LTS'tir ve desteği **Kasım 2028**'e kadar sürer; projenin tamamını ve sonrasını kapsar.

## Karar

Tüm .NET projeleri `net10.0` hedefler. Hedef çerçeve `Directory.Build.props` içinde tek yerde tanımlıdır.

## Gerekçe

- .NET 8 seçilseydi proje daha canlıya çıkmadan destek dışı bir sürüme dayanmış olurdu.
- Kod tabanı .NET 8 ile de derlenebilir; sürüme özgü tek belirgin bağımlılık `Guid.CreateVersion7()`
  (.NET 9+). Gerekirse `Guid.NewGuid()` ile değiştirilebilir — bunun bedeli indeks parçalanmasıdır.
- ASP.NET Core, EF Core ve Npgsql'in 10.x sürümleri kararlı ve birbiriyle uyumludur.

## Sonuçlar

**Olumlu:** Proje ömrü boyunca desteklenen LTS; güncel EF Core ve Npgsql özellikleri.

**Olumsuz:** Proje dosyasındaki metinle birebir örtüşmüyor. Teknopark/teşvik başvurularında
sunulacak dokümanlarda "C# / .NET (LTS)" ifadesi kullanılması ve bu ADR'ye atıf yapılması önerilir.

**Geri dönüş:** `Directory.Build.props` içinde `<TargetFramework>net8.0</TargetFramework>` ve
`Guid.CreateVersion7()` çağrılarının değiştirilmesi yeterlidir.
