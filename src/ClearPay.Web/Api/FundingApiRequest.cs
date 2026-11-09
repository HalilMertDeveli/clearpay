namespace ClearPay.Web.Api;

public sealed class FundingApiRequest
{
    public decimal Amount { get; set; }

    public string? Account { get; set; }
}
