using System.Security.Claims;
using ClearPay.Application.Activity;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages;

[Authorize]
public class DekontModel : PageModel
{
    private readonly IActivityReader _activity;

    public DekontModel(IActivityReader activity)
    {
        _activity = activity;
    }

    public ReceiptDto? Receipt { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid correlationId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        Receipt = await _activity.GetReceiptAsync(userId, correlationId, cancellationToken).ConfigureAwait(false);
        if (Receipt is null)
            return NotFound();
        return Page();
    }
}
