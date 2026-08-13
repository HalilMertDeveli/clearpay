using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>TASK-06: unique IdempotencyRecord.Key on SQL Server. Duplicate insert → 409.</summary>
public sealed class NotImplementedIdempotencyStore : IIdempotencyStore
{
    public Task<IdempotencyLookup?> FindAsync(string key, string scope, CancellationToken cancellationToken = default)
    {
        _ = key;
        _ = scope;
        _ = cancellationToken;
        throw new NotImplementedException("TASK-06: IIdempotencyStore.FindAsync.");
    }

    public Task ReserveAsync(string key, string scope, string? requestHash, CancellationToken cancellationToken = default)
    {
        _ = key;
        _ = scope;
        _ = requestHash;
        _ = cancellationToken;
        throw new NotImplementedException("TASK-06: IIdempotencyStore.ReserveAsync — unique Key.");
    }
}
