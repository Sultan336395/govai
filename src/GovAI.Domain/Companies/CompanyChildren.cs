using GovAI.Domain.Common;

namespace GovAI.Domain.Companies;

/// <summary>Firmanın faaliyet kodu. Sektörel eşleşmenin (<c>sectorMatch</c>) temel girdisidir.</summary>
public class CompanyNaceCode : Entity
{
    private CompanyNaceCode()
    {
    }

    public CompanyNaceCode(string code, bool isPrimary, string? description = null)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(code), "NACE kodu boş olamaz.");
        Code = NaceCode.Normalize(code);
        IsPrimary = isPrimary;
        Description = description;
    }

    public Guid CompanyId { get; private set; }

    /// <summary>Noktasız, büyük harfe normalize edilmiş NACE kodu (ör. "62.01" → "6201").</summary>
    public string Code { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public string? Description { get; private set; }
}

/// <summary>Firmanın bir tesisi/şubesi. Bölgesel uygunluk kontrolünde kullanılır.</summary>
public class CompanyLocation : Entity
{
    private CompanyLocation()
    {
    }

    public CompanyLocation(string city, string? district, string? nuts2Code, bool isHeadquarters, bool isInTechnopark = false)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(city), "İl bilgisi zorunludur.");
        City = city.Trim();
        District = district?.Trim();
        Nuts2Code = nuts2Code?.Trim().ToUpperInvariant();
        IsHeadquarters = isHeadquarters;
        IsInTechnopark = isInTechnopark;
    }

    public Guid CompanyId { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string? District { get; private set; }

    /// <summary>İstatistiki Bölge Birimi (ör. TR62 – Adana, Mersin). Kalkınma ajansı çağrılarında kritik.</summary>
    public string? Nuts2Code { get; private set; }

    public bool IsHeadquarters { get; private set; }

    /// <summary>Teknoloji Geliştirme Bölgesi içinde mi? Ar-Ge çağrılarında ek puan sağlar.</summary>
    public bool IsInTechnopark { get; private set; }
}

/// <summary>Firmanın sahip olduğu belge/sertifika. Belge hazır olma skorunu (<c>documentReadiness</c>) besler.</summary>
public class CompanyCertificate : Entity
{
    private CompanyCertificate()
    {
    }

    public CompanyCertificate(string code, string name, DateOnly? issuedOn, DateOnly? validUntil, string? documentUri = null)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(code), "Sertifika kodu zorunludur.");
        DomainException.ThrowIf(
            issuedOn is not null && validUntil is not null && validUntil < issuedOn,
            "Geçerlilik tarihi düzenlenme tarihinden önce olamaz.");

        Code = code.Trim().ToUpperInvariant();
        Name = string.IsNullOrWhiteSpace(name) ? Code : name.Trim();
        IssuedOn = issuedOn;
        ValidUntil = validUntil;
        DocumentUri = documentUri;
    }

    public Guid CompanyId { get; private set; }

    /// <summary>Standart kod (ör. ISO9001, ISO14001, TSE, CE, YETKILI_IHRACATCI).</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public DateOnly? IssuedOn { get; private set; }

    public DateOnly? ValidUntil { get; private set; }

    /// <summary>Belge dosyasının depolama adresi; PDF rapor eklerinde kullanılır.</summary>
    public string? DocumentUri { get; private set; }

    public bool IsValidOn(DateOnly asOf) => ValidUntil is null || ValidUntil >= asOf;
}

/// <summary>Devam eden veya planlanan yatırım. Yatırım teşviklerinde konu eşleşmesi için kullanılır.</summary>
public class CompanyInvestment : Entity
{
    private CompanyInvestment()
    {
    }

    public CompanyInvestment(string title, SupportCategory relatedCategory, decimal plannedBudget, DateOnly? plannedStart, DateOnly? plannedEnd)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "Yatırım başlığı zorunludur.");
        DomainException.ThrowIf(plannedBudget < 0, "Planlanan bütçe negatif olamaz.");

        Title = title.Trim();
        RelatedCategory = relatedCategory;
        PlannedBudget = plannedBudget;
        PlannedStart = plannedStart;
        PlannedEnd = plannedEnd;
    }

    public Guid CompanyId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public SupportCategory RelatedCategory { get; private set; }

    public decimal PlannedBudget { get; private set; }

    public DateOnly? PlannedStart { get; private set; }

    public DateOnly? PlannedEnd { get; private set; }
}
