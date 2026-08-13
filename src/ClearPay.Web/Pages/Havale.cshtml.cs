using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Web.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class HavaleModel : PageModel
{
    private readonly IWalletReader _wallets;

    public HavaleModel(IWalletReader wallets)
    {
        _wallets = wallets;
    }

    public string RemainingBalance { get; private set; } = MoneyDisplay.FormatTry(0m);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken);
        RemainingBalance = MoneyDisplay.FormatTry(summary?.Balance ?? 0m);
    }
}
