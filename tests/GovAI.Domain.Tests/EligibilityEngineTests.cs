using GovAI.Domain.Common;
using GovAI.Domain.Companies;
using GovAI.Domain.Eligibility;
using GovAI.Domain.Opportunities;
using GovAI.Domain.Scoring;

namespace GovAI.Domain.Tests;

/// <summary>
/// Kural motorunun davranış sözleşmesi. Bu testler ürünün en kritik iddiasını korur:
/// aynı firma + aynı çağrı her zaman aynı, açıklanabilir sonucu üretir.
/// </summary>
public class EligibilityEngineTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Engellenen_kosul_saglanmazsa_firma_elenir_ve_skor_sifirlanir()
    {
        var company = BuildCompany(employeeCount: 4);
        var opportunity = BuildOpportunity(minEmployeeCount: 10);

        var outcome = EligibilityEngine.Evaluate(company, opportunity, Now);

        Assert.Equal(EligibilityVerdict.NotEligible, outcome.Verdict);
        Assert.Equal(0m, outcome.Score.FinalScore);
        Assert.True(outcome.Score.HasBlockingFailure);
        Assert.Single(outcome.BlockingFailures);
    }

    [Fact]
    public void Tum_kosullar_saglandiginda_firma_uygun_bulunur()
    {
        var company = BuildCompany(employeeCount: 25);
        var opportunity = BuildOpportunity(minEmployeeCount: 10);

        var outcome = EligibilityEngine.Evaluate(company, opportunity, Now);

        Assert.Equal(EligibilityVerdict.Eligible, outcome.Verdict);
        Assert.True(outcome.Score.FinalScore > 80m, $"Beklenenden düşük skor: {outcome.Score.FinalScore}");
        Assert.Empty(outcome.BlockingFailures);
    }

    [Fact]
    public void Eksik_firma_verisi_karari_belirsiz_yapar_ve_veri_boslugu_raporlanir()
    {
        var company = new Company(Guid.CreateVersion7(), "Veri Eksik A.Ş.", "9999999999", LegalType.LimitedCompany);
        var opportunity = BuildOpportunity(minEmployeeCount: 10);

        var outcome = EligibilityEngine.Evaluate(company, opportunity, Now);

        Assert.Equal(EligibilityVerdict.Indeterminate, outcome.Verdict);
        Assert.NotEmpty(outcome.DataGaps);
        Assert.All(outcome.DataGaps, gap => Assert.NotNull(gap.SuggestedAction));
    }

    [Fact]
    public void Skor_agirliklarinin_toplami_daima_bir_olmalidir()
    {
        foreach (var category in Enum.GetValues<SupportCategory>())
        {
            var weights = ScoreWeights.For(category);
            var total = weights.SectorMatch + weights.FinancialFit + weights.EmployeeFit
                        + weights.DocumentReadiness + weights.RegionalCompliance
                        + weights.TechnicalQualification + weights.Timing;

            Assert.Equal(1m, total, precision: 4);
        }
    }

    [Fact]
    public void Varsayilan_agirliklar_proje_dosyasindaki_formulle_ayni_olmalidir()
    {
        var weights = ScoreWeights.Default;

        Assert.Equal(0.25m, weights.SectorMatch);
        Assert.Equal(0.20m, weights.FinancialFit);
        Assert.Equal(0.15m, weights.EmployeeFit);
        Assert.Equal(0.15m, weights.DocumentReadiness);
        Assert.Equal(0.10m, weights.RegionalCompliance);
        Assert.Equal(0.10m, weights.TechnicalQualification);
        Assert.Equal(0.05m, weights.Timing);
    }

    [Fact]
    public void Ayni_girdi_ayni_skoru_uretmelidir()
    {
        var company = BuildCompany(employeeCount: 18);
        var opportunity = BuildOpportunity(minEmployeeCount: 10);

        var first = EligibilityEngine.Evaluate(company, opportunity, Now);
        var second = EligibilityEngine.Evaluate(company, opportunity, Now);

        Assert.Equal(first.Score.FinalScore, second.Score.FinalScore);
        Assert.Equal(first.Verdict, second.Verdict);
    }

    [Fact]
    public void Suresi_dolmus_cagri_zamanlama_boyutundan_puan_alamaz()
    {
        var company = BuildCompany(employeeCount: 25);
        var opportunity = BuildOpportunity(minEmployeeCount: 10);
        opportunity.SetSchedule(Now.AddDays(-90), Now.AddDays(-1));

        var outcome = EligibilityEngine.Evaluate(company, opportunity, Now);

        Assert.Equal(0m, outcome.Score.ScoreOf(RuleDimension.Timing));
    }

    [Fact]
    public void Eksik_zorunlu_belge_karari_sartli_uygun_yapar()
    {
        var company = BuildCompany(employeeCount: 25);
        var opportunity = BuildOpportunity(minEmployeeCount: 10);
        opportunity.ReplaceDocumentChecklist(
        [
            new DocumentRequirement("SGK_BORCU_YOKTUR", "SGK Borcu Yoktur Yazısı", isMandatory: true, "SGK")
        ]);

        var outcome = EligibilityEngine.Evaluate(company, opportunity, Now);

        Assert.Equal(EligibilityVerdict.ConditionallyEligible, outcome.Verdict);
        Assert.Contains(outcome.DocumentChecklist, d => d.Status == DocumentStatus.Missing && d.Action is not null);
    }

    [Fact]
    public void Danisman_onayi_skor_guvenini_yukseltir()
    {
        var company = BuildCompany(employeeCount: 25);

        var unreviewed = BuildOpportunity(minEmployeeCount: 10);
        var reviewed = BuildOpportunity(minEmployeeCount: 10);
        reviewed.MarkReviewed();

        var before = EligibilityEngine.Evaluate(company, unreviewed, Now).Score.Confidence;
        var after = EligibilityEngine.Evaluate(company, reviewed, Now).Score.Confidence;

        Assert.True(after > before, $"Onay sonrası güven artmalıydı: {before} → {after}");
    }

    private static Company BuildCompany(int employeeCount)
    {
        var company = new Company(Guid.CreateVersion7(), "Test Üretim A.Ş.", "1112223334", LegalType.JointStockCompany);
        company.UpdateIdentity("Test Üretim A.Ş.", LegalType.JointStockCompany, new DateOnly(2015, 1, 1));

        company.UpdateWorkforce(new Workforce(
            employeeCount,
            womenEmployeeCount: Math.Max(1, employeeCount / 3),
            youngEmployeeCount: Math.Max(1, employeeCount / 4),
            rAndDEmployeeCount: Math.Max(1, employeeCount / 6),
            disabledEmployeeCount: 0));

        company.UpdateFinancials(new Financials(50_000_000m, 30_000_000m, 12_000_000m, 8_000_000m, "TRY", 2025));
        company.UpdateFlags(exportFlag: true, technologyFlag: true, previousSuccessfulApplications: 1);

        company.ReplaceNaceCodes([new CompanyNaceCode("2562", isPrimary: true)]);
        company.ReplaceLocations([new CompanyLocation("Mersin", "Yenişehir", "TR62", isHeadquarters: true)]);

        return company;
    }

    private static Opportunity BuildOpportunity(int minEmployeeCount)
    {
        var opportunity = new Opportunity(
            Guid.CreateVersion7(),
            SourceType.DevelopmentAgency,
            SupportCategory.Grant,
            "Test Destek Programı",
            "Test Ajansı",
            Now.AddDays(-10));

        opportunity.SetSchedule(Now.AddDays(-10), Now.AddDays(45));

        opportunity.ReplaceRules(
        [
            new OpportunityRule(
                "Workforce.EmployeeCount", RuleOperator.GreaterThanOrEqual, minEmployeeCount.ToString(),
                RuleDimension.Employment, RuleSeverity.Blocking, $"Asgari {minEmployeeCount} çalışan."),
            new OpportunityRule(
                "Company.NaceCodes", RuleOperator.NaceMatch, "25,26",
                RuleDimension.Sector, RuleSeverity.Blocking, "İmalat NACE kodları."),
            new OpportunityRule(
                "Company.Nuts2Codes", RuleOperator.ContainsAny, "TR62",
                RuleDimension.Region, RuleSeverity.Blocking, "TR62 bölgesi."),
            new OpportunityRule(
                "Financials.AnnualRevenue", RuleOperator.LessThanOrEqual, "250000000",
                RuleDimension.Financial, RuleSeverity.Blocking, "KOBİ ciro sınırı.")
        ], extractionConfidence: 0.8m);

        return opportunity;
    }
}
