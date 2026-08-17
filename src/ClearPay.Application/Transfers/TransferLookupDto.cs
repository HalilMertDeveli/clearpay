namespace ClearPay.Application.Transfers;

public sealed record TransferLookupDto(
    Guid TransferId,
    Guid CorrelationId,
    decimal Amount,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt,
    Guid FromWalletId,
    Guid ToWalletId);
