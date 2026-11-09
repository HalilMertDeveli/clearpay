using ClearPay.Application.Ports;
using ClearPay.Messaging;

namespace ClearPay.Infrastructure.Messaging;

/// <summary>
/// Outbox → RabbitMQ. Persistent message on the <c>clearpay.events</c> topic exchange, routing key = event type,
/// <c>MessageId</c> = outbox row id, <c>traceparent</c> header from the row, publisher confirm awaited.
/// Throws when the broker is down or does not confirm: the outbox row then stays Pending.
/// (The old version fell back to a log line and the row was marked Sent — that silently lost the event.)
/// </summary>
public sealed class RabbitOutboxPublisher : IOutboxPublisher
{
    private readonly IEventPublisher _publisher;

    public RabbitOutboxPublisher(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task PublishAsync(OutboxEnvelope message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _publisher.PublishAsync(
            new OutgoingMessage(
                message.MessageId,
                message.Type,
                message.Payload,
                message.CorrelationId,
                message.OccurredAt,
                message.TraceParent),
            cancellationToken);
    }
}
