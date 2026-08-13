using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class HareketlerModel : PageModel
{
    public void OnGet()
    {
    }
}
