using System.Security.Claims;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[Route("api/movements")]
[JwtApi]
public sealed class MovementsController : ControllerBase
{
    private readonly IActivityReader _activity;

    public MovementsController(IActivityReader activity)
    {
        _activity = activity;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? kind,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        DateTimeOffset? fromUtc = from is DateTime d
            ? new DateTimeOffset(DateTime.SpecifyKind(d.Date, DateTimeKind.Utc))
            : null;
        DateTimeOffset? toUtc = to is DateTime end
            ? new DateTimeOffset(DateTime.SpecifyKind(end.Date.AddDays(1), DateTimeKind.Utc))
            : null;

        var result = await _activity.ListAsync(userId, fromUtc, toUtc, kind, page, cancellationToken)
            .ConfigureAwait(false);
        return Ok(new
        {
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            items = result.Items.Select(i => new
            {
                at = i.At,
                correlationId = i.CorrelationId,
                kind = i.Kind,
                counterparty = i.Counterparty,
                signedAmount = i.SignedAmount,
                status = i.Status
            })
        });
    }
}
