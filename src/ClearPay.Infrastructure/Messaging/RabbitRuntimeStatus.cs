using ClearPay.Messaging;

namespace ClearPay.Infrastructure.Messaging;

/// <summary>Broker status for <c>/api/health</c>: read live from the connection provider, not frozen at startup.</summary>
public sealed class RabbitRuntimeStatus
{
    private readonly RabbitConnectionProvider? _provider;
    private readonly string _fixed;

    public RabbitRuntimeStatus(string value)
    {
        _fixed = value;
    }

    public RabbitRuntimeStatus(RabbitConnectionProvider provider)
    {
        _provider = provider;
        _fixed = "off";
    }

    /// <summary>up, down, or off (no connection string).</summary>
    public string Value => _provider?.Status ?? _fixed;
}
