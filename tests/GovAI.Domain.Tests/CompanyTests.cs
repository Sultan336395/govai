using GovAI.Domain.Common;
using GovAI.Domain.Companies;

namespace GovAI.Domain.Tests;

public class CompanyTests
{
    [Fact]
    public void Profil_degistikce_surum_artar_ve_skorlar_bayatlar()
    {
        var company = NewCompany();
        var initial = company.ProfileVersion;

        company.UpdateWorkforce(new Workforce(20, 8, 5, 3, 0));

        Assert.True(company.ProfileVersion > initial);
    }

    [Fact]
    public void Kadin_calisan_sayisi_toplami_asamaz()
    {
        var exception = Assert.Throws<DomainException>(() => new Workforce(10, 12, 0, 0, 0));
        Assert.Contains("Kadın çalışan", exception.Message);
    }

    [Theory]
    [InlineData(5, 1_000_000, EnterpriseSize.Micro)]
    [InlineData(5, 10_000_000, EnterpriseSize.Small)]
    [InlineData(40, 10_000_000, EnterpriseSize.Small)]
    [InlineData(40, 90_000_000, EnterpriseSize.Medium)]
    [InlineData(120, 90_000_000, EnterpriseSize.Medium)]
    [InlineData(400, 900_000_000, EnterpriseSize.Large)]
    public void Kobi_olcegi_calisan_ve_cirodan_turetilir(int employees, decimal revenue, EnterpriseSize expected)
    {
        var company = NewCompany();
        company.UpdateWorkforce(new Workforce(employees, 0, 0, 0, 0));
        company.UpdateFinancials(new Financials(revenue, revenue, 1m, 0m, "TRY", 2025));

        Assert.Equal(expected, company.Size);
    }

    [Fact]
    public void Birden_fazla_birincil_nace_kodu_kabul_edilmez()
    {
        var company = NewCompany();

        Assert.Throws<DomainException>(() => company.ReplaceNaceCodes(
        [
            new CompanyNaceCode("2562", isPrimary: true),
            new CompanyNaceCode("6201", isPrimary: true)
        ]));
    }

    [Fact]
    public void Suresi_dolmus_sertifika_gecerli_sayilmaz()
    {
        var company = NewCompany();
        company.ReplaceCertificates(
        [
            new CompanyCertificate("ISO9001", "ISO 9001", new DateOnly(2020, 1, 1), new DateOnly(2024, 1, 1)),
            new CompanyCertificate("CE", "CE", new DateOnly(2023, 1, 1), null)
        ]);

        var valid = company.ValidCertificateCodes(new DateOnly(2026, 8, 15));

        Assert.DoesNotContain("ISO9001", valid);
        Assert.Contains("CE", valid);
    }

    [Fact]
    public void Ihracat_orani_cirodan_hesaplanir()
    {
        var financials = new Financials(100m, 50m, 20m, 25m, "TRY", 2025);
        Assert.Equal(0.25m, financials.ExportRatio);
    }

    private static Company NewCompany() =>
        new(Guid.CreateVersion7(), "Test A.Ş.", "1234567890", LegalType.JointStockCompany);
}
