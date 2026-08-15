using GovAI.Domain.Common;

namespace GovAI.Domain.Assessments;

/// <summary>
/// Senaryo ve Simülasyon Modülü'nün (Modül 7) kaydı.
/// "Personel sayısı 15'e çıkarsa / ISO 9001 alınırsa fırsat havuzum nasıl değişir?" sorusunun cevabı.
/// </summary>
public class ScenarioSimulation : AggregateRoot, IAuditable, ITenantScoped
{
    private readonly List<ScenarioImpact> _impacts = [];

    private ScenarioSimulation()
    {
    }

    public ScenarioSimulation(Guid tenantId, Guid companyId, string name, string changesJson)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(name), "Senaryo adı zorunludur.");

        TenantId = tenantId;
        CompanyId = companyId;
        Name = name.Trim();
        ChangesJson = changesJson;
    }

    public Guid TenantId { get; set; }

    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Uygulanan değişikliklerin JSON gösterimi (ör. <c>{"Workforce.EmployeeCount": 15}</c>).</summary>
    public string ChangesJson { get; private set; } = "{}";

    /// <summary>Senaryo öncesi uygun fırsat sayısı.</summary>
    public int BaselineEligibleCount { get; private set; }

    /// <summary>Senaryo sonrası uygun fırsat sayısı.</summary>
    public int SimulatedEligibleCount { get; private set; }

    public decimal BaselineAverageScore { get; private set; }

    public decimal SimulatedAverageScore { get; private set; }

    public IReadOnlyCollection<ScenarioImpact> Impacts => _impacts.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public decimal ScoreDelta => Math.Round(SimulatedAverageScore - BaselineAverageScore, 2);

    public int EligibleCountDelta => SimulatedEligibleCount - BaselineEligibleCount;

    public void RecordResult(
        int baselineEligibleCount,
        int simulatedEligibleCount,
        decimal baselineAverageScore,
        decimal simulatedAverageScore,
        IEnumerable<ScenarioImpact> impacts)
    {
        BaselineEligibleCount = baselineEligibleCount;
        SimulatedEligibleCount = simulatedEligibleCount;
        BaselineAverageScore = Math.Round(baselineAverageScore, 2);
        SimulatedAverageScore = Math.Round(simulatedAverageScore, 2);
        _impacts.Clear();
        _impacts.AddRange(impacts);
    }
}

/// <summary>Senaryonun tek bir fırsat üzerindeki etkisi.</summary>
public class ScenarioImpact : Entity
{
    private ScenarioImpact()
    {
    }

    public ScenarioImpact(
        Guid opportunityId,
        string opportunityTitle,
        decimal baselineScore,
        decimal simulatedScore,
        EligibilityVerdict baselineVerdict,
        EligibilityVerdict simulatedVerdict)
    {
        OpportunityId = opportunityId;
        OpportunityTitle = opportunityTitle;
        BaselineScore = baselineScore;
        SimulatedScore = simulatedScore;
        BaselineVerdict = baselineVerdict;
        SimulatedVerdict = simulatedVerdict;
    }

    public Guid ScenarioSimulationId { get; private set; }

    public Guid OpportunityId { get; private set; }

    public string OpportunityTitle { get; private set; } = string.Empty;

    public decimal BaselineScore { get; private set; }

    public decimal SimulatedScore { get; private set; }

    public EligibilityVerdict BaselineVerdict { get; private set; }

    public EligibilityVerdict SimulatedVerdict { get; private set; }

    public decimal Delta => Math.Round(SimulatedScore - BaselineScore, 2);

    /// <summary>Senaryo sayesinde uygun hâle gelen fırsatlar dashboard'da öne çıkarılır.</summary>
    public bool BecameEligible =>
        BaselineVerdict is EligibilityVerdict.NotEligible or EligibilityVerdict.ConditionallyEligible
        && SimulatedVerdict == EligibilityVerdict.Eligible;
}
