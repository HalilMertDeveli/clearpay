using System.Text;
using ClearPay.Contracts;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ClearPay.Messaging;

/// <summary>
/// Publishes to the <c>clearpay.events</c> topic exchange with publisher confirms:
/// <see cref="PublishAsync"/> returns only after the broker has written the (persistent) message,
/// and throws otherwise, so the caller (outbox worker) keeps its row and tries again later.
/// That is at-least-once delivery; consumers remove duplicates by <c>MessageId</c>.
///
/// Not <c>mandatory</c>: an event nobody subscribed to yet (e.g. a wallet top-up) is dropped by the broker
/// on purpose. Every consumer queue is durable, so a subscribed service that is down still gets it.
/// One channel, used under a lock (channels are not thread-safe); a channel closed by a failed confirm
/// is replaced on the next call.
/// </summary>
public sealed class RabbitEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitConnectionProvider _connections;
    private readonly ILogger<RabbitEventPublisher> _logger;
    private readonly TimeSpan _confirmTimeout;
    private readonly object _gate = new();
    private IModel? _channel;

    public RabbitEventPublisher(
        RabbitConnectionProvider connections,
        ILogger<RabbitEventPublisher> logger,
        TimeSpan? confirmTimeout = null)
    {
        _connections = connections;
        _logger = logger;
        _confirmTimeout = confirmTimeout ?? TimeSpan.FromSeconds(5);
    }

    public Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var channel = EnsureChannel();
            var headers = new Dictionary<string, object>();
            using var activity = MessageTraceContext.StartPublish(
                MessagingNames.EventsExchange, message.Type, headers, message.TraceParent);
            activity?.SetTag("messaging.message.id", message.MessageId.ToString());
            activity?.SetTag("messaging.rabbitmq.destination.routing_key", message.RoutingKey);

            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.MessageId = message.MessageId.ToString();
            props.Type = message.Type;
            props.CorrelationId = message.CorrelationId.ToString();
            props.ContentType = "application/json";
            props.Timestamp = new AmqpTimestamp(message.OccurredAt.ToUnixTimeSeconds());
            props.Headers = headers;

            try
            {
                channel.BasicPublish(
                    MessagingNames.EventsExchange,
                    message.RoutingKey,
                    mandatory: false,
                    basicProperties: props,
                    body: Encoding.UTF8.GetBytes(message.Body));
                // Throws on nack or timeout and closes the channel; EnsureChannel opens a new one next time.
                channel.WaitForConfirmsOrDie(_confirmTimeout);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.GetType().Name);
                _logger.LogWarning(
                    "Publish of {MessageType} {MessageId} not confirmed: {Reason}",
                    message.Type, message.MessageId, ex.Message);
                throw new BrokerUnavailableException("RabbitMQ did not confirm the message.", ex);
            }

            _logger.LogDebug("Published {MessageType} {MessageId}", message.Type, message.MessageId);
        }

        return Task.CompletedTask;
    }

    private IModel EnsureChannel()
    {
        if (_channel is { IsOpen: true })
            return _channel;

        _channel?.Dispose();
        var channel = _connections.GetConnection().CreateModel();
        channel.ConfirmSelect();
        RabbitTopology.DeclareEventsExchange(channel);
        _channel = channel;
        return channel;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _channel?.Dispose();
            _channel = null;
        }
    }
}
