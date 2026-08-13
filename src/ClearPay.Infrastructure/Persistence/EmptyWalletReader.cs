using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// TASK-03 empty özet: zeros, no SQL, no ledger math.
/// TASK-05 replaces this registration with a SQL reader (ledger net, not UPDATE Balance).
/// </summary>
public sealed class EmptyWalletReader : IWalletReader
{
    public Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var summary = new WalletSummary(
            WalletId: Guid.Empty,
            UserId: userId,
            Balance: 0m,
            MonthOutgoing: 0m,
            MonthIncoming: 0m,
            IsFrozen: false,
            LastMovements: Array.Empty<WalletMovement>());

        return Task.FromResult<WalletSummary?>(summary);
    }
}
