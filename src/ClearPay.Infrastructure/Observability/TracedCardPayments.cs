using System.Diagnostics;
using ClearPay.Application.Payments;
using ClearPay.Application.Ports;

namespace ClearPay.Infrastructure.Observability;

/// <summary>
/// Decorator: puts every card-payment use case (create, 3DS callback, capture, void, refund) in a
/// <c>ClearPay.Payments</c> span and records the same counter/histogram as wallet payments,
/// with <c>payment.kind</c> = <c>card_create</c>, <c>card_3ds</c>, ... so one Grafana dashboard shows both.
/// Span tags: payment id, merchant id, provider, status, result kind. Never PAN, BIN, amount, OTP or HMAC.
/// </summary>
public sealed class TracedPaymentGateway : IPaymentGateway
{
    private readonly IPaymentGateway _inner;

    public TracedPaymentGateway(IPaymentGateway inner) => _inner = inner;

    public Task<PaymentOutcome> CreateAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default) =>
        TraceAsync("card_create", command.MerchantId, null, () => _inner.CreateAsync(command, cancellationToken));

    public Task<PaymentOutcome> CompleteThreeDsAsync(ThreeDsCallback callback, CancellationToken cancellationToken = default) =>
        TraceAsync("card_3ds", null, callback.PaymentId, () => _inner.CompleteThreeDsAsync(callback, cancellationToken));

    public Task<PaymentOutcome> CaptureAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default) =>
        TraceAsync("card_capture", merchantId, paymentId, () => _inner.CaptureAsync(merchantId, paymentId, cancellationToken));

    public Task<PaymentOutcome> VoidAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default) =>
        TraceAsync("card_void", merchantId, paymentId, () => _inner.VoidAsync(merchantId, paymentId, cancellationToken));

    public Task<PaymentOutcome> RefundAsync(RefundPaymentCommand command, CancellationToken cancellationToken = default) =>
        TraceAsync("card_refund", command.MerchantId, command.PaymentId, () => _inner.RefundAsync(command, cancellationToken));

    /// <summary>Plain read: no span of its own (the HTTP and SQL spans already cover it), no metric.</summary>
    public Task<PaymentView?> GetAsync(Guid paymentId, CancellationToken cancellationToken = default) =>
        _inner.GetAsync(paymentId, cancellationToken);

    private static async Task<PaymentOutcome> TraceAsync(
        string kind,
        string? merchantId,
        Guid? paymentId,
        Func<Task<PaymentOutcome>> call)
    {
        using var activity = ClearPayTelemetry.Payments.StartActivity($"payment {kind}");
        activity?.SetTag("payment.kind", kind);
        if (merchantId is not null)
            activity?.SetTag("clearpay.merchant_id", merchantId);
        if (paymentId is { } id)
            activity?.SetTag("clearpay.payment_id", id.ToString());

        var started = Stopwatch.GetTimestamp();
        try
        {
            var outcome = await call().ConfigureAwait(false);
            var result = outcome.Kind.ToString();
            activity?.SetTag("payment.outcome", result);
            if (outcome.Payment is { } payment)
            {
                activity?.SetTag("clearpay.payment_id", payment.PaymentId.ToString());
                activity?.SetTag("clearpay.merchant_id", payment.MerchantId);
                activity?.SetTag("payment.provider", payment.Provider);
                activity?.SetTag("payment.status", payment.Status);
                if (payment.FailureCode is not null)
                    activity?.SetTag("payment.failure_code", payment.FailureCode);
            }

            // Business "no" (declined card, refund over the limit, duplicate order) is a normal answer.
            // A forged 3DS signature or a lost concurrency race is what an on-call engineer wants to find.
            if (outcome.Kind is PaymentResultKind.InvalidSignature or PaymentResultKind.ConcurrencyConflict)
                activity?.SetStatus(ActivityStatusCode.Error, result);

            ClearPayTelemetry.RecordPayment(kind, result, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return outcome;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
            ClearPayTelemetry.RecordPayment(kind, "Exception", Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// Decorator for one bank / PSP adapter: a child span per acquirer call (<c>acquirer authorize</c>, ...)
/// tagged with the adapter name, the normalized code and the raw provider code. That is the span to look
/// at when "bank B declines everything" shows up in production.
/// </summary>
public sealed class TracedAcquirerAdapter : IAcquirerAdapter
{
    private readonly IAcquirerAdapter _inner;

    public TracedAcquirerAdapter(IAcquirerAdapter inner) => _inner = inner;

    public string Name => _inner.Name;

    public Task<AcquirerResult> AuthorizeAsync(AcquirerRequest request, CancellationToken cancellationToken = default) =>
        TraceAsync("authorize", request, () => _inner.AuthorizeAsync(request, cancellationToken));

    public Task<AcquirerResult> CaptureAsync(AcquirerRequest request, CancellationToken cancellationToken = default) =>
        TraceAsync("capture", request, () => _inner.CaptureAsync(request, cancellationToken));

    public Task<AcquirerResult> VoidAsync(AcquirerRequest request, CancellationToken cancellationToken = default) =>
        TraceAsync("void", request, () => _inner.VoidAsync(request, cancellationToken));

    public Task<AcquirerResult> RefundAsync(
        AcquirerRequest request,
        decimal refundAmount,
        CancellationToken cancellationToken = default) =>
        TraceAsync("refund", request, () => _inner.RefundAsync(request, refundAmount, cancellationToken));

    private async Task<AcquirerResult> TraceAsync(string operation, AcquirerRequest request, Func<Task<AcquirerResult>> call)
    {
        using var activity = ClearPayTelemetry.Payments.StartActivity($"acquirer {operation}", ActivityKind.Client);
        activity?.SetTag("acquirer.name", _inner.Name);
        activity?.SetTag("acquirer.operation", operation);
        activity?.SetTag("clearpay.payment_id", request.PaymentId.ToString());
        try
        {
            var result = await call().ConfigureAwait(false);
            activity?.SetTag("acquirer.response_code", result.Code.ToString());
            activity?.SetTag("acquirer.raw_code", result.RawCode);
            if (result.Code is AcquirerResponseCode.IssuerUnavailable or AcquirerResponseCode.Unknown)
                activity?.SetStatus(ActivityStatusCode.Error, result.Code.ToString());
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
            throw;
        }
    }
}
