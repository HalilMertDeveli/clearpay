using ClearPay.Application.Activity;

namespace ClearPay.Application.Ports;

public interface IActivityReader
{
    Task<ActivityPage> ListAsync(
        string userId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? kind,
        int page,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<ReceiptDto?> GetReceiptAsync(
        string userId,
        Guid correlationId,
        CancellationToken cancellationToken = default);
}
