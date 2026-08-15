using System.Text.Json;
using GovAI.Application.Abstractions.Services;
using GovAI.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace GovAI.Infrastructure.Caching;

/// <summary>Redis devre dışıyken kullanılan boş uygulama; cache olmadan da sistem çalışır.</summary>
public sealed class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class RedisCacheService(
    IConnectionMultiplexer connection,
    IOptions<RedisOptions> options,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly RedisOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await connection.GetDatabase().StringGetAsync(Prefixed(key));
            // RedisValue hem string hem byte[] dönüşümü sunduğu için aşırı yükleme belirsizliği olmasın diye açıkça string'e çevriliyor.
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
        }
        catch (Exception ex) when (ex is RedisException or JsonException)
        {
            // Cache erişilemezse istek başarısız olmamalı; kaynaktan okumaya düşer.
            logger.LogWarning(ex, "Cache okunamadı. Key={Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await connection.GetDatabase().StringSetAsync(Prefixed(key), payload, ttl);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Cache yazılamadı. Key={Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await connection.GetDatabase().KeyDeleteAsync(Prefixed(key));
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Cache silinemedi. Key={Key}", key);
        }
    }

    /// <summary>Firma profili değiştiğinde o firmaya ait tüm önbellek girdilerini temizler.</summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = $"{Prefixed(prefix)}*";

            foreach (var endpoint in connection.GetEndPoints())
            {
                var server = connection.GetServer(endpoint);
                if (!server.IsConnected || server.IsReplica)
                {
                    continue;
                }

                var database = connection.GetDatabase();
                await foreach (var key in server.KeysAsync(pattern: pattern).WithCancellation(cancellationToken))
                {
                    await database.KeyDeleteAsync(key);
                }
            }
        }
        catch (Exception ex) when (ex is RedisException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Önek ile cache temizlenemedi. Prefix={Prefix}", prefix);
        }
    }

    private string Prefixed(string key) => $"{_options.InstanceName}{key}";
}
