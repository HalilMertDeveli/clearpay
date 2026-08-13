namespace ClearPay.Application.Transfers;

/// <summary><see cref="IsReplay"/> true means the key was seen — caller maps to 409.</summary>
public sealed record TransferOutcome(
    bool IsReplay,
    Guid? TransferId,
    Guid CorrelationId);
