using GovAI.Domain.Common;

namespace GovAI.Domain.Sources;

/// <summary>
/// İzlenen resmî veri kaynağı (Modül 1). Tarama takvimi ve son durum bilgisini taşır.
/// Python collector worker'ı bu kayıtları okuyup tarama yapar.
/// </summary>
public class Source : AggregateRoot, IAuditable
{
    private Source()
    {
    }

    public Source(string name, SourceType type, string baseUrl, string cronExpression)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(name), "Kaynak adı zorunludur.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(baseUrl), "Kaynak adresi zorunludur.");
        DomainException.ThrowIf(
            !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps),
            "Kaynak adresi geçerli bir http/https adresi olmalıdır.");

        Name = name.Trim();
        Type = type;
        BaseUrl = baseUrl.Trim();
        CronExpression = cronExpression;
        IsEnabled = true;
    }

    public string Name { get; private set; } = string.Empty;

    public SourceType Type { get; private set; }

    public string BaseUrl { get; private set; } = string.Empty;

    /// <summary>Tarama takvimi (ör. <c>0 6 * * *</c> — her gün 06:00).</summary>
    public string CronExpression { get; private set; } = "0 6 * * *";

    /// <summary>Kaynağa özgü ayarlar (seçiciler, sayfalama, kimlik doğrulama) — jsonb olarak saklanır.</summary>
    public string? ConfigurationJson { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTimeOffset? LastRunAt { get; private set; }

    public CrawlStatus LastRunStatus { get; private set; } = CrawlStatus.Pending;

    public string? LastRunMessage { get; private set; }

    /// <summary>Üst üste başarısız çalışma sayısı; eşiği aşarsa kaynak otomatik devre dışı bırakılır.</summary>
    public int ConsecutiveFailureCount { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    private const int MaxConsecutiveFailures = 5;

    public void Configure(string cronExpression, string? configurationJson)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(cronExpression), "Tarama takvimi zorunludur.");
        CronExpression = cronExpression.Trim();
        ConfigurationJson = configurationJson;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    public void RecordRun(DateTimeOffset runAt, CrawlStatus status, string? message)
    {
        LastRunAt = runAt;
        LastRunStatus = status;
        LastRunMessage = message;

        if (status == CrawlStatus.Failed)
        {
            ConsecutiveFailureCount++;
            if (ConsecutiveFailureCount >= MaxConsecutiveFailures)
            {
                IsEnabled = false;
            }
        }
        else if (status == CrawlStatus.Succeeded)
        {
            ConsecutiveFailureCount = 0;
        }
    }
}
