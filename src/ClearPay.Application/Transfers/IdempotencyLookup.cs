namespace ClearPay.Application.Transfers;

public sealed record IdempotencyLookup(
    string Key,
    string Scope,
    Guid? ResourceId,
    string? RequestHash,
    DateTimeOffset CreatedAt);
