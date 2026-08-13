namespace ClearPay.Domain.Ledger;

/// <summary>
/// SPEC: kim, ne, ne zaman, correlation id. Written in the same transaction as the ledger pair.
/// Refunds are a reverse pair plus a new audit row — never an unaudited balance patch.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; set; }

    public string ActorUserId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    /// <summary>JSON details (wallets, amount). Not a substitute for ledger rows.</summary>
    public string? Details { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
