namespace ClearPay.Web.Api;

public sealed class CardApiRequest
{
    public string? Last4 { get; set; }

    /// <summary>Demo PAN for BIN detect. Never persisted; parser keeps last4 + scheme.</summary>
    public string? Number { get; set; }

    public string? Label { get; set; }
}
