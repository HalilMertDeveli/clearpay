using System.Globalization;
using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages.Payments;

/// <summary>
/// Merchant return URL. The query string is only a hint: the page re-reads the payment from the
/// gateway (a shopper could edit <c>?status=</c>). Buttons drive capture / void / refund as the demo merchant.
/// </summary>
[AllowAnonymous]
public sealed class DemoShopResultModel : PageModel
{
    private readonly IPaymentGateway _gateway;

    public DemoShopResultModel(IPaymentGateway gateway)
    {
        _gateway = gateway;
    }

    public PaymentView? Payment { get; private set; }

    public string? Message { get; private set; }

    [BindProperty]
    public string? RefundAmount { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        await LoadAsync(paymentId, cancellationToken).ConfigureAwait(false);
        return Payment is null ? NotFound() : Page();
    }

    public Task<IActionResult> OnPostCaptureAsync(Guid paymentId, CancellationToken cancellationToken) =>
        RunAsync(paymentId, () => _gateway.CaptureAsync(DemoShopModel.MerchantId, paymentId, cancellationToken), cancellationToken);

    public Task<IActionResult> OnPostVoidAsync(Guid paymentId, CancellationToken cancellationToken) =>
        RunAsync(paymentId, () => _gateway.VoidAsync(DemoShopModel.MerchantId, paymentId, cancellationToken), cancellationToken);

    public Task<IActionResult> OnPostRefundAsync(Guid paymentId, CancellationToken cancellationToken) =>
        RunAsync(
            paymentId,
            () => _gateway.RefundAsync(new RefundPaymentCommand(DemoShopModel.MerchantId, paymentId, DemoAmount.Parse(RefundAmount), "demo"), cancellationToken),
            cancellationToken);

    private async Task<IActionResult> RunAsync(Guid paymentId, Func<Task<PaymentOutcome>> action, CancellationToken cancellationToken)
    {
        var outcome = await action().ConfigureAwait(false);
        Message = outcome.IsSuccess ? "OK" : $"{outcome.Kind}: {outcome.Error}";
        await LoadAsync(paymentId, cancellationToken).ConfigureAwait(false);
        return Payment is null ? NotFound() : Page();
    }

    private async Task LoadAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var view = await _gateway.GetAsync(paymentId, cancellationToken).ConfigureAwait(false);
        Payment = view is not null && view.MerchantId == DemoShopModel.MerchantId ? view : null;
        if (Payment is not null && string.IsNullOrWhiteSpace(RefundAmount))
            RefundAmount = (Payment.CapturedAmount - Payment.RefundedAmount).ToString("F2", CultureInfo.InvariantCulture);
    }
}
