using GovAI.Domain.Common;

namespace GovAI.Domain.Auditing;

/// <summary>
/// İşlem bazlı denetim kaydı (Teknik doküman 5.4).
/// "Her skor ve kullanıcı aksiyonu, geriye dönük izlenebilirlik için zaman damgası ile kaydedilmelidir."
/// Kayıtlar yalnızca eklenir; güncellenmez ve silinmez.
/// </summary>
public class AuditLogEntry : Entity
{
    private AuditLogEntry()
    {
    }

    public AuditLogEntry(
        Guid? tenantId,
        string action,
        string entityType,
        string? entityId,
        string? userId,
        string? userEmail,
        DateTimeOffset occurredAt)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(action), "Denetim eylemi zorunludur.");

        TenantId = tenantId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        UserId = userId;
        UserEmail = userEmail;
        OccurredAt = occurredAt;
    }

    public Guid? TenantId { get; private set; }

    /// <summary>Ör. <c>CompanyProfile.Updated</c>, <c>Eligibility.Recalculated</c>, <c>Report.Exported</c>.</summary>
    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string? EntityId { get; private set; }

    public string? UserId { get; private set; }

    public string? UserEmail { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    /// <summary>Değişiklik ayrıntısı (eski/yeni değerler) — kişisel veri içermeyecek şekilde maskelenir.</summary>
    public string? PayloadJson { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>İlgili HTTP isteğinin korelasyon kimliği; merkezi loglarla eşleştirmeyi sağlar.</summary>
    public string? CorrelationId { get; private set; }

    public void SetRequestContext(string? ipAddress, string? userAgent, string? correlationId)
    {
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CorrelationId = correlationId;
    }

    public void SetPayload(string? payloadJson) => PayloadJson = payloadJson;
}
