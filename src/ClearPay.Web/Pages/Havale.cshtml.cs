using System.Globalization;
using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using ClearPay.Web.Localization;
using ClearPay.Web.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Pages;

[Authorize]
public class HavaleModel : PageModel
{
    private readonly IWalletReader _wallets;
    private readonly ITransferExecutor _transfers;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public HavaleModel(
        IWalletReader wallets,
        ITransferExecutor transfers,
        IStringLocalizer<SharedResource> localizer)
    {
        _wallets = wallets;
        _transfers = transfers;
        _localizer = localizer;
    }

    [BindProperty]
    public TransferInput Input { get; set; } = new();

    public string RemainingBalance { get; private set; } = MoneyDisplay.FormatTry(0m);

    public bool CanSend { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Input.IdempotencyKey = Guid.NewGuid().ToString("N");
        await LoadBalanceAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadBalanceAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(Input.IdempotencyKey))
            Input.IdempotencyKey = Guid.NewGuid().ToString("N");

        if (!TryParseAmount(Input.Amount, out var amount))
        {
            ModelState.AddModelError(nameof(Input.Amount), _localizer["TransferInvalidAmount"]);
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var outcome = await _transfers.ExecuteAsync(
            new TransferCommand(
                userId,
                Input.Recipient ?? string.Empty,
                amount,
                Input.Description,
                Input.IdempotencyKey),
            cancellationToken).ConfigureAwait(false);

        if (outcome.IsSuccess)
        {
            TempData["Flash"] = _localizer["TransferSuccess"].Value;
            return RedirectToPage("/Dekont", new { correlationId = outcome.CorrelationId });
        }

        ModelState.AddModelError(string.Empty, MapError(outcome.Kind));
        return Page();
    }

    private async Task LoadBalanceAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        RemainingBalance = MoneyDisplay.FormatTry(summary?.Balance ?? 0m);
        CanSend = summary is { Balance: > 0m, IsFrozen: false };
    }

    private string MapError(TransferResultKind kind) => kind switch
    {
        TransferResultKind.Replay or TransferResultKind.KeyPayloadMismatch => _localizer["TransferReplay"],
        TransferResultKind.InsufficientFunds => _localizer["TransferInsufficient"],
        TransferResultKind.FrozenSender => _localizer["TransferFrozen"],
        TransferResultKind.SelfTransfer => _localizer["TransferSelf"],
        TransferResultKind.RecipientNotFound => _localizer["TransferRecipientMissing"],
        TransferResultKind.InvalidAmount => _localizer["TransferInvalidAmount"],
        TransferResultKind.MissingKey => _localizer["TransferMissingKey"],
        TransferResultKind.Unavailable => _localizer["TransferUnavailable"],
        _ => _localizer["TransferInvalidAmount"]
    };

    private static bool TryParseAmount(string? raw, out decimal amount)
    {
        amount = 0m;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var trimmed = raw.Replace("₺", string.Empty, StringComparison.Ordinal).Trim();
        var tr = CultureInfo.GetCultureInfo("tr-TR");
        if (!decimal.TryParse(trimmed, NumberStyles.Number, tr, out amount)
            && !decimal.TryParse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
        {
            return false;
        }

        return amount > 0m && decimal.Round(amount, 2) == amount;
    }

    public sealed class TransferInput
    {
        public string Recipient { get; set; } = string.Empty;

        public string Amount { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
