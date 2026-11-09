namespace ClearPay.Domain.Payments;

/// <summary>One refund attempt against a captured <see cref="Payment"/>. Partial refunds are separate rows.</summary>
public sealed class PaymentRefund
{
    public Guid Id { get; set; }

    public Guid PaymentId { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }

    public PaymentRefundStatus Status { get; set; }

    public string? ProviderReference { get; set; }

    public string? FailureCode { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
