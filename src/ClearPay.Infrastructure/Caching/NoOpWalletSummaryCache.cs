using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;

namespace ClearPay.Infrastructure.Caching;

/// <summary>Redis connection string empty (tests, Production until Q2).</summary>
public sealed class NoOpWalletSummaryCache : IWalletSummaryCache
{
    public Task<WalletSummary?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<WalletSummary?>(null);
    }

    public Task SetAsync(WalletSummary summary, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(summary);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
