using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using ClearPay.Web.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[Route("api/transfers")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class TransfersController : ControllerBase
{
    private readonly ITransferExecutor _transfers;

    public TransfersController(ITransferExecutor transfers)
    {
        _transfers = transfers;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Post(
        [FromBody] TransferApiRequest body,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = Request.Headers["Idempotency-Key"].ToString();
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Unauthorized();

        var command = new TransferCommand(
            actor,
            body?.Recipient ?? string.Empty,
            body?.Amount ?? 0m,
            body?.Description,
            key);
        var outcome = await _transfers.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
        return Map(outcome);
    }

    private IActionResult Map(TransferOutcome outcome)
    {
        return outcome.Kind switch
        {
            TransferResultKind.Created => Created(
                $"/api/transfers/{outcome.TransferId}",
                new { transferId = outcome.TransferId, correlationId = outcome.CorrelationId }),
            TransferResultKind.Replay or TransferResultKind.KeyPayloadMismatch => ConflictProblem(
                "This transfer was already processed. The wallet was not charged again.",
                outcome),
            TransferResultKind.InsufficientFunds => Problem(
                title: "Insufficient funds",
                detail: "Balance is not enough for this amount.",
                statusCode: StatusCodes.Status422UnprocessableEntity),
            TransferResultKind.FrozenSender => Problem(
                title: "Wallet frozen",
                detail: "Frozen wallets cannot send.",
                statusCode: StatusCodes.Status403Forbidden),
            TransferResultKind.SelfTransfer => Problem(
                title: "Invalid recipient",
                detail: "You cannot send to yourself.",
                statusCode: StatusCodes.Status400BadRequest),
            TransferResultKind.RecipientNotFound => Problem(
                title: "Recipient not found",
                detail: "No customer is registered with that email.",
                statusCode: StatusCodes.Status404NotFound),
            TransferResultKind.InvalidAmount => Problem(
                title: "Invalid amount",
                detail: "Amount must be greater than zero with at most two decimal places.",
                statusCode: StatusCodes.Status400BadRequest),
            TransferResultKind.MissingKey => Problem(
                title: "Idempotency-Key required",
                detail: "Send the Idempotency-Key header. The same key returns 409 on replay.",
                statusCode: StatusCodes.Status400BadRequest),
            TransferResultKind.Unavailable => Problem(
                title: "Ledger unavailable",
                detail: "SQL Server did not respond in time. Nothing was charged. Try again.",
                statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Problem(statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private IActionResult ConflictProblem(string detail, TransferOutcome outcome)
    {
        var problem = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            detail: detail);
        problem.Extensions["transferId"] = outcome.TransferId;
        problem.Extensions["correlationId"] = outcome.CorrelationId;
        return Conflict(problem);
    }
}
