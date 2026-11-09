using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// TASK-05: özet from ledger net. No <c>UPDATE Balance</c>. PageModels do not call this type.
/// SQL unreachable → empty zeros (Identity-only site still loads).
/// SQL Server: totals and last 5 come from <c>usp_Wallet_GetSummary</c> / <c>usp_Wallet_GetStatementPage</c>
/// (aggregated in SQL, not by loading every ledger row). SQLite (tests): same numbers via LINQ.
/// </summary>
public sealed class SqlWalletReader : IWalletReader
{
    private const int LastMovementCount = 5;

    private readonly ClearPayDbContext _db;
    private readonly IClock _clock;
    private readonly LedgerProcedures _procedures;

    public SqlWalletReader(ClearPayDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
        _procedures = new LedgerProcedures(db);
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

        var now = _clock.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        if (_procedures.IsAvailable)
            return await FromProceduresAsync(wallet, userId, monthStart, cancellationToken).ConfigureAwait(false);

        var entries = await _db.LedgerEntries
            .AsNoTracking()
            .Where(e => e.WalletId == wallet.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var inMonth = entries.Where(e => e.CreatedAt >= monthStart).ToList();

        var last = entries
            .OrderByDescending(e => e.CreatedAt)
            .Take(LastMovementCount)
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

    private async Task<WalletSummary> FromProceduresAsync(
        Wallet wallet,
        string userId,
        DateTimeOffset monthStart,
        CancellationToken cancellationToken)
    {
        var totals = await _procedures
            .GetWalletSummaryAsync(wallet.Id, monthStart, cancellationToken)
            .ConfigureAwait(false);
        var lastRows = await _procedures
            .GetStatementPageAsync(wallet.Id, null, null, null, 0, LastMovementCount, cancellationToken)
            .ConfigureAwait(false);

        var last = lastRows
            .Select(r => new WalletMovement(r.CreatedAt, ((LedgerEntryKind)r.Kind).ToString(), r.Amount, r.CorrelationId))
            .ToList();

        return new WalletSummary(
            WalletId: wallet.Id,
            UserId: userId,
            Balance: totals.Balance,
            MonthOutgoing: totals.MonthOutgoing,
            MonthIncoming: totals.MonthIncoming,
            IsFrozen: wallet.IsFrozen,
            LastMovements: last);
    }

    private async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));
            return await _db.Database.CanConnectAsync(timeout.Token).ConfigureAwait(false);
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
