using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Web.Api;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[Route("api/cards")]
[JwtApi]
public sealed class CardsController : ControllerBase
{
    private readonly ILinkedInstrumentStore _cards;

    public CardsController(ILinkedInstrumentStore cards)
    {
        _cards = cards;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var items = await _cards.ListAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            items = items.Select(c => new
            {
                id = c.Id,
                last4 = c.Last4,
                label = c.Label,
                accountHint = c.AccountHint
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CardApiRequest body, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var added = await _cards.AddAsync(
            userId,
            body?.Last4 ?? string.Empty,
            body?.Label ?? string.Empty,
            cancellationToken).ConfigureAwait(false);
        if (added is null)
        {
            return Problem(
                title: "Card not added",
                detail: "Last4 must be 4 digits and unique per wallet. Ledger unchanged.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Created($"/api/cards/{added.Id}", new
        {
            id = added.Id,
            last4 = added.Last4,
            label = added.Label,
            accountHint = added.AccountHint
        });
    }
}
