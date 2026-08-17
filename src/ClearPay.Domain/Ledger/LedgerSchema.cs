namespace ClearPay.Domain.Ledger;

/// <summary>
/// EF mapping hints for Coder (SQL Server, not Identity SQLite). Domain has no DbContext.
/// </summary>
public static class LedgerSchema
{
    public const string WalletUserIdUnique = "UX_Wallet_UserId";

    /// <summary>PLAN: LedgerEntry(WalletId, CreatedAt).</summary>
    public const string LedgerEntryWalletCreated = "IX_LedgerEntry_WalletId_CreatedAt";

    /// <summary>PLAN: IdempotencyRecord(Key) unique — duplicate → 409.</summary>
    public const string IdempotencyKeyUnique = "UX_IdempotencyRecord_Key";

    /// <summary>TASK-11 worker polls Pending. Table exists now; Hangfire is later.</summary>
    public const string OutboxStatusOccurred = "IX_OutboxMessage_Status_OccurredAt";

    public const int AmountPrecision = 18;

    public const int AmountScale = 2;

    public const int IdempotencyKeyMaxLength = 128;

    public const int UserIdMaxLength = 450;

    public const string LinkedInstrumentUserLast4Unique = "UX_LinkedInstrument_UserId_Last4";

    public const int LinkedLast4Length = 4;

    public const int LinkedLabelMaxLength = 40;
}
