using System.Globalization;

namespace ClearPay.Domain.Payments;

/// <summary>
/// One card payment a merchant started on the (mock) acquiring gateway.
/// No PAN is stored: only BIN (first six) and last four. Amounts are decimal with two places (kuruş).
/// <see cref="Version"/> is an optimistic concurrency token: two parallel refunds or callbacks on the
/// same row → one SaveChanges wins, the other gets a concurrency conflict instead of a double refund.
/// </summary>
public sealed class Payment
{
    public Guid Id { get; set; }

    public string MerchantId { get; set; } = string.Empty;

    /// <summary>Merchant order number. Unique per merchant, so a retried create returns the same payment.</summary>
    public string MerchantOrderId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "TRY";

    public string CardBin { get; set; } = string.Empty;

    public string CardLast4 { get; set; } = string.Empty;

    /// <summary>Adapter chosen by the router (MockBankA, MockBankB, MockPspC).</summary>
    public string Provider { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; }

    /// <summary>Set when 3DS starts; the ACS echoes it back in the signed callback.</summary>
    public string? ThreeDsTransactionId { get; set; }

    /// <summary>Acquirer authorization code / reference after a successful provision.</summary>
    public string? ProviderReference { get; set; }

    public decimal CapturedAmount { get; set; }

    /// <summary>Pending + succeeded refunds. Never exceeds <see cref="CapturedAmount"/>.</summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>Normalized code (Application <c>AcquirerResponseCode</c>) when the payment failed.</summary>
    public string? FailureCode { get; set; }

    public string? FailureMessage { get; set; }

    public string ReturnUrl { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Concurrency token. Bumped by every successful SaveChanges of this row.</summary>
    public int Version { get; set; }

    public List<PaymentRefund> Refunds { get; set; } = [];

    public decimal RefundableAmount => CapturedAmount - RefundedAmount;

    public static Payment Create(
        string merchantId,
        string merchantOrderId,
        decimal amount,
        string currency,
        string cardBin,
        string cardLast4,
        string provider,
        string returnUrl,
        DateTimeOffset now)
    {
        EnsureMoney(amount);
        return new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            MerchantOrderId = merchantOrderId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            CardBin = cardBin,
            CardLast4 = cardLast4,
            Provider = provider,
            ReturnUrl = returnUrl,
            Status = PaymentStatus.Created,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>Money rule shared by create and refund: positive, at most two decimals.</summary>
    public static bool IsValidMoney(decimal amount) =>
        amount > 0m && decimal.Round(amount, 2) == amount;

    public void StartThreeDs(string threeDsTransactionId, DateTimeOffset now)
    {
        Move(PaymentStatus.Pending3DS, now);
        ThreeDsTransactionId = threeDsTransactionId;
    }

    public void Authorize(string providerReference, DateTimeOffset now)
    {
        Move(PaymentStatus.Authorized, now);
        ProviderReference = providerReference;
    }

    public void Fail(string code, string? message, DateTimeOffset now)
    {
        Move(PaymentStatus.Failed, now);
        FailureCode = code;
        FailureMessage = message;
    }

    /// <summary>Full capture of the authorized amount (partial capture is out of scope).</summary>
    public void Capture(DateTimeOffset now)
    {
        Move(PaymentStatus.Captured, now);
        CapturedAmount = Amount;
    }

    /// <summary>Void = cancel an authorization before capture. After capture you refund instead.</summary>
    public void Void(DateTimeOffset now) => Move(PaymentStatus.Voided, now);

    /// <summary>
    /// Refund step 1: reserve the amount on this row (the save bumps <see cref="Version"/>) before the
    /// acquirer is called, so two concurrent refunds cannot both pass the limit check.
    /// </summary>
    public PaymentRefund ReserveRefund(decimal amount, string? reason, DateTimeOffset now)
    {
        EnsureMoney(amount);
        if (Status is not (PaymentStatus.Captured or PaymentStatus.PartiallyRefunded))
            throw new PaymentStateException($"Payment in {Status} cannot be refunded; only captured payments can.");
        if (amount > RefundableAmount)
            throw new RefundLimitExceededException(amount, RefundableAmount);

        RefundedAmount += amount;
        UpdatedAt = now;
        var refund = new PaymentRefund
        {
            Id = Guid.NewGuid(),
            PaymentId = Id,
            Amount = amount,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            Status = PaymentRefundStatus.Pending,
            CreatedAt = now
        };
        Refunds.Add(refund);
        return refund;
    }

    /// <summary>Refund step 2 (acquirer approved): the reservation becomes final; status follows the total.</summary>
    public void CompleteRefund(PaymentRefund refund, string providerReference, DateTimeOffset now)
    {
        EnsurePending(refund);
        refund.Status = PaymentRefundStatus.Succeeded;
        refund.ProviderReference = providerReference;
        refund.CompletedAt = now;
        var fullyRefunded = Refunds
            .Where(r => r.Status == PaymentRefundStatus.Succeeded)
            .Sum(r => r.Amount) == CapturedAmount;
        Move(fullyRefunded ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded, now);
    }

    /// <summary>Refund step 2 (acquirer declined): release the reserved amount; status does not change.</summary>
    public void ReleaseRefund(PaymentRefund refund, string failureCode, DateTimeOffset now)
    {
        EnsurePending(refund);
        refund.Status = PaymentRefundStatus.Failed;
        refund.FailureCode = failureCode;
        refund.CompletedAt = now;
        RefundedAmount -= refund.Amount;
        UpdatedAt = now;
    }

    private void Move(PaymentStatus to, DateTimeOffset now)
    {
        PaymentStateMachine.Ensure(Status, to);
        Status = to;
        UpdatedAt = now;
    }

    private void EnsurePending(PaymentRefund refund)
    {
        if (refund.PaymentId != Id || refund.Status != PaymentRefundStatus.Pending)
            throw new PaymentStateException("Refund is not a pending reservation of this payment.");
    }

    private static void EnsureMoney(decimal amount)
    {
        if (!IsValidMoney(amount))
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be > 0 with at most two decimals.");
    }
}

public sealed class RefundLimitExceededException : InvalidOperationException
{
    public RefundLimitExceededException(decimal requested, decimal refundable)
        : base(string.Create(CultureInfo.InvariantCulture, $"Refund {requested:F2} exceeds refundable {refundable:F2}."))
    {
        Requested = requested;
        Refundable = refundable;
    }

    public decimal Requested { get; }

    public decimal Refundable { get; }
}
