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
    private readonly IReceiptPdf _pdf;

    public DekontModel(IActivityReader activity, IReceiptPdf pdf)
    {
        _activity = activity;
        _pdf = pdf;
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

    public async Task<IActionResult> OnGetPdfAsync(Guid correlationId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var receipt = await _activity.GetReceiptAsync(userId, correlationId, cancellationToken).ConfigureAwait(false);
        if (receipt is null)
            return NotFound();
        var bytes = _pdf.Render(receipt);
        return File(bytes, "application/pdf", $"clearpay-dekont-{correlationId:N}.pdf");
    }
}
