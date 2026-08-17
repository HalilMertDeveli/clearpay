using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

public sealed class SqlLinkedInstrumentStore : ILinkedInstrumentStore
{
    private readonly ClearPayDbContext _db;
    private readonly IClock _clock;

    public SqlLinkedInstrumentStore(ClearPayDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<LinkedInstrumentDto>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || !await TryConnectAsync(cancellationToken).ConfigureAwait(false))
            return [];

        var rows = await _db.LinkedInstruments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToDto)
            .ToList();
    }

    public async Task<LinkedInstrumentDto?> AddAsync(
        string userId,
        string last4,
        string label,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;
        if (!TryNormalizeLast4(last4, out var digits))
            return null;
        if (!await TryConnectAsync(cancellationToken).ConfigureAwait(false))
            return null;

        var exists = await _db.LinkedInstruments
            .AnyAsync(x => x.UserId == userId && x.Last4 == digits, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
            return null;

        var count = await _db.LinkedInstruments.CountAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (count >= 5)
            return null;

        var trimmedLabel = string.IsNullOrWhiteSpace(label)
            ? "ClearPay Demo"
            : label.Trim();
        if (trimmedLabel.Length > LedgerSchema.LinkedLabelMaxLength)
            trimmedLabel = trimmedLabel[..LedgerSchema.LinkedLabelMaxLength];

        var row = new LinkedInstrument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Last4 = digits,
            Label = trimmedLabel,
            CreatedAt = _clock.UtcNow
        };
        _db.LinkedInstruments.Add(row);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return ToDto(row);
    }

    internal static bool TryNormalizeLast4(string? raw, out string digits)
    {
        digits = string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
            return false;
        var only = new string(raw.Where(char.IsDigit).ToArray());
        if (only.Length != LedgerSchema.LinkedLast4Length)
            return false;
        digits = only;
        return true;
    }

    private async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _db.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    private static LinkedInstrumentDto ToDto(LinkedInstrument row) =>
        new(row.Id, row.Last4, row.Label, row.AccountHint);
}
