namespace ClearPay.Contracts;

/// <summary>Broker names every service agrees on. Queue names belong to each consumer, not here.</summary>
public static class MessagingNames
{
    /// <summary>Durable topic exchange. Routing key = event type, e.g. <c>clearpay.payment.captured</c>.</summary>
    public const string EventsExchange = "clearpay.events";

    /// <summary>Suffix of the TTL queue a failed message waits in before it goes back to the work queue.</summary>
    public const string RetrySuffix = ".retry";

    /// <summary>Suffix of the dead-letter queue: poison messages and messages that used up their retries.</summary>
    public const string DeadLetterSuffix = ".dlq";

    /// <summary>Header set on a dead-lettered message: why it ended up in the DLQ.</summary>
    public const string ErrorHeader = "x-clearpay-error";

    /// <summary>Header set on a dead-lettered message: how many times the consumer tried it.</summary>
    public const string AttemptsHeader = "x-clearpay-attempts";
}
