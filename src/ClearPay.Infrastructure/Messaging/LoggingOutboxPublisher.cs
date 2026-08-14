using ClearPay.Application.Ports;
using Microsoft.Extensions.Logging;

namespace ClearPay.Infrastructure.Messaging;

/// <summary>Fallback when RabbitMQ is off or down. Hangfire still marks outbox Sent after this call.</summary>
public sealed class LoggingOutboxPublisher : IOutboxPublisher
{
    private readonly ILogger<LoggingOutboxPublisher> _logger;

    public LoggingOutboxPublisher(ILogger<LoggingOutboxPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(string type, string payload, Guid correlationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(
            "Outbox {Type} correlation {CorrelationId} payload length {Length}",
            type,
            correlationId,
            payload?.Length ?? 0);
        return Task.CompletedTask;
    }
}
