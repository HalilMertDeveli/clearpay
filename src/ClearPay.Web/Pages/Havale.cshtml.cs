using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class HavaleModel : PageModel
{
    public string RemainingBalance { get; } = "0,00 ₺";
}
