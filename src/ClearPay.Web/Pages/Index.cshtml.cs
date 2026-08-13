using System.Globalization;
using ClearPay.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    private readonly UserManager<ApplicationUser> _users;

    public IndexModel(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    public string GreetingName { get; private set; } = string.Empty;
    public string BalanceText { get; private set; } = FormatMoney(0m);
    public string MonthOutText { get; private set; } = FormatMoney(0m);
    public string MonthInText { get; private set; } = FormatMoney(0m);

    public async Task OnGetAsync()
    {
        var user = await _users.GetUserAsync(User);
        GreetingName = user?.FullName ?? user?.Email ?? User.Identity?.Name ?? string.Empty;
        BalanceText = FormatMoney(0m);
        MonthOutText = FormatMoney(0m);
        MonthInText = FormatMoney(0m);
    }

    private static string FormatMoney(decimal amount) => amount.ToString("N2", Turkish) + " ₺";
}
