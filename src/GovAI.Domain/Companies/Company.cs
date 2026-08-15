using GovAI.Domain.Common;

namespace GovAI.Domain.Companies;

/// <summary>
/// Kurumsal Profil Motoru'nun (Modül 3) çıktısı olan firma kartı.
/// ERP, İK ve muhasebe sistemlerinden gelen veri bu tek modelde birleştirilir.
/// Teknik dokümandaki <c>CompanyProfile</c> yapısının kalıcı karşılığıdır.
/// </summary>
public class Company : AggregateRoot, IAuditable, ISoftDeletable, ITenantScoped
{
    private readonly List<CompanyLocation> _locations = [];
    private readonly List<CompanyCertificate> _certificates = [];
    private readonly List<CompanyInvestment> _activeInvestments = [];
    private readonly List<CompanyNaceCode> _naceCodes = [];

    private Company()
    {
    }

    public Company(Guid tenantId, string legalName, string taxNumber, LegalType legalType)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(legalName), "Firma unvanı zorunludur.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(taxNumber), "Vergi numarası zorunludur.");

        TenantId = tenantId;
        LegalName = legalName.Trim();
        TaxNumber = taxNumber.Trim();
        LegalType = legalType;
    }

    public Guid TenantId { get; set; }

    public string LegalName { get; private set; } = string.Empty;

    public string TaxNumber { get; private set; } = string.Empty;

    public LegalType LegalType { get; private set; }

    /// <summary>Kuruluş tarihi; bazı çağrılar asgari faaliyet süresi arar.</summary>
    public DateOnly? FoundedOn { get; private set; }

    public Workforce Workforce { get; private set; } = Workforce.Empty;

    public Financials Financials { get; private set; } = Financials.Empty;

    /// <summary>İhracat yapıyor mu? (<c>exportFlag</c>)</summary>
    public bool ExportFlag { get; private set; }

    /// <summary>Teknopark / Ar-Ge merkezi / teknoloji firması statüsü var mı? (<c>technologyFlag</c>)</summary>
    public bool TechnologyFlag { get; private set; }

    /// <summary>Daha önce kamu destek programına başvurup kabul aldı mı? Geçmiş başvuru kabiliyeti göstergesi.</summary>
    public int PreviousSuccessfulApplications { get; private set; }

    public IReadOnlyCollection<CompanyNaceCode> NaceCodes => _naceCodes.AsReadOnly();

    public IReadOnlyCollection<CompanyLocation> Locations => _locations.AsReadOnly();

    public IReadOnlyCollection<CompanyCertificate> Certificates => _certificates.AsReadOnly();

    public IReadOnlyCollection<CompanyInvestment> ActiveInvestments => _activeInvestments.AsReadOnly();

    /// <summary>ERP eşitlemesinin en son başarıyla tamamlandığı an.</summary>
    public DateTimeOffset? LastSyncedAt { get; private set; }

    /// <summary>Profilin kaçıncı sürümü olduğu; her anlamlı değişiklikte artar ve skor yeniden hesaplanır.</summary>
    public int ProfileVersion { get; private set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>KOBİ ölçeği; çalışan sayısı ve ciro eşiklerinden türetilir (2003/361/EC uyarlaması).</summary>
    public EnterpriseSize Size => Workforce.EmployeeCount switch
    {
        < 10 => Financials.AnnualRevenue <= 3_000_000m ? EnterpriseSize.Micro : EnterpriseSize.Small,
        < 50 => Financials.AnnualRevenue <= 25_000_000m ? EnterpriseSize.Small : EnterpriseSize.Medium,
        < 250 => EnterpriseSize.Medium,
        _ => EnterpriseSize.Large
    };

    public string? PrimaryNaceCode => _naceCodes.FirstOrDefault(n => n.IsPrimary)?.Code
                                      ?? _naceCodes.FirstOrDefault()?.Code;

    public void UpdateIdentity(string legalName, LegalType legalType, DateOnly? foundedOn)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(legalName), "Firma unvanı zorunludur.");
        LegalName = legalName.Trim();
        LegalType = legalType;
        FoundedOn = foundedOn;
        BumpVersion();
    }

    public void UpdateWorkforce(Workforce workforce)
    {
        Workforce = workforce;
        BumpVersion();
    }

    public void UpdateFinancials(Financials financials)
    {
        Financials = financials;
        BumpVersion();
    }

    public void UpdateFlags(bool exportFlag, bool technologyFlag, int previousSuccessfulApplications)
    {
        DomainException.ThrowIf(previousSuccessfulApplications < 0, "Geçmiş başvuru sayısı negatif olamaz.");
        ExportFlag = exportFlag;
        TechnologyFlag = technologyFlag;
        PreviousSuccessfulApplications = previousSuccessfulApplications;
        BumpVersion();
    }

    public void ReplaceNaceCodes(IEnumerable<CompanyNaceCode> codes)
    {
        _naceCodes.Clear();
        _naceCodes.AddRange(codes);
        DomainException.ThrowIf(_naceCodes.Count(c => c.IsPrimary) > 1, "Yalnızca bir NACE kodu birincil olabilir.");
        BumpVersion();
    }

    public void ReplaceLocations(IEnumerable<CompanyLocation> locations)
    {
        _locations.Clear();
        _locations.AddRange(locations);
        BumpVersion();
    }

    public void ReplaceCertificates(IEnumerable<CompanyCertificate> certificates)
    {
        _certificates.Clear();
        _certificates.AddRange(certificates);
        BumpVersion();
    }

    public void ReplaceInvestments(IEnumerable<CompanyInvestment> investments)
    {
        _activeInvestments.Clear();
        _activeInvestments.AddRange(investments);
        BumpVersion();
    }

    public void MarkSynced(DateTimeOffset syncedAt) => LastSyncedAt = syncedAt;

    /// <summary>Belirtilen tarihte geçerli olan sertifikaların kodlarını döner.</summary>
    public IReadOnlySet<string> ValidCertificateCodes(DateOnly asOf) =>
        _certificates
            .Where(c => c.ValidUntil is null || c.ValidUntil >= asOf)
            .Select(c => c.Code.ToUpperInvariant())
            .ToHashSet();

    private void BumpVersion() => ProfileVersion++;
}
