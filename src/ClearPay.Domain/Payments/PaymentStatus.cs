namespace ClearPay.Domain.Payments;

/// <summary>
/// Card payment lifecycle on the mock acquiring gateway. Stored as int — do not renumber.
/// Created → Pending3DS → Authorized | Failed; Authorized → Captured | Voided;
/// Captured → PartiallyRefunded → Refunded.
/// </summary>
public enum PaymentStatus
{
    Created = 0,
    Pending3DS = 1,
    Authorized = 2,
    Failed = 3,
    Captured = 4,
    PartiallyRefunded = 5,
    Refunded = 6,
    Voided = 7
}

public enum PaymentRefundStatus
{
    /// <summary>Amount is reserved on the payment before the acquirer is called.</summary>
    Pending = 0,
    Succeeded = 1,
    Failed = 2
}
