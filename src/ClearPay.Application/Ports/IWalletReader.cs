using ClearPay.Application.Wallets;

namespace ClearPay.Application.Ports;

/// <summary>
/// ISP: read-only wallet summary. PageModels must not compute ledger net.
/// TASK-03: EmptyWalletReader (zeros). TASK-05: SQL ledger net, not UPDATE. PageModels do not compute net.
/// </summary>
public interface IWalletReader
{
    Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
