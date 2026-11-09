using ClearPay.Contracts;
using ClearPay.Application.Payments;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using ClearPay.Domain.Payments;
using ClearPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearPay.Infrastructure.Payments;

/// <summary>
/// Mock acquiring gateway on the same SQL database as the wallet ledger.
/// Rules live in Domain <see cref="Payment"/> (state machine, refund limit); this class loads/saves,
/// calls the routed <see cref="IAcquirerAdapter"/>, and writes an outbox row in the same SaveChanges
/// as every state change. Card payments do not post wallet ledger rows (settlement is out of scope).
/// </summary>
public sealed class SqlPaymentGateway : IPaymentGateway
{
    public const string EventPrefix = "clearpay.payment.";

    private readonly ClearPayDbContext _db;
    private readonly AcquirerRouter _router;
    private readonly ThreeDsSigner _signer;
    private readonly IClock _clock;
    private readonly ILogger<SqlPaymentGateway> _logger;

    public SqlPaymentGateway(
        ClearPayDbContext db,
        AcquirerRouter router,
        ThreeDsSigner signer,
        IClock clock,
        ILogger<SqlPaymentGateway> logger)
    {
        _db = db;
        _router = router;
        _signer = signer;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PaymentOutcome> CreateAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var error = Validate(command, out var digits);
        if (error is not null)
            return PaymentOutcome.Fail(PaymentResultKind.InvalidRequest, error);

        var merchantId = command.MerchantId.Trim();
        var orderId = command.MerchantOrderId.Trim();
        var existing = await FindByOrderAsync(merchantId, orderId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return PaymentOutcome.Fail(PaymentResultKind.DuplicateOrder, "Merchant order id already used.", ToView(existing));

        var bin = digits[..6];
        var adapter = _router.ForBin(bin);
        var now = _clock.UtcNow;
        var payment = Payment.Create(
            merchantId,
            orderId,
            command.Amount,
            command.Currency.Trim(),
            bin,
            digits[^4..],
            adapter.Name,
            command.ReturnUrl.Trim(),
            now);

        // Every card payment on this gateway goes through 3DS (mock policy, no frictionless path).
        payment.StartThreeDs(Guid.NewGuid().ToString("N"), now);
        _db.Payments.Add(payment);
        AddEvent(payment, "pending_3ds", now);

        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (UniqueConstraint.IsDuplicateKey(ex))
        {
            // Two creates for the same order raced; the unique index picked one.
            _db.ChangeTracker.Clear();
            var raced = await FindByOrderAsync(merchantId, orderId, cancellationToken).ConfigureAwait(false);
            return PaymentOutcome.Fail(
                PaymentResultKind.DuplicateOrder,
                "Merchant order id already used.",
                raced is null ? null : ToView(raced));
        }

        _logger.LogInformation(
            "Payment {PaymentId} created for merchant {MerchantId} order {OrderId} via {Provider}; awaiting 3DS",
            payment.Id, merchantId, orderId, adapter.Name);
        return PaymentOutcome.Success(ToView(payment), PaymentResultKind.Created);
    }

    public async Task<PaymentOutcome> CompleteThreeDsAsync(ThreeDsCallback callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        var payment = await LoadAsync(callback.PaymentId, cancellationToken).ConfigureAwait(false);
        if (payment is null)
            return PaymentOutcome.Fail(PaymentResultKind.NotFound, "Payment not found.");

        // Verify against what the gateway stored, not what the browser sent: amount and 3DS id come from SQL.
        var signatureOk = payment.ThreeDsTransactionId is not null
            && string.Equals(payment.ThreeDsTransactionId, callback.ThreeDsTransactionId, StringComparison.Ordinal)
            && _signer.Verify(payment.Id, payment.ThreeDsTransactionId, callback.Result, payment.Amount, callback.Signature);
        if (!signatureOk)
        {
            _logger.LogWarning("3DS callback for payment {PaymentId} rejected: bad signature", payment.Id);
            return PaymentOutcome.Fail(PaymentResultKind.InvalidSignature, "3DS signature is not valid.");
        }

        if (payment.Status != PaymentStatus.Pending3DS)
        {
            // Browser double-submit or ACS retry: same answer, no second provision, no second event.
            _logger.LogInformation(
                "Duplicate 3DS callback for payment {PaymentId} ignored (status {Status})", payment.Id, payment.Status);
            return PaymentOutcome.Fail(PaymentResultKind.AlreadyProcessed, "3DS result was already applied.", ToView(payment));
        }

        var now = _clock.UtcNow;
        if (callback.Result != ThreeDsResult.Authenticated)
        {
            payment.Fail("3DS_FAILED", "Cardholder authentication failed.", now);
            AddEvent(payment, "failed", now);
        }
        else
        {
            var result = await _router.ByName(payment.Provider)
                .AuthorizeAsync(ToRequest(payment), cancellationToken)
                .ConfigureAwait(false);
            if (result.IsApproved)
            {
                payment.Authorize(result.Reference ?? string.Empty, now);
                AddEvent(payment, "authorized", now);
            }
            else
            {
                payment.Fail(result.Code.ToString(), result.Message, now);
                AddEvent(payment, "failed", now, new PaymentEventExtra(RawCode: result.RawCode));
            }

            _logger.LogInformation(
                "Payment {PaymentId} provision via {Provider}: {Code} (raw {RawCode})",
                payment.Id, payment.Provider, result.Code, result.RawCode);
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A parallel duplicate callback won the race; report its result instead of a second one.
            _db.ChangeTracker.Clear();
            var winner = await LoadAsync(payment.Id, cancellationToken).ConfigureAwait(false);
            return PaymentOutcome.Fail(
                PaymentResultKind.AlreadyProcessed,
                "3DS result was already applied.",
                winner is null ? null : ToView(winner));
        }

        return PaymentOutcome.Success(ToView(payment));
    }

    public Task<PaymentOutcome> CaptureAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            merchantId,
            paymentId,
            PaymentStatus.Authorized,
            (adapter, request, ct) => adapter.CaptureAsync(request, ct),
            (payment, _, now) => payment.Capture(now),
            "captured",
            cancellationToken);

    public Task<PaymentOutcome> VoidAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            merchantId,
            paymentId,
            PaymentStatus.Authorized,
            (adapter, request, ct) => adapter.VoidAsync(request, ct),
            (payment, _, now) => payment.Void(now),
            "voided",
            cancellationToken);

    public async Task<PaymentOutcome> RefundAsync(RefundPaymentCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!Payment.IsValidMoney(command.Amount))
            return PaymentOutcome.Fail(PaymentResultKind.InvalidRequest, "Amount must be > 0 with at most two decimals.");

        var payment = await LoadOwnedAsync(command.MerchantId, command.PaymentId, cancellationToken).ConfigureAwait(false);
        if (payment is null)
            return PaymentOutcome.Fail(PaymentResultKind.NotFound, "Payment not found.");

        // Step 1: reserve. The Version check makes this the serialization point for concurrent refunds.
        PaymentRefund refund;
        try
        {
            refund = payment.ReserveRefund(command.Amount, command.Reason, _clock.UtcNow);
        }
        catch (RefundLimitExceededException ex)
        {
            return PaymentOutcome.Fail(PaymentResultKind.RefundLimitExceeded, ex.Message, ToView(payment));
        }
        catch (PaymentStateException ex)
        {
            return PaymentOutcome.Fail(PaymentResultKind.InvalidState, ex.Message, ToView(payment));
        }

        _db.PaymentRefunds.Add(refund);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            _db.ChangeTracker.Clear();
            return PaymentOutcome.Fail(
                PaymentResultKind.ConcurrencyConflict,
                "Payment changed by another request. Read it again and retry.");
        }

        // Step 2: only now talk to the acquirer, then settle the reservation.
        var result = await _router.ByName(payment.Provider)
            .RefundAsync(ToRequest(payment), refund.Amount, cancellationToken)
            .ConfigureAwait(false);
        var saved = await SettleRefundAsync(payment, refund.Id, result, cancellationToken).ConfigureAwait(false);
        var view = ToView(saved);
        return result.IsApproved
            ? PaymentOutcome.Success(view, PaymentResultKind.Created)
            : PaymentOutcome.Fail(PaymentResultKind.AcquirerDeclined, result.Message ?? result.Code.ToString(), view);
    }

    public async Task<PaymentView?> GetAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _db.Payments.AsNoTracking()
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken)
            .ConfigureAwait(false);
        return payment is null ? null : ToView(payment);
    }

    private async Task<PaymentOutcome> TransitionAsync(
        string merchantId,
        Guid paymentId,
        PaymentStatus requiredStatus,
        Func<IAcquirerAdapter, AcquirerRequest, CancellationToken, Task<AcquirerResult>> callAcquirer,
        Action<Payment, AcquirerResult, DateTimeOffset> apply,
        string eventName,
        CancellationToken cancellationToken)
    {
        var payment = await LoadOwnedAsync(merchantId, paymentId, cancellationToken).ConfigureAwait(false);
        if (payment is null)
            return PaymentOutcome.Fail(PaymentResultKind.NotFound, "Payment not found.");
        if (payment.Status != requiredStatus)
        {
            return PaymentOutcome.Fail(
                PaymentResultKind.InvalidState,
                $"Payment is {payment.Status}; this operation needs {requiredStatus}.",
                ToView(payment));
        }

        var result = await callAcquirer(_router.ByName(payment.Provider), ToRequest(payment), cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsApproved)
        {
            return PaymentOutcome.Fail(
                PaymentResultKind.AcquirerDeclined,
                result.Message ?? result.Code.ToString(),
                ToView(payment));
        }

        var now = _clock.UtcNow;
        apply(payment, result, now);
        AddEvent(payment, eventName, now);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            _db.ChangeTracker.Clear();
            return PaymentOutcome.Fail(
                PaymentResultKind.ConcurrencyConflict,
                "Payment changed by another request. Read it again and retry.");
        }

        _logger.LogInformation("Payment {PaymentId} {Event} via {Provider}", payment.Id, eventName, payment.Provider);
        return PaymentOutcome.Success(ToView(payment));
    }

    /// <summary>
    /// Refund step 2: turn the reservation into Succeeded or Failed. If another refund reserved in
    /// between, our tracked Version is stale: reload and re-apply the same acquirer result
    /// (the bank is not called again).
    /// </summary>
    private async Task<Payment> SettleRefundAsync(
        Payment payment,
        Guid refundId,
        AcquirerResult result,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var refund = payment.Refunds.Single(r => r.Id == refundId);
            var now = _clock.UtcNow;
            if (result.IsApproved)
                payment.CompleteRefund(refund, result.Reference ?? string.Empty, now);
            else
                payment.ReleaseRefund(refund, result.Code.ToString(), now);
            AddEvent(
                payment,
                result.IsApproved ? "refunded" : "refund_failed",
                now,
                new PaymentEventExtra(RefundId: refundId, RefundAmount: refund.Amount, Code: result.Code.ToString()));

            try
            {
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return payment;
            }
            catch (DbUpdateConcurrencyException) when (attempt < 3)
            {
                _db.ChangeTracker.Clear();
                payment = await LoadAsync(payment.Id, cancellationToken).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("Payment disappeared during refund.");
            }
        }
    }

    private void AddEvent(Payment payment, string name, DateTimeOffset now, PaymentEventExtra? extra = null)
    {
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventPrefix + name,
            // Shared contract (ClearPay.Contracts): the notification and reconciliation services read this type.
            Payload = ContractJson.Serialize(new PaymentEventV1(
                payment.Id,
                payment.MerchantId,
                payment.MerchantOrderId,
                payment.Status.ToString(),
                payment.Amount,
                payment.Currency,
                payment.Provider,
                payment.CapturedAmount,
                payment.RefundedAmount,
                payment.FailureCode,
                payment.ProviderReference,
                now,
                extra)),
            CorrelationId = payment.Id,
            OccurredAt = now,
            Status = OutboxStatus.Pending
        });
    }

    private Task<Payment?> LoadAsync(Guid paymentId, CancellationToken cancellationToken) =>
        _db.Payments.Include(p => p.Refunds).FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

    private async Task<Payment?> LoadOwnedAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await LoadAsync(paymentId, cancellationToken).ConfigureAwait(false);
        return payment is not null && string.Equals(payment.MerchantId, merchantId, StringComparison.Ordinal)
            ? payment
            : null;
    }

    private Task<Payment?> FindByOrderAsync(string merchantId, string orderId, CancellationToken cancellationToken) =>
        _db.Payments.AsNoTracking()
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.MerchantOrderId == orderId, cancellationToken);

    private static string? Validate(CreatePaymentCommand command, out string digits)
    {
        digits = new string((command.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (string.IsNullOrWhiteSpace(command.MerchantId))
            return "Merchant is required.";
        if (string.IsNullOrWhiteSpace(command.MerchantOrderId) || command.MerchantOrderId.Trim().Length > 64)
            return "merchantOrderId is required (max 64 characters).";
        if (!Payment.IsValidMoney(command.Amount))
            return "Amount must be > 0 with at most two decimals.";
        if (string.IsNullOrWhiteSpace(command.Currency) || command.Currency.Trim().Length != 3)
            return "Currency must be a three-letter code (e.g. TRY).";
        if (digits.Length is < 13 or > 19 || !Luhn.IsValid(digits))
            return "Card number is not valid.";
        if (!IsAllowedReturnUrl(command.ReturnUrl))
            return "returnUrl must be an absolute http(s) URL or a site-relative path.";
        return null;
    }

    private static bool IsAllowedReturnUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > 500)
            return false;
        if (url.StartsWith('/') && !url.StartsWith("//", StringComparison.Ordinal))
            return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }

    private static AcquirerRequest ToRequest(Payment p) =>
        new(p.Id, p.Amount, p.Currency, p.CardBin, p.CardLast4, p.ThreeDsTransactionId, p.ProviderReference);

    internal static PaymentView ToView(Payment p) => new(
        p.Id,
        p.MerchantId,
        p.MerchantOrderId,
        p.Amount,
        p.Currency,
        p.Status.ToString(),
        p.Provider,
        p.CardBin,
        p.CardLast4,
        p.ThreeDsTransactionId,
        p.ProviderReference,
        p.CapturedAmount,
        p.RefundedAmount,
        p.FailureCode,
        p.FailureMessage,
        p.ReturnUrl,
        p.CreatedAt,
        p.Refunds
            .OrderBy(r => r.CreatedAt)
            .Select(r => new RefundView(r.Id, r.Amount, r.Status.ToString(), r.ProviderReference, r.FailureCode, r.CreatedAt))
            .ToList());
}

internal static class Luhn
{
    public static bool IsValid(string digits)
    {
        var sum = 0;
        var doubleIt = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var d = digits[i] - '0';
            if (doubleIt)
            {
                d *= 2;
                if (d > 9)
                    d -= 9;
            }

            sum += d;
            doubleIt = !doubleIt;
        }

        return sum % 10 == 0;
    }
}
