namespace ClearPay.Application.Transfers;

/// <summary>Input for TASK-06. HTTP layer maps body + Idempotency-Key; it does not post ledger rows.</summary>
public sealed record TransferCommand(
    string ActorUserId,
    string Recipient,
    decimal Amount,
    string? Description,
    string IdempotencyKey);
