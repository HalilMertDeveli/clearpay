namespace ClearPay.Domain.Ledger;

/// <summary>
/// Customer-to-customer send. Amount is always positive; the signed pair lives on
/// <see cref="LedgerEntry"/>. Insert this row in the same SQL transaction as the pair,
/// <see cref="IdempotencyRecord"/>, <see cref="AuditLog"/>, and <see cref="OutboxMessage"/>.
/// </summary>
public sealed class Transfer
{
    public Guid Id { get; set; }

    public Guid FromWalletId { get; set; }

    public Guid ToWalletId { get; set; }

    /// <summary>Positive TRY. Debit = −Amount, credit = +Amount on the ledger pair.</summary>
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public TransferStatus Status { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
