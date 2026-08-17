using System.Globalization;
using System.Security.Claims;
using ClearPay.Application.Banking;
using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Web.Localization;
using ClearPay.Web.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Pages;

[Authorize]
public class YukleCekModel : PageModel
{
    private readonly IWalletReader _wallets;
    private readonly IFundingExecutor _funding;
    private readonly ILinkedInstrumentStore _cards;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public YukleCekModel(
        IWalletReader wallets,
        IFundingExecutor funding,
        ILinkedInstrumentStore cards,
        IStringLocalizer<SharedResource> localizer)
    {
        _wallets = wallets;
        _funding = funding;
        _cards = cards;
        _localizer = localizer;
    }

    public string BalanceText { get; private set; } = MoneyDisplay.FormatTry(0m);

    public IReadOnlyList<LinkedInstrumentDto> Cards { get; private set; } = [];

    public LinkedInstrumentDto? SelectedCard { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Kart { get; set; }

    [BindProperty]
    public FundingInput TopUp { get; set; } = new();

    [BindProperty]
    public FundingInput Withdraw { get; set; } = new();

    [BindProperty]
    public CardInput NewCard { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        TopUp.IdempotencyKey = Guid.NewGuid().ToString("N");
        Withdraw.IdempotencyKey = Guid.NewGuid().ToString("N");
        await LoadAsync(cancellationToken).ConfigureAwait(false);
        ApplySelectedHint();
    }

    public Task<IActionResult> OnPostTopUpAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(BankOperation.TopUp, TopUp, cancellationToken);

    public Task<IActionResult> OnPostWithdrawAsync(CancellationToken cancellationToken) =>
        ExecuteAsync(BankOperation.Withdraw, Withdraw, cancellationToken);

    public async Task<IActionResult> OnPostAddCardAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var added = await _cards.AddAsync(userId, NewCard.Last4, NewCard.Label, cancellationToken)
            .ConfigureAwait(false);
        if (added is null)
        {
            await LoadAsync(cancellationToken).ConfigureAwait(false);
            ModelState.AddModelError(string.Empty, _localizer["CardAddFail"]);
            TopUp.IdempotencyKey = Guid.NewGuid().ToString("N");
            Withdraw.IdempotencyKey = Guid.NewGuid().ToString("N");
            return Page();
        }

        TempData["Flash"] = _localizer["CardAddOk"].Value;
        return RedirectToPage(new { kart = added.Last4 });
    }

    private async Task<IActionResult> ExecuteAsync(
        BankOperation operation,
        FundingInput input,
        CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(input.IdempotencyKey))
            input.IdempotencyKey = Guid.NewGuid().ToString("N");
        if (string.IsNullOrWhiteSpace(TopUp.IdempotencyKey))
            TopUp.IdempotencyKey = Guid.NewGuid().ToString("N");
        if (string.IsNullOrWhiteSpace(Withdraw.IdempotencyKey))
            Withdraw.IdempotencyKey = Guid.NewGuid().ToString("N");

        if (!TryParseAmount(input.Amount, out var amount))
        {
            ModelState.AddModelError(string.Empty, _localizer["TransferInvalidAmount"]);
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var outcome = await _funding.ExecuteAsync(
            new FundingCommand(
                userId,
                operation,
                amount,
                input.Account ?? string.Empty,
                input.IdempotencyKey),
            cancellationToken).ConfigureAwait(false);

        if (outcome.IsSuccess)
        {
            TempData["Flash"] = operation == BankOperation.TopUp
                ? _localizer["TopUpSuccess"].Value
                : _localizer["WithdrawSuccess"].Value;
            return RedirectToPage("/Dekont", new { correlationId = outcome.CorrelationId });
        }

        ModelState.AddModelError(string.Empty, MapError(outcome.Kind));
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var summary = await _wallets.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        BalanceText = MoneyDisplay.FormatTry(summary?.Balance ?? 0m);
        Cards = await _cards.ListAsync(userId, cancellationToken).ConfigureAwait(false);
        ApplySelectedHint();
    }

    private void ApplySelectedHint()
    {
        var last4 = Kart;
        if (string.IsNullOrWhiteSpace(last4) || last4.Length != 4 || !last4.All(char.IsDigit))
        {
            SelectedCard = Cards.FirstOrDefault();
        }
        else
        {
            SelectedCard = Cards.FirstOrDefault(c => c.Last4 == last4) ?? Cards.FirstOrDefault();
        }

        if (SelectedCard is null)
            return;
        if (string.IsNullOrWhiteSpace(TopUp.Account))
            TopUp.Account = SelectedCard.AccountHint;
        if (string.IsNullOrWhiteSpace(Withdraw.Account))
            Withdraw.Account = SelectedCard.AccountHint;
    }

    private string MapError(FundingResultKind kind) => kind switch
    {
        FundingResultKind.Replay => _localizer["FundingReplay"],
        FundingResultKind.TimedOut => _localizer["FundingTimeout"],
        FundingResultKind.InsufficientFunds => _localizer["TransferInsufficient"],
        FundingResultKind.Frozen => _localizer["WithdrawFrozen"],
        FundingResultKind.GatewayFailed => _localizer["FundingGatewayFailed"],
        FundingResultKind.MissingKey => _localizer["TransferMissingKey"],
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

    public sealed class FundingInput
    {
        public string Amount { get; set; } = string.Empty;

        public string Account { get; set; } = string.Empty;

        public string IdempotencyKey { get; set; } = string.Empty;
    }

    public sealed class CardInput
    {
        public string Last4 { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
