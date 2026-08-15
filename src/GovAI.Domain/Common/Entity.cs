namespace GovAI.Domain.Common;

/// <summary>
/// Tüm kalıcı varlıkların ortak temeli. Kimlik karşılaştırması referans değil <see cref="Id"/> üzerinden yapılır.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public override bool Equals(object? obj)
        => obj is Entity other && other.GetType() == GetType() && other.Id == Id && Id != Guid.Empty;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}

/// <summary>
/// Bir tutarlılık sınırının (aggregate) kökü. Repository'ler yalnızca aggregate root üzerinden çalışır.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Denetim izi (audit log) için zorunlu alanlar. Persistence katmanı bu alanları otomatik doldurur.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>
/// Kayıtların fiziksel olarak silinmesi yerine pasifleştirilmesini sağlar (KVKK ve izlenebilirlik gereği).
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}

/// <summary>
/// Çok kiracılı (multi-tenant) kurulumda kaydın hangi müşteriye ait olduğunu belirtir.
/// </summary>
public interface ITenantScoped
{
    Guid TenantId { get; set; }
}

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
