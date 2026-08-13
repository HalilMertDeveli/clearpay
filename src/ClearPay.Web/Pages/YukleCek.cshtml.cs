using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class YukleCekModel : PageModel
{
    public string BalanceText { get; } = "0,00 ₺";
}
