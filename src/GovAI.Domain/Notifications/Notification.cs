using GovAI.Domain.Common;

namespace GovAI.Domain.Notifications;

/// <summary>
/// Bildirim ve Hatırlatma Modülü (Modül 10) kaydı.
/// <see cref="DeduplicationKey"/> aynı olayın tekrar tekrar bildirilmesini engeller.
/// </summary>
public class Notification : AggregateRoot, ITenantScoped
{
    private Notification()
    {
    }

    public Notification(
        Guid tenantId,
        Guid? companyId,
        NotificationKind kind,
        string title,
        string body,
        DateTimeOffset createdAt,
        string deduplicationKey,
        Guid? opportunityId = null)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "Bildirim başlığı zorunludur.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(deduplicationKey), "Tekilleştirme anahtarı zorunludur.");

        TenantId = tenantId;
        CompanyId = companyId;
        OpportunityId = opportunityId;
        Kind = kind;
        Title = title.Trim();
        Body = body;
        CreatedAt = createdAt;
        DeduplicationKey = deduplicationKey;
    }

    public Guid TenantId { get; set; }

    public Guid? CompanyId { get; private set; }

    public Guid? OpportunityId { get; private set; }

    public NotificationKind Kind { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    /// <summary>Ör. <c>deadline:{opportunityId}:{companyId}:7d</c> — aynı hatırlatma iki kez gönderilmez.</summary>
    public string DeduplicationKey { get; private set; } = string.Empty;

    public NotificationChannel Channel { get; private set; } = NotificationChannel.InApp;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public string? DeliveryError { get; private set; }

    public int DeliveryAttemptCount { get; private set; }

    public bool IsRead => ReadAt is not null;

    public void SetChannel(NotificationChannel channel) => Channel = channel;

    public void MarkSent(DateTimeOffset sentAt)
    {
        SentAt = sentAt;
        DeliveryError = null;
        DeliveryAttemptCount++;
    }

    public void MarkFailed(string error)
    {
        DeliveryError = error;
        DeliveryAttemptCount++;
    }

    public void MarkRead(DateTimeOffset readAt) => ReadAt ??= readAt;
}
