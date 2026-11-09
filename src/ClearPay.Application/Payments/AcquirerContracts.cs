namespace ClearPay.Application.Payments;

/// <summary>
/// One contract for every bank / PSP. Each mock adapter speaks its own wire format
/// (ISO-8583 style codes, XML, JSON) and normalizes it to <see cref="AcquirerResult"/> here,
/// so the gateway never branches on a provider name.
/// </summary>
public interface IAcquirerAdapter
{
    /// <summary>Stable name used by routing config and stored on the payment row.</summary>
    string Name { get; }

    Task<AcquirerResult> AuthorizeAsync(AcquirerRequest request, CancellationToken cancellationToken = default);

    Task<AcquirerResult> CaptureAsync(AcquirerRequest request, CancellationToken cancellationToken = default);

    Task<AcquirerResult> VoidAsync(AcquirerRequest request, CancellationToken cancellationToken = default);

    Task<AcquirerResult> RefundAsync(
        AcquirerRequest request,
        decimal refundAmount,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// What the gateway sends to an adapter. No PAN: BIN + last four only.
/// <see cref="PaymentId"/> doubles as the idempotency reference a real acquirer would dedupe on.
/// </summary>
public sealed record AcquirerRequest(
    Guid PaymentId,
    decimal Amount,
    string Currency,
    string CardBin,
    string CardLast4,
    string? ThreeDsTransactionId,
    string? ProviderReference);

/// <summary>Normalized response codes. Raw provider codes are kept in <see cref="AcquirerResult.RawCode"/> for logs.</summary>
public enum AcquirerResponseCode
{
    Approved = 0,
    Declined = 1,
    InsufficientFunds = 2,
    InvalidCard = 3,
    IssuerUnavailable = 4,
    Unknown = 99
}

public sealed record AcquirerResult(
    AcquirerResponseCode Code,
    string RawCode,
    string? Reference,
    string? Message)
{
    public bool IsApproved => Code == AcquirerResponseCode.Approved;

    public static AcquirerResult Approved(string rawCode, string reference) =>
        new(AcquirerResponseCode.Approved, rawCode, reference, null);

    public static AcquirerResult Rejected(AcquirerResponseCode code, string rawCode, string? message) =>
        new(code, rawCode, null, message);
}
