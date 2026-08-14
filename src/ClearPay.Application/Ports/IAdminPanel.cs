using ClearPay.Application.Admin;

namespace ClearPay.Application.Ports;

public interface IAdminPanel
{
    Task<bool> FreezeByEmailAsync(string email, string actorUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FailedOutboxItem>> ListFailedAsync(CancellationToken cancellationToken = default);

    Task<bool> RequeueAsync(Guid outboxId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditItem>> SearchAuditAsync(
        string? actorUserId,
        Guid? correlationId,
        DateTimeOffset? from,
        CancellationToken cancellationToken = default);
}
