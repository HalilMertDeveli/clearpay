using System.Security.Claims;
using ClearPay.Application.Banking;
using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Web.Api;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[JwtApi]
public sealed class FundingController : ControllerBase
{
    private readonly IFundingExecutor _funding;

    public FundingController(IFundingExecutor funding)
    {
        _funding = funding;
    }

    [HttpPost("/api/topup")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> TopUp([FromBody] FundingApiRequest body, CancellationToken cancellationToken) =>
        Execute(BankOperation.TopUp, body, cancellationToken);

    [HttpPost("/api/withdraw")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public Task<IActionResult> Withdraw([FromBody] FundingApiRequest body, CancellationToken cancellationToken) =>
        Execute(BankOperation.Withdraw, body, cancellationToken);

    private async Task<IActionResult> Execute(
        BankOperation operation,
        FundingApiRequest body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Unauthorized();

        var key = Request.Headers["Idempotency-Key"].ToString();
        var outcome = await _funding.ExecuteAsync(
            new FundingCommand(
                actor,
                operation,
                body?.Amount ?? 0m,
                body?.Account ?? string.Empty,
                key),
            cancellationToken).ConfigureAwait(false);
        return Map(outcome);
    }

    private IActionResult Map(FundingOutcome outcome)
    {
        return outcome.Kind switch
        {
            FundingResultKind.Created => Created(
                $"/api/receipts/{outcome.CorrelationId}",
                new { correlationId = outcome.CorrelationId, gatewayReference = outcome.GatewayReference }),
            FundingResultKind.Replay => ConflictProblem(outcome),
            FundingResultKind.TimedOut => Accepted(new
            {
                correlationId = outcome.CorrelationId,
                detail = "Gateway timed out. Nothing was posted to the ledger. Retry with the same Idempotency-Key."
            }),
            FundingResultKind.InsufficientFunds => Problem(
                title: "Insufficient funds",
                detail: "Balance is not enough for this withdrawal.",
                statusCode: StatusCodes.Status422UnprocessableEntity),
            FundingResultKind.Frozen => Problem(
                title: "Wallet frozen",
                detail: "Frozen wallets cannot withdraw.",
                statusCode: StatusCodes.Status403Forbidden),
            FundingResultKind.MissingKey => Problem(
                title: "Idempotency-Key required",
                detail: "Send the Idempotency-Key header. The same key returns 409 on replay.",
                statusCode: StatusCodes.Status400BadRequest),
            FundingResultKind.GatewayFailed => Problem(
                title: "Gateway failed",
                detail: "Fake bank gateway rejected the request. Ledger unchanged.",
                statusCode: StatusCodes.Status502BadGateway),
            _ => Problem(
                title: "Invalid amount",
                detail: "Amount must be greater than zero with at most two decimal places.",
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private IActionResult ConflictProblem(FundingOutcome outcome)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            detail: "This funding request was already processed. The wallet was not charged again.");
        problem.Extensions["correlationId"] = outcome.CorrelationId;
        return Conflict(problem);
    }
}
