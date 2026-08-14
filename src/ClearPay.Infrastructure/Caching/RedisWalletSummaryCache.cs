using System.Text.Json;
using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ClearPay.Infrastructure.Caching;

public sealed class RedisWalletSummaryCache : IWalletSummaryCache
{
    internal const string KeyPrefix = "clearpay:wallet-summary:";
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan OpBudget = TimeSpan.FromSeconds(2);
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IConnectionMultiplexer _mux;
    private readonly ILogger<RedisWalletSummaryCache> _logger;

    public RedisWalletSummaryCache(IConnectionMultiplexer mux, ILogger<RedisWalletSummaryCache> logger)
    {
        _mux = mux;
        _logger = logger;
    }

    public async Task<WalletSummary?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        if (!_mux.IsConnected)
            return null;

        try
        {
            var value = await WithBudgetAsync(
                    _mux.GetDatabase().StringGetAsync(Key(userId)),
                    cancellationToken)
                .ConfigureAwait(false);
            if (value.IsNullOrEmpty)
                return null;

            var json = value.ToString();
            if (string.IsNullOrEmpty(json))
                return null;

            var payload = JsonSerializer.Deserialize<CachedWalletSummaryPayload>(json, Json);
            return payload?.ToSummary();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Redis GET miss/fail for wallet summary {UserId}", userId);
            return null;
        }
    }

    public async Task SetAsync(WalletSummary summary, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(summary);
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(summary.UserId) || summary.WalletId == Guid.Empty)
            return;

        if (!_mux.IsConnected)
            return;

        try
        {
            var json = JsonSerializer.Serialize(CachedWalletSummaryPayload.From(summary), Json);
            await WithBudgetAsync(
                    _mux.GetDatabase().StringSetAsync(Key(summary.UserId), json, Ttl),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Redis SET fail for wallet summary {UserId}", summary.UserId);
        }
    }

    public async Task InvalidateAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(userId))
            return;

        if (!_mux.IsConnected)
            return;

        try
        {
            await WithBudgetAsync(
                    _mux.GetDatabase().KeyDeleteAsync(Key(userId)),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Redis DEL fail for wallet summary {UserId}", userId);
        }
    }

    public static string Key(string userId) => KeyPrefix + userId;

    private static async Task<T> WithBudgetAsync<T>(Task<T> operation, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(OpBudget);
        return await operation.WaitAsync(cts.Token).ConfigureAwait(false);
    }
}

internal sealed record CachedWalletSummaryPayload(
    Guid WalletId,
    string UserId,
    decimal Balance,
    decimal MonthOutgoing,
    decimal MonthIncoming,
    bool IsFrozen,
    List<CachedWalletMovementPayload> LastMovements)
{
    public static CachedWalletSummaryPayload From(WalletSummary summary) => new(
        summary.WalletId,
        summary.UserId,
        summary.Balance,
        summary.MonthOutgoing,
        summary.MonthIncoming,
        summary.IsFrozen,
        summary.LastMovements.Select(m => new CachedWalletMovementPayload(m.At, m.Kind, m.Amount, m.CorrelationId)).ToList());

    public WalletSummary ToSummary() => new(
        WalletId,
        UserId,
        Balance,
        MonthOutgoing,
        MonthIncoming,
        IsFrozen,
        LastMovements.Select(m => new WalletMovement(m.At, m.Kind, m.Amount, m.CorrelationId)).ToList());
}

internal sealed record CachedWalletMovementPayload(
    DateTimeOffset At,
    string Kind,
    decimal Amount,
    Guid CorrelationId);
