using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ClearPay.Messaging;

/// <summary>
/// One long-lived AMQP connection per process (connections are expensive, channels are cheap).
/// Connects lazily, so the app still boots when the broker is down; the next publish or the consumer's
/// reconnect loop tries again. <c>AutomaticRecoveryEnabled</c> handles network blips after the first connect.
/// </summary>
public sealed class RabbitConnectionProvider : IDisposable
{
    private static readonly TimeSpan ReconnectThrottle = TimeSpan.FromSeconds(5);

    private readonly string? _amqpUri;
    private readonly string _clientName;
    private readonly ILogger<RabbitConnectionProvider> _logger;
    private readonly object _gate = new();
    private IConnection? _connection;
    private DateTimeOffset _lastFailure = DateTimeOffset.MinValue;

    public RabbitConnectionProvider(string? amqpUri, string clientName, ILogger<RabbitConnectionProvider> logger)
    {
        _amqpUri = string.IsNullOrWhiteSpace(amqpUri) ? null : amqpUri;
        _clientName = clientName;
        _logger = logger;
    }

    /// <summary>False when no connection string is set (local demo without a broker).</summary>
    public bool IsConfigured => _amqpUri is not null;

    /// <summary><c>off</c> (not configured), <c>up</c> or <c>down</c>; used by <c>/api/health</c>.</summary>
    public string Status => !IsConfigured ? "off" : _connection?.IsOpen == true ? "up" : "down";

    /// <summary>Returns the open connection or throws <see cref="BrokerUnavailableException"/>.</summary>
    public IConnection GetConnection()
    {
        if (!IsConfigured)
            throw new BrokerUnavailableException("RabbitMQ is not configured (ConnectionStrings:RabbitMq is empty).");

        lock (_gate)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            // Do not hammer a dead broker from every outbox row: one try per few seconds.
            if (DateTimeOffset.UtcNow - _lastFailure < ReconnectThrottle)
                throw new BrokerUnavailableException("RabbitMQ was unreachable a moment ago; not retrying yet.");

            try
            {
                _connection?.Dispose();
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_amqpUri!),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(3),
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                    DispatchConsumersAsync = true
                };
                _connection = factory.CreateConnection(_clientName);
                _logger.LogInformation("Connected to RabbitMQ as {ClientName}", _clientName);
                return _connection;
            }
            catch (Exception ex)
            {
                _connection = null;
                _lastFailure = DateTimeOffset.UtcNow;
                throw new BrokerUnavailableException("Could not connect to RabbitMQ.", ex);
            }
        }
    }

    /// <summary>Connect once at startup so <c>/api/health</c> shows <c>up</c> early; never throws.</summary>
    public void TryWarmUp()
    {
        if (!IsConfigured)
            return;
        try
        {
            GetConnection();
        }
        catch (BrokerUnavailableException ex)
        {
            _logger.LogWarning("RabbitMQ not reachable at startup ({Reason}); will retry on demand", ex.InnerException?.Message ?? ex.Message);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}

/// <summary>The broker cannot be reached. Callers keep their work (outbox row stays Pending) and retry later.</summary>
public sealed class BrokerUnavailableException : Exception
{
    public BrokerUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
