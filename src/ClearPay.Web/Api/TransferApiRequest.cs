namespace ClearPay.Web.Api;

public sealed class TransferApiRequest
{
    public string Recipient { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? Description { get; set; }
}
