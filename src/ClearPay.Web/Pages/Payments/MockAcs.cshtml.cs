using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages.Payments;

/// <summary>
/// Mock issuer ACS (3D Secure challenge page). In real life this is the card-issuing bank's site;
/// here it lives in the same host for the demo. OTP 123456 = authenticated (Y), anything else = N.
/// After the OTP it renders an auto-submitting form that POSTs the HMAC-signed result to the gateway.
/// </summary>
[AllowAnonymous]
public sealed class MockAcsModel : PageModel
{
    public const string DemoOtp = "123456";

    private readonly IPaymentGateway _gateway;
    private readonly ThreeDsSigner _signer;

    public MockAcsModel(IPaymentGateway gateway, ThreeDsSigner signer)
    {
        _gateway = gateway;
        _signer = signer;
    }

    public PaymentView? Payment { get; private set; }

    /// <summary>Set after the OTP step: the signed fields the browser will post to the callback.</summary>
    public SignedResult? Result { get; private set; }

    [BindProperty]
    public string? Otp { get; set; }

    public string BankName => Payment?.Provider switch
    {
        "MockBankA" => "Mock Bank A",
        "MockBankB" => "Mock Bank B",
        "MockPspC" => "Mock PSP C",
        _ => "Mock Bank"
    };

    public string Accent => Payment?.Provider switch
    {
        "MockBankB" => "#7a1f5c",
        "MockPspC" => "#0b6e6e",
        _ => "#1B2A4A"
    };

    public async Task<IActionResult> OnGetAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        Payment = await _gateway.GetAsync(paymentId, cancellationToken).ConfigureAwait(false);
        return Payment is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid paymentId, string? action, CancellationToken cancellationToken)
    {
        Payment = await _gateway.GetAsync(paymentId, cancellationToken).ConfigureAwait(false);
        if (Payment?.ThreeDsTransactionId is null)
            return NotFound();

        var authenticated = action != "cancel" && string.Equals(Otp?.Trim(), DemoOtp, StringComparison.Ordinal);
        var result = authenticated ? ThreeDsResult.Authenticated : ThreeDsResult.NotAuthenticated;
        Result = new SignedResult(
            Payment.PaymentId,
            Payment.ThreeDsTransactionId,
            result,
            _signer.Sign(Payment.PaymentId, Payment.ThreeDsTransactionId, result, Payment.Amount));
        return Page();
    }

    public sealed record SignedResult(Guid PaymentId, string ThreeDsTransactionId, string Result, string Signature);
}
