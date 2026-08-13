using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Web.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class YukleCekModel : PageModel
{
    private readonly IWalletReader _wallets;

    public YukleCekModel(IWalletReader wallets)
    {
        _wallets = wallets;
    }

    public string BalanceText { get; private set; } = MoneyDisplay.FormatTry(0m);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken);
        BalanceText = MoneyDisplay.FormatTry(summary?.Balance ?? 0m);
    }
}
