using ClearPay.Contracts;
using RabbitMQ.Client;

namespace ClearPay.Messaging;

/// <summary>
/// Declares the broker objects. Every declare is idempotent, so producers and consumers both call it
/// on start-up and nobody depends on "who ran first". Changing a queue's arguments later needs a new
/// queue name (RabbitMQ refuses a re-declare with different arguments: PRECONDITION_FAILED).
///
/// Per consumer queue <c>Q</c>:
/// <code>
///   clearpay.events (topic) --binding keys--&gt; Q
///   Q        : nack(requeue:false) dead-letters to Q.retry  (x-dead-letter-exchange "", routing key Q.retry)
///   Q.retry  : no consumer; after x-message-ttl it dead-letters back to Q  (each round updates x-death)
///   Q.dlq    : poison messages and messages that used all attempts; a human looks at these
/// </code>
/// </summary>
public static class RabbitTopology
{
    public static void DeclareEventsExchange(IModel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        channel.ExchangeDeclare(MessagingNames.EventsExchange, ExchangeType.Topic, durable: true, autoDelete: false);
    }

    public static void DeclareConsumerQueue(IModel channel, ConsumerQueueOptions options)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(options);
        DeclareEventsExchange(channel);

        var queue = options.QueueName;
        channel.QueueDeclare(options.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(
            options.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-message-ttl"] = (int)options.RetryDelay.TotalMilliseconds,
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = queue
            });
        channel.QueueDeclare(
            queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = options.RetryQueue
            });

        foreach (var key in options.BindingKeys)
            channel.QueueBind(queue, MessagingNames.EventsExchange, key);
    }
}

/// <summary>One consumer's queue and retry policy.</summary>
public sealed class ConsumerQueueOptions
{
    /// <summary>Also used as the consumer name in the idempotency table.</summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>Topic patterns, e.g. <c>clearpay.payment.captured</c> or <c>clearpay.reconciliation.*</c>.</summary>
    public IReadOnlyList<string> BindingKeys { get; set; } = [];

    /// <summary>Total tries including the first delivery. 3 = first try + 2 delayed retries, then DLQ.</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>How long a failed message waits in the retry queue (fixed delay: one TTL per queue).</summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Unacked messages in flight per consumer.</summary>
    public ushort Prefetch { get; set; } = 10;

    public string RetryQueue => QueueName + MessagingNames.RetrySuffix;

    public string DeadLetterQueue => QueueName + MessagingNames.DeadLetterSuffix;
}
