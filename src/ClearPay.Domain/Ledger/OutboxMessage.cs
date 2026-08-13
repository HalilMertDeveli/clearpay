namespace ClearPay.Domain.Ledger;

/// <summary>
/// Neden outbox? HTTP bittikten sonra kuyruğa “elde” basmak timeout’ta kaybettirir:
/// istemci retry eder, banka/mesaj “gitti sandık” veya hiç gitmez. Satır ledger ile
/// aynı SQL transaction’da commit olur; worker commit’ten sonra yayınlar.
/// Timeout kaybettirmez — kayıt DB’de bekler (Hangfire TASK-11).
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Null = unpublished. Worker sets this after a successful publish.</summary>
    public DateTimeOffset? ProcessedAt { get; set; }
}
