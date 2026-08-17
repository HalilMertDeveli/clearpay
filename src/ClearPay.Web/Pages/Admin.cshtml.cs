using System.Security.Claims;
using ClearPay.Application.Admin;
using ClearPay.Application.Ports;
using ClearPay.Domain.Identity;
using ClearPay.Web.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Pages;

[Authorize(Roles = AppRoles.Admin)]
public class AdminModel : PageModel
{
    private readonly IAdminPanel _admin;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AdminModel(IAdminPanel admin, IStringLocalizer<SharedResource> localizer)
    {
        _admin = admin;
        _localizer = localizer;
    }

    [BindProperty]
    public string? FreezeEmail { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AuditActor { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AuditCorrelation { get; set; }

    public IReadOnlyList<FailedOutboxItem> Failed { get; private set; } = [];
    public IReadOnlyList<AuditItem> Audits { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostFreezeAsync(CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(FreezeEmail)
            || !await _admin.FreezeByEmailAsync(FreezeEmail, actor, cancellationToken).ConfigureAwait(false))
        {
            ModelState.AddModelError(string.Empty, _localizer["AdminFreezeMiss"]);
        }
        else
        {
            TempData["Flash"] = _localizer["AdminFreezeOk"].Value;
        }

        await LoadAsync(cancellationToken).ConfigureAwait(false);
        return Page();
    }

    public async Task<IActionResult> OnPostUnfreezeAsync(CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(FreezeEmail)
            || !await _admin.UnfreezeByEmailAsync(FreezeEmail, actor, cancellationToken).ConfigureAwait(false))
        {
            ModelState.AddModelError(string.Empty, _localizer["AdminUnfreezeMiss"]);
        }
        else
        {
            TempData["Flash"] = _localizer["AdminUnfreezeOk"].Value;
        }

        await LoadAsync(cancellationToken).ConfigureAwait(false);
        return Page();
    }

    public async Task<IActionResult> OnPostRequeueAsync(Guid id, CancellationToken cancellationToken)
    {
        await _admin.RequeueAsync(id, cancellationToken).ConfigureAwait(false);
        await LoadAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Failed = await _admin.ListFailedAsync(cancellationToken).ConfigureAwait(false);
        Guid? cid = Guid.TryParse(AuditCorrelation, out var parsed) ? parsed : null;
        Audits = await _admin.SearchAuditAsync(AuditActor, cid, from: null, cancellationToken)
            .ConfigureAwait(false);
    }
}
