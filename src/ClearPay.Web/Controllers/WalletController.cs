using System.Security.Claims;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[Route("api/wallet")]
[JwtApi]
public sealed class WalletController : ControllerBase
{
    private readonly IWalletReader _wallets;

    public WalletController(IWalletReader wallets)
    {
        _wallets = wallets;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (summary is null)
        {
            return Ok(new
            {
                walletId = Guid.Empty,
                userId,
                balance = 0m,
                monthOutgoing = 0m,
                monthIncoming = 0m,
                isFrozen = false,
                lastMovements = Array.Empty<object>()
            });
        }

        return Ok(new
        {
            walletId = summary.WalletId,
            userId = summary.UserId,
            balance = summary.Balance,
            monthOutgoing = summary.MonthOutgoing,
            monthIncoming = summary.MonthIncoming,
            isFrozen = summary.IsFrozen,
            lastMovements = summary.LastMovements.Select(m => new
            {
                at = m.At,
                kind = m.Kind,
                amount = m.Amount,
                correlationId = m.CorrelationId
            })
        });
    }
}
