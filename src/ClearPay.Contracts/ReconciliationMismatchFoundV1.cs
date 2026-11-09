namespace ClearPay.Contracts;

/// <summary>
/// Published once per reconciliation report that has at least one difference.
/// The message id is the report id, so a re-publish after a crash is dropped by the consumer's idempotency table.
/// </summary>
public sealed record ReconciliationMismatchFoundV1(
    Guid ReportId,
    DateOnly BusinessDate,
    int MatchedCount,
    int OnlyAtBankCount,
    int OnlyInClearPayCount,
    int AmountMismatchCount,
    int LedgerImbalanceCount,
    DateTimeOffset CompletedAt);
