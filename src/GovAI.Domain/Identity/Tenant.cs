using GovAI.Domain.Common;

namespace GovAI.Domain.Identity;

/// <summary>
/// Müşteri hesabı. Beyaz etiket ve danışmanlık senaryolarında bir kiracı birden çok firmayı yönetebilir
/// (danışmanlık şirketi → müşterileri). Tüm kiracıya bağlı veriler <see cref="ITenantScoped"/> ile filtrelenir.
/// </summary>
public class Tenant : AggregateRoot, IAuditable, ISoftDeletable
{
    private Tenant()
    {
    }

    public Tenant(string name, string slug)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(name), "Kiracı adı zorunludur.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(slug), "Kiracı kısa adı zorunludur.");

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    /// <summary>Abonelik paketi (Starter, Professional, Enterprise, WhiteLabel).</summary>
    public string Plan { get; private set; } = "Starter";

    /// <summary>Pakete dahil azami firma sayısı.</summary>
    public int MaxCompanies { get; private set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public void SetPlan(string plan, int maxCompanies)
    {
        DomainException.ThrowIf(maxCompanies < 1, "Paket en az bir firmayı kapsamalıdır.");
        Plan = plan;
        MaxCompanies = maxCompanies;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
