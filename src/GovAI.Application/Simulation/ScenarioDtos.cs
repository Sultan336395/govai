using GovAI.Domain.Common;

namespace GovAI.Application.Simulation;

/// <summary>
/// "What-if" senaryosunun girdisi. Null bırakılan alanlar firmanın mevcut değerini korur.
/// </summary>
public sealed record ScenarioRequest
{
    public required string Name { get; init; }

    public int? EmployeeCount { get; init; }

    public int? WomenEmployeeCount { get; init; }

    public int? YoungEmployeeCount { get; init; }

    public int? RAndDEmployeeCount { get; init; }

    public int? DisabledEmployeeCount { get; init; }

    public decimal? AnnualRevenue { get; init; }

    public decimal? BalanceSize { get; init; }

    public decimal? Equity { get; init; }

    public decimal? ExportRevenue { get; init; }

    public bool? ExportFlag { get; init; }

    public bool? TechnologyFlag { get; init; }

    /// <summary>Senaryoda alınacağı varsayılan sertifikalar (ör. ISO9001).</summary>
    public IReadOnlyList<string>? AddCertificateCodes { get; init; }

    /// <summary>Süresi dolacak / kaybedilecek sertifikalar.</summary>
    public IReadOnlyList<string>? RemoveCertificateCodes { get; init; }

    /// <summary>Yalnızca belirli destek türleri üzerinde çalıştır.</summary>
    public IReadOnlyList<SupportCategory>? Categories { get; init; }
}

public sealed record ScenarioImpactDto(
    Guid OpportunityId,
    string OpportunityTitle,
    SupportCategory SupportCategory,
    decimal BaselineScore,
    decimal SimulatedScore,
    decimal Delta,
    EligibilityVerdict BaselineVerdict,
    EligibilityVerdict SimulatedVerdict,
    bool BecameEligible);

public sealed record ScenarioResultDto(
    Guid? SimulationId,
    Guid CompanyId,
    string Name,
    int EvaluatedOpportunityCount,
    int BaselineEligibleCount,
    int SimulatedEligibleCount,
    decimal BaselineAverageScore,
    decimal SimulatedAverageScore,
    IReadOnlyList<ScenarioImpactDto> Impacts)
{
    public int EligibleCountDelta => SimulatedEligibleCount - BaselineEligibleCount;

    public decimal AverageScoreDelta => Math.Round(SimulatedAverageScore - BaselineAverageScore, 2);

    /// <summary>Senaryo sayesinde uygun hâle gelen fırsatlar — yöneticiye sunulacak asıl kazanım.</summary>
    public IReadOnlyList<ScenarioImpactDto> NewlyEligible => Impacts.Where(i => i.BecameEligible).ToList();
}

public sealed record ScenarioSummaryDto(
    Guid Id,
    string Name,
    int BaselineEligibleCount,
    int SimulatedEligibleCount,
    int EligibleCountDelta,
    decimal ScoreDelta,
    DateTimeOffset CreatedAt);
