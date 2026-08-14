using ClearPay.Application.Activity;
using ClearPay.Application.Banking;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

public sealed class SqlActivityReader : IActivityReader
{
    public const int PageSize = 20;

    private readonly ClearPayDbContext _db;
    private readonly IUserDirectory _users;

    public SqlActivityReader(ClearPayDbContext db, IUserDirectory users)
    {
        _db = db;
        _users = users;
    }

    public async Task<ActivityPage> ListAsync(
        string userId,
        DateTimeOffset? from,
        string? kind,
        int page,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(userId))
            return new ActivityPage([], 1, PageSize, 0);

        if (!await TryConnectAsync(cancellationToken).ConfigureAwait(false))
            return new ActivityPage([], 1, PageSize, 0);

        var wallet = await _db.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (wallet is null)
            return new ActivityPage([], 1, PageSize, 0);

        page = Math.Max(page, 1);
        var query = _db.LedgerEntries.AsNoTracking().Where(e => e.WalletId == wallet.Id);
        var rows = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        if (from is DateTimeOffset start)
            rows = rows.Where(e => e.CreatedAt >= start).ToList();
        if (TryParseKind(kind, out var parsed))
            rows = rows.Where(e => e.Kind == parsed).ToList();

        var total = rows.Count;
        rows = rows
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var pairIds = rows.Select(r => r.PairId).Distinct().ToList();
        var counterparts = await _db.LedgerEntries.AsNoTracking()
            .Where(e => pairIds.Contains(e.PairId) && e.WalletId != wallet.Id)
            .Select(e => new { e.PairId, e.WalletId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var otherWalletIds = counterparts.Select(c => c.WalletId).Distinct().ToList();
        var otherWallets = await _db.Wallets.AsNoTracking()
            .Where(w => otherWalletIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, cancellationToken)
            .ConfigureAwait(false);

        var items = new List<ActivityItem>(rows.Count);
        foreach (var row in rows)
        {
            var otherId = counterparts.FirstOrDefault(c => c.PairId == row.PairId)?.WalletId;
            var otherUser = otherId is Guid id && otherWallets.TryGetValue(id, out var ow) ? ow.UserId : "";
            items.Add(new ActivityItem(
                row.CreatedAt,
                row.CorrelationId,
                row.Kind.ToString(),
                await LabelAsync(otherUser, cancellationToken).ConfigureAwait(false),
                row.Amount,
                "Completed"));
        }

        return new ActivityPage(items, page, PageSize, total);
    }

    public async Task<ReceiptDto?> GetReceiptAsync(
        string userId,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (correlationId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            return null;
        if (!await TryConnectAsync(cancellationToken).ConfigureAwait(false))
            return null;

        var wallet = await _db.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (wallet is null)
            return null;

        var mine = await _db.LedgerEntries.AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.WalletId == wallet.Id && e.CorrelationId == correlationId,
                cancellationToken)
            .ConfigureAwait(false);
        if (mine is null)
            return null;

        var pair = await _db.LedgerEntries.AsNoTracking()
            .Where(e => e.PairId == mine.PairId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var debit = pair.Single(e => e.Amount < 0m);
        var credit = pair.Single(e => e.Amount > 0m);
        var debitWallet = await _db.Wallets.AsNoTracking()
            .SingleAsync(w => w.Id == debit.WalletId, cancellationToken)
            .ConfigureAwait(false);
        var creditWallet = await _db.Wallets.AsNoTracking()
            .SingleAsync(w => w.Id == credit.WalletId, cancellationToken)
            .ConfigureAwait(false);

        return new ReceiptDto(
            correlationId,
            mine.CreatedAt,
            mine.Kind.ToString(),
            credit.Amount,
            await LabelAsync(debitWallet.UserId, cancellationToken).ConfigureAwait(false),
            await LabelAsync(creditWallet.UserId, cancellationToken).ConfigureAwait(false),
            mine.Description);
    }

    private async Task<string> LabelAsync(string otherUserId, CancellationToken cancellationToken)
    {
        if (string.Equals(otherUserId, Treasury.UserId, StringComparison.Ordinal))
            return "ClearPay treasury (demo)";
        var email = await _users.FindEmailByUserIdAsync(otherUserId, cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(email) ? otherUserId : email;
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

    private static bool TryParseKind(string? kind, out LedgerEntryKind parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(kind) || kind.Equals("all", StringComparison.OrdinalIgnoreCase))
            return false;
        return Enum.TryParse(kind, ignoreCase: true, out parsed);
    }
}
