namespace ClearPay.Application.Banking;

/// <summary>Timeout must not post ledger. Outbox/retry is TASK-07/11.</summary>
public sealed record BankGatewayResult(bool Succeeded, bool TimedOut, string? Reference);
