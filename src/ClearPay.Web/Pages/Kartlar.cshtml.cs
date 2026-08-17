using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Web.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace ClearPay.Web.Pages;

[Authorize]
public class KartlarModel : PageModel
{
    private readonly ILinkedInstrumentStore _cards;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public KartlarModel(ILinkedInstrumentStore cards, IStringLocalizer<SharedResource> localizer)
    {
        _cards = cards;
        _localizer = localizer;
    }

    public IReadOnlyList<LinkedInstrumentDto> Cards { get; private set; } = [];

    [BindProperty]
    public CardBindInput NewCard { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        await LoadAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var nickname = string.IsNullOrWhiteSpace(NewCard.Label) ? NewCard.Holder : NewCard.Label;
        if (!CardBindingParser.TryParse(NewCard.Number, nickname, out var last4, out var scheme, out var label))
        {
            await LoadAsync(cancellationToken).ConfigureAwait(false);
            ModelState.AddModelError(string.Empty, _localizer["CardAddFail"]);
            ClearSensitiveFields();
            return Page();
        }

        var added = await _cards.AddAsync(userId, last4, label, scheme, cancellationToken)
            .ConfigureAwait(false);
        ClearSensitiveFields();
        if (added is null)
        {
            await LoadAsync(cancellationToken).ConfigureAwait(false);
            ModelState.AddModelError(string.Empty, _localizer["CardAddFail"]);
            return Page();
        }

        TempData["Flash"] = _localizer["CardAddOk"].Value;
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        Cards = await _cards.ListAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    private void ClearSensitiveFields()
    {
        NewCard.Number = string.Empty;
        ModelState.Remove("NewCard.Number");
    }

    public sealed class CardBindInput
    {
        public string Number { get; set; } = string.Empty;

        public string Holder { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
