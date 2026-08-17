namespace ClearPay.Domain.Ledger;

/// <summary>
/// Demo linked card for top-up/withdraw hints. Last four digits only — no PAN, no CVV.
/// Not a second cash box; money stays on the wallet ledger.
/// </summary>
public sealed class LinkedInstrument
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    /// <summary>Exactly four digits. Never a full card number.</summary>
    public string Last4 { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public string AccountHint => "****" + Last4;
}
