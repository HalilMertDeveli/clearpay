using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace ClearPay.Web.Controllers;

/// <summary>
/// Where the (mock) bank ACS posts the 3DS result through the shopper's browser.
/// No cookie or merchant key: trust comes only from the HMAC signature, which the gateway verifies
/// against the amount and 3DS id it stored. Then the browser is redirected to the merchant return URL.
/// </summary>
[ApiController]
[Route("api/payments/3ds/callback")]
public sealed class ThreeDsCallbackController : ControllerBase
{
    private readonly IPaymentGateway _gateway;

    public ThreeDsCallbackController(IPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromForm] ThreeDsCallbackForm form, CancellationToken cancellationToken)
    {
        var outcome = await _gateway.CompleteThreeDsAsync(
            new ThreeDsCallback(
                form.PaymentId,
                form.ThreeDsTransactionId ?? string.Empty,
                form.Result ?? string.Empty,
                form.Signature ?? string.Empty),
            cancellationToken).ConfigureAwait(false);

        switch (outcome.Kind)
        {
            case PaymentResultKind.Ok:
            case PaymentResultKind.AlreadyProcessed when outcome.Payment is not null:
                var payment = outcome.Payment!;
                var target = QueryHelpers.AddQueryString(payment.ReturnUrl, new Dictionary<string, string?>
                {
                    ["paymentId"] = payment.PaymentId.ToString(),
                    ["orderId"] = payment.MerchantOrderId,
                    ["status"] = payment.Status
                });
                return Redirect(target);
            case PaymentResultKind.NotFound:
                return NotFound();
            default:
                return Problem(
                    title: "3DS callback rejected",
                    detail: outcome.Error,
                    statusCode: StatusCodes.Status400BadRequest);
        }
    }
}

public sealed class ThreeDsCallbackForm
{
    public Guid PaymentId { get; set; }

    public string? ThreeDsTransactionId { get; set; }

    public string? Result { get; set; }

    public string? Signature { get; set; }
}
