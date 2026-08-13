namespace ClearPay.Domain.Ledger;

/// <summary>
/// Neden transaction? Gönderen − ve alıcı + ayrı commit olursa bir taraf yazılır, diğeri kalır;
/// bakiye invarianti bozulur. Ledger çifti + Transfer + IdempotencyRecord + AuditLog + OutboxMessage
/// tek SQL transaction. Biri fail → hepsi rollback.
/// Duplicate <see cref="IdempotencyRecord.Key"/> (unique) → 409, ikinci kesinti yok.
/// </summary>
public static class MoneyTransaction
{
    /// <summary>
    /// Rows that must be inserted together. Application/Infrastructure (TASK-06) opens one DbContext transaction.
    /// Domain does not call SaveChanges or UPDATE Balance.
    /// </summary>
    public static readonly string[] RequiredInserts =
    [
        nameof(LedgerEntry) + "#debit",
        nameof(LedgerEntry) + "#credit",
        nameof(Transfer),
        nameof(IdempotencyRecord),
        nameof(AuditLog),
        nameof(OutboxMessage)
    ];
}
