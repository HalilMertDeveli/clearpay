namespace ClearPay.Domain.Ledger;

/// <summary>
/// Signed ledger line. Amount &gt; 0 credit (incoming), Amount &lt; 0 debit (outgoing).
/// Every movement is a +/− pair sharing <see cref="PairId"/> and <see cref="CorrelationId"/>;
/// the two amounts must sum to zero. Index later: (WalletId, CreatedAt).
/// There is no “UPDATE Balance” path — net of these rows is the wallet.
/// </summary>
public sealed class LedgerEntry
{
    public Guid Id { get; set; }

    public Guid WalletId { get; set; }

    /// <summary>Signed TRY. Scale 2 (kuruş). Never zero.</summary>
    public decimal Amount { get; set; }

    /// <summary>Links the debit and credit rows of one movement.</summary>
    public Guid PairId { get; set; }

    /// <summary>Dekont / audit trail. Same value on both sides of the pair and on Transfer/Audit/Outbox.</summary>
    public Guid CorrelationId { get; set; }

    public Guid? TransferId { get; set; }

    public LedgerEntryKind Kind { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsDebit => Amount < 0m;

    public bool IsCredit => Amount > 0m;
}
