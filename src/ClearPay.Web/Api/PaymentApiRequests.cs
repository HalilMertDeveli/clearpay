namespace ClearPay.Web.Api;

public sealed class CreatePaymentApiRequest
{
    public string? MerchantOrderId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    /// <summary>Test PAN. Used for BIN routing + last four, never stored (mock — no PCI scope claimed).</summary>
    public string? CardNumber { get; set; }

    public string? ReturnUrl { get; set; }
}

public sealed class RefundApiRequest
{
    public decimal Amount { get; set; }

    public string? Reason { get; set; }
}
