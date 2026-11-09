namespace ClearPay.Application.Ports;

/// <summary>
/// Delivers one outbox row to the broker. Must throw when delivery is not confirmed, so the
/// row stays Pending and the next worker run tries again (at-least-once; consumers dedupe by <see cref="OutboxEnvelope.MessageId"/>).
/// </summary>
public interface IOutboxPublisher
{
    Task PublishAsync(OutboxEnvelope message, CancellationToken cancellationToken = default);
}

/// <summary>An outbox row on its way out. <see cref="MessageId"/> = outbox row id, stable across re-sends.</summary>
public sealed record OutboxEnvelope(
    Guid MessageId,
    string Type,
    string Payload,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    string? TraceParent = null);

public interface IOutboxProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}
