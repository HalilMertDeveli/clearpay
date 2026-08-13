namespace ClearPay.Domain.Ledger;

/// <summary>
/// One customer purse. Unique <see cref="UserId"/> (1 user = 1 wallet).
/// Balance is never stored as an independently updated column: it is
/// <see cref="LedgerPair.NetOf"/> over signed <see cref="LedgerEntry"/> rows.
/// Frozen wallets cannot send or withdraw; incoming credit may still post.
/// </summary>
public sealed class Wallet
{
    public Guid Id { get; set; }

    /// <summary>ASP.NET Identity user id (string PK). Unique index in SQL Server.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>SPEC: dondurulmuş cüzdan gönderemez / çekemez.</summary>
    public bool IsFrozen { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool CanDebit => !IsFrozen;
}
