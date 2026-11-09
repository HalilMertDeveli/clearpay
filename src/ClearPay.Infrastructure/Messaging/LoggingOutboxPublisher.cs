using ClearPay.Application.Ports;
using Microsoft.Extensions.Logging;

namespace ClearPay.Infrastructure.Messaging;

/// <summary>
/// Used only when no broker is configured (<c>ConnectionStrings:RabbitMq</c> empty: tests, a laptop demo).
/// Then there are no consumers either, so logging and marking the row Sent loses nothing.
/// When a broker IS configured but down, <see cref="RabbitOutboxPublisher"/> throws instead.
/// </summary>
public sealed class LoggingOutboxPublisher : IOutboxPublisher
{
    private readonly ILogger<LoggingOutboxPublisher> _logger;

    public LoggingOutboxPublisher(ILogger<LoggingOutboxPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(OutboxEnvelope message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation(
            "Outbox {Type} {MessageId} correlation {CorrelationId} payload length {Length} (no broker configured)",
            message.Type,
            message.MessageId,
            message.CorrelationId,
            message.Payload?.Length ?? 0);
        return Task.CompletedTask;
    }
}
