using ClearPay.Application.Transfers;

namespace ClearPay.Application.Ports;

/// <summary>
/// ISP: 409 store is not the transfer executor. Unique Key → duplicate insert is 409.
/// Implementation: TASK-06.
/// </summary>
public interface IIdempotencyStore
{
    Task<IdempotencyLookup?> FindAsync(string key, string scope, CancellationToken cancellationToken = default);

    Task ReserveAsync(string key, string scope, string? requestHash, CancellationToken cancellationToken = default);
}
