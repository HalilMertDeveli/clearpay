namespace ClearPay.Application.Payments;

/// <summary>Merchant asks the gateway to start a card payment. <see cref="CardNumber"/> is used for BIN/last4 only, never stored.</summary>
public sealed record CreatePaymentCommand(
    string MerchantId,
    string MerchantOrderId,
    decimal Amount,
    string Currency,
    string CardNumber,
    string ReturnUrl);

/// <summary>What the mock ACS posts back (browser form post), signed with HMAC.</summary>
public sealed record ThreeDsCallback(
    Guid PaymentId,
    string ThreeDsTransactionId,
    string Result,
    string Signature);

public sealed record RefundPaymentCommand(
    string MerchantId,
    Guid PaymentId,
    decimal Amount,
    string? Reason);

/// <summary>Read model for API / pages. Status is a string so Web does not need Domain types.</summary>
public sealed record PaymentView(
    Guid PaymentId,
    string MerchantId,
    string MerchantOrderId,
    decimal Amount,
    string Currency,
    string Status,
    string Provider,
    string CardBin,
    string CardLast4,
    string? ThreeDsTransactionId,
    string? ProviderReference,
    decimal CapturedAmount,
    decimal RefundedAmount,
    string? FailureCode,
    string? FailureMessage,
    string ReturnUrl,
    DateTimeOffset CreatedAt,
    IReadOnlyList<RefundView> Refunds);

public sealed record RefundView(
    Guid RefundId,
    decimal Amount,
    string Status,
    string? ProviderReference,
    string? FailureCode,
    DateTimeOffset CreatedAt);

public enum PaymentResultKind
{
    Ok = 0,
    Created = 1,

    /// <summary>Same merchant order id again: no second payment, caller maps to 409.</summary>
    DuplicateOrder = 2,

    /// <summary>3DS callback for a payment that already left Pending3DS: nothing is done twice.</summary>
    AlreadyProcessed = 3,
    NotFound = 4,
    InvalidRequest = 5,
    InvalidSignature = 6,
    InvalidState = 7,
    RefundLimitExceeded = 8,

    /// <summary>Acquirer said no to capture / void / refund. Payment state is unchanged.</summary>
    AcquirerDeclined = 9,

    /// <summary>Another request changed the same payment first (optimistic concurrency).</summary>
    ConcurrencyConflict = 10
}

public sealed record PaymentOutcome(PaymentResultKind Kind, PaymentView? Payment, string? Error)
{
    public bool IsSuccess => Kind is PaymentResultKind.Ok or PaymentResultKind.Created;

    public static PaymentOutcome Success(PaymentView payment, PaymentResultKind kind = PaymentResultKind.Ok) =>
        new(kind, payment, null);

    public static PaymentOutcome Fail(PaymentResultKind kind, string error, PaymentView? payment = null) =>
        new(kind, payment, error);
}
