using ClearPay.Application.Wallets;

namespace ClearPay.Application.Ports;

/// <summary>
/// ISP: read-only wallet summary. PageModels must not compute ledger net.
/// Implementation: TASK-05 (Infrastructure). Balance = signed ledger sum, not UPDATE.
/// </summary>
public interface IWalletReader
{
    Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
