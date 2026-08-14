namespace ClearPay.Infrastructure.Messaging;

public sealed class RabbitRuntimeStatus
{
    public RabbitRuntimeStatus(string value) => Value = value;

    /// <summary>up, down, or off (no connection string).</summary>
    public string Value { get; }
}
