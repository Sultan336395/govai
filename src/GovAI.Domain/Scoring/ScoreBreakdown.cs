using GovAI.Domain.Common;

namespace GovAI.Domain.Scoring;

/// <summary>
/// Tek bir skor boyutunun sonucu ve gerekçesi. "Kara kutu değil" ilkesinin karşılığı:
/// her boyut kendi puanını, ağırlığını, nihai skora katkısını ve dayanağını taşır.
/// </summary>
public sealed record DimensionScore
{
    public required RuleDimension Dimension { get; init; }

    /// <summary>0..1 aralığında boyut puanı.</summary>
    public required decimal Value { get; init; }

    public required decimal Weight { get; init; }

    /// <summary>Nihai skora katkı = <see cref="Value"/> × <see cref="Weight"/>.</summary>
    public decimal Contribution => Math.Round(Value * Weight, 4);

    /// <summary>Bu boyutu belirleyen kural sayısı; 0 ise puan varsayımdan gelmiştir.</summary>
    public required int EvaluatedRuleCount { get; init; }

    /// <summary>Puanın nasıl oluştuğunun Türkçe açıklaması.</summary>
    public required string Rationale { get; init; }

    /// <summary>Bu boyutta veri eksikliği nedeniyle karar verilemeyen kural sayısı.</summary>
    public int UnknownRuleCount { get; init; }
}

/// <summary>
/// Skorlama Servisi'nin (Modül 6) tam çıktısı.
/// </summary>
public sealed record ScoreBreakdown
{
    public required IReadOnlyList<DimensionScore> Dimensions { get; init; }

    public required ScoreWeights Weights { get; init; }

    /// <summary>0..100 aralığında nihai fırsat skoru.</summary>
    public required decimal FinalScore { get; init; }

    /// <summary>Engelleyici (Blocking) bir koşul sağlanmadığı için skor sıfırlandı mı?</summary>
    public required bool HasBlockingFailure { get; init; }

    /// <summary>
    /// Skorun ne kadar sağlam veriye dayandığı (0..1). Veri eksikliği ve düşük kural çıkarım güveni
    /// bu değeri düşürür; kullanıcıya "bu skora ne kadar güvenebilirim" bilgisini verir.
    /// </summary>
    public required decimal Confidence { get; init; }

    public decimal ScoreOf(RuleDimension dimension) =>
        Dimensions.FirstOrDefault(d => d.Dimension == dimension)?.Value ?? 0m;
}
