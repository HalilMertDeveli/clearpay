using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using ClearPay.Web.Api;
using ClearPay.Web.Payments;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

/// <summary>
/// Merchant-facing card payment API (mock acquiring gateway). Auth: <c>X-Merchant-Key</c>.
/// HTTP mapping only; rules are in Domain <c>Payment</c> and <see cref="IPaymentGateway"/>.
/// </summary>
[ApiController]
[Route("api/payments")]
[MerchantKey]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentGateway _gateway;

    public PaymentsController(IPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    /// <summary>Starts a payment. Response carries <c>redirectUrl</c>: send the shopper's browser there for 3DS.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentApiRequest body, CancellationToken cancellationToken)
    {
        var outcome = await _gateway.CreateAsync(
            new CreatePaymentCommand(
                MerchantKeyFilter.MerchantId(HttpContext),
                body?.MerchantOrderId ?? string.Empty,
                body?.Amount ?? 0m,
                body?.Currency ?? "TRY",
                body?.CardNumber ?? string.Empty,
                body?.ReturnUrl ?? string.Empty),
            cancellationToken).ConfigureAwait(false);

        if (outcome.Kind == PaymentResultKind.Created && outcome.Payment is { } created)
        {
            return Created($"/api/payments/{created.PaymentId}", new
            {
                paymentId = created.PaymentId,
                status = created.Status,
                provider = created.Provider,
                redirectUrl = MockAcsUrl(created.PaymentId)
            });
        }

        return Map(outcome);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var view = await _gateway.GetAsync(id, cancellationToken).ConfigureAwait(false);
        if (view is null || view.MerchantId != MerchantKeyFilter.MerchantId(HttpContext))
            return NotFound();
        return Ok(view);
    }

    [HttpPost("{id:guid}/capture")]
    public async Task<IActionResult> Capture(Guid id, CancellationToken cancellationToken) =>
        Map(await _gateway.CaptureAsync(MerchantKeyFilter.MerchantId(HttpContext), id, cancellationToken).ConfigureAwait(false));

    /// <summary>Cancel an authorization before capture.</summary>
    [HttpPost("{id:guid}/void")]
    public async Task<IActionResult> Void(Guid id, CancellationToken cancellationToken) =>
        Map(await _gateway.VoidAsync(MerchantKeyFilter.MerchantId(HttpContext), id, cancellationToken).ConfigureAwait(false));

    /// <summary>Full or partial refund. The sum of refunds can never exceed the captured amount.</summary>
    [HttpPost("{id:guid}/refunds")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundApiRequest body, CancellationToken cancellationToken)
    {
        var outcome = await _gateway.RefundAsync(
            new RefundPaymentCommand(MerchantKeyFilter.MerchantId(HttpContext), id, body?.Amount ?? 0m, body?.Reason),
            cancellationToken).ConfigureAwait(false);
        if (outcome.Kind == PaymentResultKind.Created && outcome.Payment is { } payment)
            return Created($"/api/payments/{payment.PaymentId}", payment);
        return Map(outcome);
    }

    internal static string MockAcsUrl(Guid paymentId) => $"/mock-acs/{paymentId}";

    private IActionResult Map(PaymentOutcome outcome)
    {
        if (outcome.IsSuccess && outcome.Payment is not null)
            return Ok(outcome.Payment);

        var status = outcome.Kind switch
        {
            PaymentResultKind.NotFound => StatusCodes.Status404NotFound,
            PaymentResultKind.DuplicateOrder => StatusCodes.Status409Conflict,
            PaymentResultKind.ConcurrencyConflict => StatusCodes.Status409Conflict,
            PaymentResultKind.InvalidState => StatusCodes.Status409Conflict,
            PaymentResultKind.RefundLimitExceeded => StatusCodes.Status422UnprocessableEntity,
            PaymentResultKind.AcquirerDeclined => StatusCodes.Status402PaymentRequired,
            _ => StatusCodes.Status400BadRequest
        };
        var problem = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: status,
            title: outcome.Kind.ToString(),
            detail: outcome.Error);
        if (outcome.Payment is { } payment)
        {
            problem.Extensions["paymentId"] = payment.PaymentId;
            problem.Extensions["status"] = payment.Status;
        }

        return new ObjectResult(problem) { StatusCode = status };
    }
}
