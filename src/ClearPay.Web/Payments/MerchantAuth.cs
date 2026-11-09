using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace ClearPay.Web.Payments;

/// <summary>
/// Config: <c>Payments:Merchants</c>. Demo merchants and their API keys.
/// Only Development ships a value; Production has none, so the merchant API answers 401 there.
/// </summary>
public sealed class PaymentMerchantOptions
{
    public const string SectionName = "Payments";

    public List<MerchantCredential> Merchants { get; set; } = [];
}

public sealed class MerchantCredential
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>Merchant API auth: <c>X-Merchant-Key</c> header. Card-acquiring merchants are not wallet users, so no JWT here.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class MerchantKeyAttribute : TypeFilterAttribute
{
    public MerchantKeyAttribute()
        : base(typeof(MerchantKeyFilter))
    {
    }
}

public sealed class MerchantKeyFilter : IAuthorizationFilter
{
    public const string HeaderName = "X-Merchant-Key";

    private const string ItemKey = "clearpay.merchantId";

    private readonly PaymentMerchantOptions _options;

    public MerchantKeyFilter(IOptions<PaymentMerchantOptions> options)
    {
        _options = options.Value;
    }

    public static string MerchantId(HttpContext context) =>
        context.Items[ItemKey] as string ?? throw new InvalidOperationException("Merchant filter did not run.");

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var given = context.HttpContext.Request.Headers[HeaderName].ToString();
        var merchant = string.IsNullOrEmpty(given)
            ? null
            : _options.Merchants.FirstOrDefault(m =>
                !string.IsNullOrEmpty(m.ApiKey)
                && CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(m.ApiKey),
                    Encoding.UTF8.GetBytes(given)));
        if (merchant is null)
        {
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Title = "Merchant key required",
                Detail = $"Send a valid {HeaderName} header.",
                Status = StatusCodes.Status401Unauthorized
            });
            return;
        }

        context.HttpContext.Items[ItemKey] = merchant.Id;
    }
}
