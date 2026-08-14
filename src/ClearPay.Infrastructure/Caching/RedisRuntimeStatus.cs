using StackExchange.Redis;

namespace ClearPay.Infrastructure.Caching;

/// <summary>Health probe: <c>off</c> (no mux), <c>up</c>, or <c>down</c>.</summary>
public sealed class RedisRuntimeStatus
{
    private readonly IConnectionMultiplexer? _mux;

    public RedisRuntimeStatus(IConnectionMultiplexer? mux) => _mux = mux;

    public string Value => _mux is null ? "off" : _mux.IsConnected ? "up" : "down";
}
