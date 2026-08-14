using ClearPay.Application.Wallets;

namespace ClearPay.Application.Ports;

/// <summary>
/// ISP: read-only wallet summary. PageModels must not compute ledger net.
/// TASK-05: SqlWalletReader (ledger net, not UPDATE). T-041: CachedWalletReader wraps it.
/// PageModels do not compute net. Cache ≠ ledger.
/// </summary>
public interface IWalletReader
{
    Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
