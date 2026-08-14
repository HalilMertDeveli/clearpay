using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>Stages idempotency rows on the same DbContext as the ledger transaction.</summary>
public sealed class SqlIdempotencyStore : IIdempotencyStore
{
    private readonly ClearPayDbContext _db;
    private readonly IClock _clock;

    public SqlIdempotencyStore(ClearPayDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IdempotencyLookup?> FindAsync(
        string key,
        string scope,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var row = await _db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == key && r.Scope == scope, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;

        return new IdempotencyLookup(row.Key, row.Scope, row.ResourceId, row.RequestHash, row.CreatedAt);
    }

    public Task ReserveAsync(
        string key,
        string scope,
        string? requestHash,
        Guid? resourceId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _db.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Key = key,
            Scope = scope,
            RequestHash = requestHash,
            ResourceId = resourceId,
            CreatedAt = _clock.UtcNow
        });
        return Task.CompletedTask;
    }
}
