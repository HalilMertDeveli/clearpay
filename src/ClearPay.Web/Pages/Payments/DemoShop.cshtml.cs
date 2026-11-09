using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages.Payments;

/// <summary>
/// A pretend merchant checkout so the 3DS flow is clickable in a browser.
/// It plays the merchant's backend in-process (calls <see cref="IPaymentGateway"/> as merchant
/// <see cref="MerchantId"/>); an external merchant would use <c>POST /api/payments</c> with its key.
/// </summary>
[AllowAnonymous]
public sealed class DemoShopModel : PageModel
{
    public const string MerchantId = "demo-shop";

    private readonly IPaymentGateway _gateway;

    public DemoShopModel(IPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    [BindProperty]
    public string Amount { get; set; } = "249.90";

    [BindProperty]
    public string CardNumber { get; set; } = "4111111111111111";

    public string? Error { get; private set; }

    public static IReadOnlyList<(string Number, string Label)> TestCards { get; } =
    [
        ("4111111111111111", "Visa → MockBankA (ISO 8583 codes)"),
        ("5555555555554444", "Mastercard → MockBankB (XML)"),
        ("9792030000000018", "Troy → MockPspC (JSON)"),
        ("4111111111170000", "Visa ending 0000 → invalid card")
    ];

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var outcome = await _gateway.CreateAsync(
            new CreatePaymentCommand(
                MerchantId,
                $"DEMO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}",
                DemoAmount.Parse(Amount),
                "TRY",
                CardNumber,
                "/demo-magaza/sonuc"),
            cancellationToken).ConfigureAwait(false);
        if (outcome.Payment is null || outcome.Kind != PaymentResultKind.Created)
        {
            Error = outcome.Error ?? "Payment could not be started.";
            return Page();
        }

        return Redirect($"/mock-acs/{outcome.Payment.PaymentId}");
    }
}
