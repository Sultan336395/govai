using System.ComponentModel.DataAnnotations;

namespace GovAI.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = "govai";

    [Required]
    public string Audience { get; set; } = "govai-api";

    public int AccessTokenMinutes { get; set; } = 60;
}

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>Kural çıkarımı gibi yapılandırılmış çıktı gerektiren işler için model.</summary>
    public string ExtractionModel { get; set; } = "gpt-4.1";

    /// <summary>Yönetici özeti gibi metin üretimi için model.</summary>
    public string SummaryModel { get; set; } = "gpt-4.1-mini";

    public int TimeoutSeconds { get; set; } = 90;

    /// <summary>API anahtarı yoksa servis devre dışı kalır; skorlama etkilenmez, yalnızca özet üretilmez.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";

    public string InstanceName { get; set; } = "govai:";

    public bool Enabled { get; set; } = true;
}

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    /// <summary>Tüm GOVAI olayları bu topic exchange üzerinden dağıtılır.</summary>
    public string ExchangeName { get; set; } = "govai.events";

    public bool Enabled { get; set; } = true;
}
