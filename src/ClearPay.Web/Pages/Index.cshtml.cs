using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IWalletReader _wallets;

    public IndexModel(UserManager<ApplicationUser> users, IWalletReader wallets)
    {
        _users = users;
        _wallets = wallets;
    }

    public string GreetingName { get; private set; } = string.Empty;
    public string BalanceText { get; private set; } = MoneyDisplay.FormatTry(0m);
    public string MonthOutText { get; private set; } = MoneyDisplay.FormatTry(0m);
    public string MonthInText { get; private set; } = MoneyDisplay.FormatTry(0m);
    public bool IsFrozen { get; private set; }
    public IReadOnlyList<WalletMovement> LastMovements { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _users.GetUserAsync(User);
        GreetingName = user?.FullName ?? user?.Email ?? User.Identity?.Name ?? string.Empty;

        var userId = _users.GetUserId(User)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;
        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken);
        BalanceText = MoneyDisplay.FormatTry(summary?.Balance ?? 0m);
        MonthOutText = MoneyDisplay.FormatTry(summary?.MonthOutgoing ?? 0m);
        MonthInText = MoneyDisplay.FormatTry(summary?.MonthIncoming ?? 0m);
        IsFrozen = summary?.IsFrozen ?? false;
        LastMovements = summary?.LastMovements ?? [];
    }
}
