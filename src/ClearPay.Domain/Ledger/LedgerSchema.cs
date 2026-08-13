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

    public const int AmountPrecision = 18;

    public const int AmountScale = 2;
}
