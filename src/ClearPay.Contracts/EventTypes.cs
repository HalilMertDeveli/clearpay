namespace ClearPay.Contracts;

/// <summary>
/// Event type = RabbitMQ routing key = <c>OutboxMessage.Type</c>. Renaming one is a breaking change
/// for every consumer binding, so new meaning gets a new name instead.
/// </summary>
public static class EventTypes
{
    public const string PaymentPrefix = "clearpay.payment.";

    public const string PaymentPending3Ds = PaymentPrefix + "pending_3ds";
    public const string PaymentAuthorized = PaymentPrefix + "authorized";
    public const string PaymentFailed = PaymentPrefix + "failed";
    public const string PaymentCaptured = PaymentPrefix + "captured";
    public const string PaymentVoided = PaymentPrefix + "voided";
    public const string PaymentRefunded = PaymentPrefix + "refunded";
    public const string PaymentRefundFailed = PaymentPrefix + "refund_failed";

    public const string TransferCompleted = "clearpay.transfer.completed";

    /// <summary>Published by the reconciliation service when a nightly run finds differences.</summary>
    public const string ReconciliationMismatchFound = "clearpay.reconciliation.mismatch_found";
}
