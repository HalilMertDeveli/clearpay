using System.Security.Claims;
using ClearPay.Application.Activity;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class HareketlerModel : PageModel
{
    private readonly IActivityReader _activity;

    public HareketlerModel(IActivityReader activity)
    {
        _activity = activity;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? Baslangic { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Tur { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Sayfa { get; set; } = 1;

    public ActivityPage Result { get; private set; } = new([], 1, 20, 0);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        DateTimeOffset? from = Baslangic is DateTime d
            ? new DateTimeOffset(DateTime.SpecifyKind(d, DateTimeKind.Utc))
            : null;
        Result = await _activity.ListAsync(userId, from, Tur, Sayfa, cancellationToken).ConfigureAwait(false);
    }
}
