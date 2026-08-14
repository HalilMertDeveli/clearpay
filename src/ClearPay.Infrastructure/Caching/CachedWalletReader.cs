using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;

namespace ClearPay.Infrastructure.Caching;

/// <summary>
/// Decorator over <see cref="Persistence.SqlWalletReader"/>. Miss / Redis down → SQL.
/// Empty SQL-down summaries (<c>WalletId == Guid.Empty</c>) are not cached.
/// </summary>
public sealed class CachedWalletReader : IWalletReader
{
    private readonly IWalletReader _inner;
    private readonly IWalletSummaryCache _cache;

    public CachedWalletReader(IWalletReader inner, IWalletSummaryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(userId))
            return await _inner.GetByUserIdAsync(userId ?? string.Empty, cancellationToken).ConfigureAwait(false);

        var cached = await _cache.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
            return cached;

        var live = await _inner.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (live is not null && live.WalletId != Guid.Empty)
            await _cache.SetAsync(live, cancellationToken).ConfigureAwait(false);

        return live;
    }
}
