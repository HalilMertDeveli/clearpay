using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Web.Api;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[Route("api/admin")]
[JwtApi("Admin")]
public sealed class AdminApiController : ControllerBase
{
    private readonly IAdminPanel _admin;

    public AdminApiController(IAdminPanel admin)
    {
        _admin = admin;
    }

    [HttpGet("outbox")]
    public async Task<IActionResult> Outbox(CancellationToken cancellationToken)
    {
        var items = await _admin.ListFailedAsync(cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            items = items.Select(i => new
            {
                id = i.Id,
                type = i.Type,
                correlationId = i.CorrelationId,
                occurredAt = i.OccurredAt,
                payload = i.Payload
            })
        });
    }

    [HttpGet("audit")]
    public async Task<IActionResult> Audit(
        [FromQuery] string? actor,
        [FromQuery] Guid? correlationId,
        CancellationToken cancellationToken)
    {
        var items = await _admin.SearchAuditAsync(actor, correlationId, from: null, cancellationToken)
            .ConfigureAwait(false);
        return Ok(new
        {
            items = items.Select(i => new
            {
                id = i.Id,
                actorUserId = i.ActorUserId,
                action = i.Action,
                correlationId = i.CorrelationId,
                createdAt = i.CreatedAt,
                details = i.Details
            })
        });
    }

    [HttpPost("freeze")]
    public Task<IActionResult> Freeze([FromBody] EmailApiRequest body, CancellationToken cancellationToken) =>
        Toggle(freeze: true, body, cancellationToken);

    [HttpPost("unfreeze")]
    public Task<IActionResult> Unfreeze([FromBody] EmailApiRequest body, CancellationToken cancellationToken) =>
        Toggle(freeze: false, body, cancellationToken);

    [HttpPost("outbox/{id:guid}/requeue")]
    public async Task<IActionResult> Requeue(Guid id, CancellationToken cancellationToken)
    {
        var ok = await _admin.RequeueAsync(id, cancellationToken).ConfigureAwait(false);
        return ok ? Ok(new { requeued = true }) : NotFound();
    }

    private async Task<IActionResult> Toggle(bool freeze, EmailApiRequest body, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Unauthorized();

        var email = body?.Email ?? string.Empty;
        var ok = freeze
            ? await _admin.FreezeByEmailAsync(email, actor, cancellationToken).ConfigureAwait(false)
            : await _admin.UnfreezeByEmailAsync(email, actor, cancellationToken).ConfigureAwait(false);
        if (!ok)
        {
            return Problem(
                title: freeze ? "Freeze missed" : "Unfreeze missed",
                detail: "No customer wallet matched that email.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(new { email, frozen = freeze });
    }
}
