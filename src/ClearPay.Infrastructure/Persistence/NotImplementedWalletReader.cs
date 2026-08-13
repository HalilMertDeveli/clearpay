using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>TASK-05: SQL Server read — ledger net, month aggregates, last 5. Not Identity SQLite.</summary>
public sealed class NotImplementedWalletReader : IWalletReader
{
    public Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        _ = userId;
        _ = cancellationToken;
        throw new NotImplementedException("TASK-05: IWalletReader — bakiye ledger net, ay giden/gelen, son 5 hareket.");
    }
}
