using ClearPay.Application.Wallets;

namespace ClearPay.Application.Ports;

/// <summary>
/// TASK-12 kısmi (T-041): özet DTO cache. Kaynak ledger SQL; cache ≠ kasa.
/// Havale sonrası <see cref="InvalidateAsync"/> (TASK-06 bağlar).
/// </summary>
public interface IWalletSummaryCache
{
    Task<WalletSummary?> GetAsync(string userId, CancellationToken cancellationToken = default);

    Task SetAsync(WalletSummary summary, CancellationToken cancellationToken = default);

    Task InvalidateAsync(string userId, CancellationToken cancellationToken = default);
}
