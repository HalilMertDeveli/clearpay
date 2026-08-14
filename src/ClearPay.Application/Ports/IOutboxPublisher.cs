namespace ClearPay.Application.Ports;

/// <summary>TASK-11: deliver one outbox payload. TASK-12 may bind Rabbit.</summary>
public interface IOutboxPublisher
{
    Task PublishAsync(string type, string payload, Guid correlationId, CancellationToken cancellationToken = default);
}

public interface IOutboxProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}
