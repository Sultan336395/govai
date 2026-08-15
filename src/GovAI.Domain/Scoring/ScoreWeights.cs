using GovAI.Domain.Common;

namespace GovAI.Domain.Scoring;

/// <summary>
/// Nihai fırsat skorunun ağırlıkları. Varsayılan değerler proje dosyasındaki formüldür:
/// <code>
/// Final Opportunity Score =
///     0.25 * sectorMatch
///   + 0.20 * financialFit
///   + 0.15 * employeeFit
///   + 0.15 * documentReadiness
///   + 0.10 * regionalCompliance
///   + 0.10 * technicalQualification
///   + 0.05 * timingScore
/// </code>
/// Ağırlıklar destek türüne göre özelleştirilebilir (ör. ihalede teknik yeterlilik daha ağır basar);
/// bu nedenle sabit değil, veri olarak taşınır.
/// </summary>
public sealed record ScoreWeights
{
    public static readonly ScoreWeights Default = new(0.25m, 0.20m, 0.15m, 0.15m, 0.10m, 0.10m, 0.05m);

    public ScoreWeights(
        decimal sectorMatch,
        decimal financialFit,
        decimal employeeFit,
        decimal documentReadiness,
        decimal regionalCompliance,
        decimal technicalQualification,
        decimal timing)
    {
        var total = sectorMatch + financialFit + employeeFit + documentReadiness
                    + regionalCompliance + technicalQualification + timing;

        DomainException.ThrowIf(
            Math.Abs(total - 1m) > 0.0001m,
            $"Skor ağırlıklarının toplamı 1.0 olmalıdır; gelen toplam {total}.");

        SectorMatch = sectorMatch;
        FinancialFit = financialFit;
        EmployeeFit = employeeFit;
        DocumentReadiness = documentReadiness;
        RegionalCompliance = regionalCompliance;
        TechnicalQualification = technicalQualification;
        Timing = timing;
    }

    public decimal SectorMatch { get; init; }
    public decimal FinancialFit { get; init; }
    public decimal EmployeeFit { get; init; }
    public decimal DocumentReadiness { get; init; }
    public decimal RegionalCompliance { get; init; }
    public decimal TechnicalQualification { get; init; }
    public decimal Timing { get; init; }

    /// <summary>İhale senaryosunda teknik yeterlilik ve belge hazırlığı öne çıkar.</summary>
    public static ScoreWeights ForTender() => new(0.15m, 0.20m, 0.10m, 0.20m, 0.10m, 0.20m, 0.05m);

    /// <summary>İstihdam teşviklerinde personel yapısı belirleyicidir.</summary>
    public static ScoreWeights ForEmploymentIncentive() => new(0.20m, 0.15m, 0.30m, 0.15m, 0.10m, 0.05m, 0.05m);

    public static ScoreWeights For(SupportCategory category) => category switch
    {
        SupportCategory.Tender => ForTender(),
        SupportCategory.EmploymentIncentive => ForEmploymentIncentive(),
        _ => Default
    };

    public decimal WeightOf(RuleDimension dimension) => dimension switch
    {
        RuleDimension.Sector => SectorMatch,
        RuleDimension.Financial => FinancialFit,
        RuleDimension.Employment => EmployeeFit,
        RuleDimension.Documentation => DocumentReadiness,
        RuleDimension.Region => RegionalCompliance,
        RuleDimension.TechnicalQualification => TechnicalQualification,
        RuleDimension.Timing => Timing,
        _ => 0m
    };
}
