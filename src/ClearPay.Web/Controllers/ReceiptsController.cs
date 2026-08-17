using System.Security.Claims;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[Route("api/receipts")]
[JwtApi]
public sealed class ReceiptsController : ControllerBase
{
    private readonly IActivityReader _activity;

    public ReceiptsController(IActivityReader activity)
    {
        _activity = activity;
    }

    [HttpGet("{correlationId:guid}")]
    public async Task<IActionResult> Get(Guid correlationId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var receipt = await _activity.GetReceiptAsync(userId, correlationId, cancellationToken)
            .ConfigureAwait(false);
        if (receipt is null)
            return NotFound();

        return Ok(new
        {
            correlationId = receipt.CorrelationId,
            at = receipt.At,
            kind = receipt.Kind,
            amount = receipt.Amount,
            debitParty = receipt.DebitParty,
            creditParty = receipt.CreditParty,
            description = receipt.Description,
            instrumentHint = receipt.InstrumentHint
        });
    }
}
