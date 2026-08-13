using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// TASK-05: özet from ledger net. No <c>UPDATE Balance</c>. PageModels do not call this type.
/// SQL unreachable → empty zeros (Identity-only site still loads).
/// </summary>
public sealed class SqlWalletReader : IWalletReader
{
    private readonly ClearPayDbContext _db;
    private readonly IClock _clock;

    public SqlWalletReader(ClearPayDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(userId))
            return Empty(userId ?? string.Empty);

        if (!await TryConnectAsync(cancellationToken).ConfigureAwait(false))
            return Empty(userId);

        var wallet = await _db.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (wallet is null)
            wallet = await EnsureWalletAsync(userId, cancellationToken).ConfigureAwait(false);

        var entries = await _db.LedgerEntries
            .AsNoTracking()
            .Where(e => e.WalletId == wallet.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = _clock.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var inMonth = entries.Where(e => e.CreatedAt >= monthStart).ToList();

        var last = entries
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .Select(e => new WalletMovement(e.CreatedAt, e.Kind.ToString(), e.Amount, e.CorrelationId))
            .ToList();

        return new WalletSummary(
            WalletId: wallet.Id,
            UserId: userId,
            Balance: LedgerPair.NetOf(entries, wallet.Id),
            MonthOutgoing: decimal.Negate(inMonth.Where(e => e.Amount < 0m).Sum(e => e.Amount)),
            MonthIncoming: inMonth.Where(e => e.Amount > 0m).Sum(e => e.Amount),
            IsFrozen: wallet.IsFrozen,
            LastMovements: last);
    }

    private async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _db.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<Wallet> EnsureWalletAsync(string userId, CancellationToken cancellationToken)
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsFrozen = false,
            CreatedAt = _clock.UtcNow
        };

        _db.Wallets.Add(wallet);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return wallet;
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            var existing = await _db.Wallets
                .AsNoTracking()
                .SingleAsync(w => w.UserId == userId, cancellationToken)
                .ConfigureAwait(false);
            return existing;
        }
    }

    private static WalletSummary Empty(string userId) => new(
        WalletId: Guid.Empty,
        UserId: userId,
        Balance: 0m,
        MonthOutgoing: 0m,
        MonthIncoming: 0m,
        IsFrozen: false,
        LastMovements: Array.Empty<WalletMovement>());
}
