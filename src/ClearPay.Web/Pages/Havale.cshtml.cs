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

    public string? AfterSendText { get; private set; }

    public string? ConfirmAmountText { get; private set; }

    public bool CanSend { get; private set; }

    public bool Confirming { get; private set; }

    public string IdempotencyShort =>
        string.IsNullOrEmpty(Input.IdempotencyKey) || Input.IdempotencyKey.Length < 8
            ? Input.IdempotencyKey
            : Input.IdempotencyKey[^8..];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Input.IdempotencyKey = Guid.NewGuid().ToString("N");
        await LoadBalanceAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostReviewAsync(CancellationToken cancellationToken)
    {
        await LoadBalanceAsync(cancellationToken).ConfigureAwait(false);
        EnsureKey();
        if (!ValidateDraft())
            return Page();

        Confirming = true;
        ConfirmAmountText = MoneyDisplay.FormatTry(_draftAmount);
        AfterSendText = MoneyDisplay.FormatTry(_remainingAmount - _draftAmount);
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken cancellationToken)
    {
        await LoadBalanceAsync(cancellationToken).ConfigureAwait(false);
        Input.IdempotencyKey = Guid.NewGuid().ToString("N");
        Confirming = false;
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(CancellationToken cancellationToken)
    {
        await LoadBalanceAsync(cancellationToken).ConfigureAwait(false);
        EnsureKey();
        if (!ValidateDraft())
            return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var outcome = await _transfers.ExecuteAsync(
            new TransferCommand(
                userId,
                Input.Recipient ?? string.Empty,
                _draftAmount,
                Input.Description,
                Input.IdempotencyKey),
            cancellationToken).ConfigureAwait(false);

        if (outcome.IsSuccess
            || (outcome.Kind == TransferResultKind.Replay && outcome.CorrelationId != Guid.Empty))
        {
            TempData["Flash"] = outcome.IsSuccess
                ? _localizer["TransferSuccess"].Value
                : _localizer["TransferReplay"].Value;
            return RedirectToPage("/Dekont", new { correlationId = outcome.CorrelationId });
        }

        ModelState.AddModelError(string.Empty, MapError(outcome.Kind));
        Confirming = true;
        ConfirmAmountText = MoneyDisplay.FormatTry(_draftAmount);
        AfterSendText = MoneyDisplay.FormatTry(_remainingAmount - _draftAmount);
        return Page();
    }

    private decimal _remainingAmount;
    private decimal _draftAmount;

    private async Task LoadBalanceAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        _remainingAmount = summary?.Balance ?? 0m;
        RemainingBalance = MoneyDisplay.FormatTry(_remainingAmount);
        CanSend = summary is { Balance: > 0m, IsFrozen: false };
    }

    private void EnsureKey()
    {
        if (string.IsNullOrWhiteSpace(Input.IdempotencyKey))
            Input.IdempotencyKey = Guid.NewGuid().ToString("N");
    }

    private bool ValidateDraft()
    {
        if (string.IsNullOrWhiteSpace(Input.Recipient))
        {
            ModelState.AddModelError("Input.Recipient", _localizer["TransferRecipientRequired"]);
            return false;
        }

        if (!TryParseAmount(Input.Amount, out _draftAmount))
        {
            ModelState.AddModelError("Input.Amount", _localizer["TransferInvalidAmount"]);
            return false;
        }

        if (_draftAmount > _remainingAmount)
        {
            ModelState.AddModelError("Input.Amount", _localizer["TransferInsufficient"]);
            return false;
        }

        return ModelState.IsValid;
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
