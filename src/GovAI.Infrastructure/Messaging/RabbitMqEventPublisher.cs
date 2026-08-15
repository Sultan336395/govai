using System.Text;
using System.Text.Json;
using GovAI.Application.Abstractions.Services;
using GovAI.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace GovAI.Infrastructure.Messaging;

/// <summary>Kuyruk devre dışıyken olayları yalnızca loglar; yerel geliştirmede RabbitMQ zorunlu olmasın diye.</summary>
public sealed class LoggingEventPublisher(ILogger<LoggingEventPublisher> logger) : IEventPublisher
{
    public Task PublishAsync<T>(string routingKey, T payload, CancellationToken cancellationToken = default) where T : class
    {
        logger.LogInformation("[kuyruk devre dışı] Olay={RoutingKey} Yük={Payload}", routingKey, JsonSerializer.Serialize(payload));
        return Task.CompletedTask;
    }
}

/// <summary>
/// RabbitMQ topic exchange üzerinden olay yayınlar. Python worker'ları aynı exchange'i dinler.
/// Bağlantı ve kanal tembel (lazy) açılır ve uygulama ömrü boyunca yeniden kullanılır.
/// </summary>
public sealed class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string routingKey, T payload, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var channel = await GetChannelAsync(cancellationToken);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, JsonOptions));

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = Guid.CreateVersion7().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Olay yayınlandı. RoutingKey={RoutingKey}", routingKey);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Kuyruk erişilemezse kullanıcı isteği başarısız olmamalı; iş zamanlanmış tarama ile telafi edilir.
            _logger.LogError(ex, "Olay yayınlanamadı. RoutingKey={RoutingKey}", routingKey);
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            return _channel;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _connectionLock.Dispose();
    }
}
