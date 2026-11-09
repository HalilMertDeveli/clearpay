using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    public decimal BalanceAmount { get; private set; }
    public decimal MonthOutAmount { get; private set; }
    public decimal MonthInAmount { get; private set; }
    public bool IsFrozen { get; private set; }
    public IReadOnlyList<WalletMovement> LastMovements { get; private set; } = [];
    public string? Flash { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Flash = TempData["Flash"] as string;
        var user = await _users.GetUserAsync(User);
        GreetingName = user?.FullName ?? user?.Email ?? User.Identity?.Name ?? string.Empty;

        var userId = _users.GetUserId(User)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;
        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken);
        BalanceAmount = summary?.Balance ?? 0m;
        MonthOutAmount = summary?.MonthOutgoing ?? 0m;
        MonthInAmount = summary?.MonthIncoming ?? 0m;
        BalanceText = MoneyDisplay.FormatTry(BalanceAmount);
        MonthOutText = MoneyDisplay.FormatTry(MonthOutAmount);
        MonthInText = MoneyDisplay.FormatTry(MonthInAmount);
        IsFrozen = summary?.IsFrozen ?? false;
        LastMovements = summary?.LastMovements ?? [];
        // #region agent log
        AgentDebugLog.Write("H", "Index.cshtml.cs:OnGetAsync", "overview", new { authed = User.Identity?.IsAuthenticated == true, frozen = IsFrozen, hasName = !string.IsNullOrWhiteSpace(GreetingName) });
        // #endregion
    }
}
