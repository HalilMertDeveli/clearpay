using System.Text;
using ClearPay.Application.Ports;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ClearPay.Infrastructure.Messaging;

public sealed class RabbitOutboxPublisher : IOutboxPublisher
{
    public const string QueueName = "clearpay.outbox";

    private readonly IConnection? _connection;
    private readonly ILogger<RabbitOutboxPublisher> _logger;
    private readonly LoggingOutboxPublisher _fallback;

    public RabbitOutboxPublisher(
        IConnection? connection,
        ILogger<RabbitOutboxPublisher> logger,
        LoggingOutboxPublisher fallback)
    {
        _connection = connection;
        _logger = logger;
        _fallback = fallback;
    }

    public Task PublishAsync(string type, string payload, Guid correlationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_connection is null || !_connection.IsOpen)
        {
            return _fallback.PublishAsync(type, payload, correlationId, cancellationToken);
        }

        try
        {
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(payload ?? string.Empty);
            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.Type = type;
            props.CorrelationId = correlationId.ToString();
            channel.BasicPublish(exchange: string.Empty, routingKey: QueueName, basicProperties: props, body: body);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rabbit publish failed; falling back to log publisher.");
            return _fallback.PublishAsync(type, payload, correlationId, cancellationToken);
        }
    }
}

internal static class RabbitConnectionFactory
{
    public static IConnection? TryCreate(string? amqp)
    {
        if (string.IsNullOrWhiteSpace(amqp))
            return null;
        try
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(amqp),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(2)
            };
            return factory.CreateConnection("clearpay");
        }
        catch (Exception)
        {
            return null;
        }
    }
}
