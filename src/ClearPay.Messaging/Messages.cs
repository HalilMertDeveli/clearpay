using ClearPay.Contracts;

namespace ClearPay.Messaging;

/// <summary>
/// What a producer hands to <see cref="RabbitEventPublisher"/>. <see cref="MessageId"/> is the outbox row id
/// (or the report id), never a fresh Guid per send: a re-send after a crash must carry the same id so the
/// consumer's idempotency table can drop it.
/// </summary>
public sealed record OutgoingMessage(
    Guid MessageId,
    string Type,
    string Body,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    string? TraceParent = null)
{
    /// <summary>Topic routing key. Defaults to the event type.</summary>
    public string RoutingKey { get; init; } = Type;
}

/// <summary>A delivered message as a handler sees it: no RabbitMQ types leak into handlers.</summary>
public sealed record IncomingMessage(
    Guid MessageId,
    string Type,
    string Body,
    Guid? CorrelationId,
    int Attempt);

/// <summary>One handler per consumer queue. Resolved in a fresh DI scope per message (own DbContext).</summary>
public interface IMessageHandler
{
    Task HandleAsync(IncomingMessage message, CancellationToken cancellationToken);
}

/// <summary>Port for services that publish their own events (the reconciliation service).</summary>
public interface IEventPublisher
{
    /// <summary>Returns when the broker confirmed the message; throws when it did not.</summary>
    Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken = default);
}

/// <summary>
/// The message itself is wrong (bad JSON, missing field, unknown version). Retrying cannot help,
/// so the consumer sends it straight to the dead-letter queue.
/// </summary>
public sealed class PoisonMessageException : Exception
{
    public PoisonMessageException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
