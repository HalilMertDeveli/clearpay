namespace ClearPay.Contracts;

/// <summary>
/// Body of every <c>clearpay.payment.*</c> event (JSON, camelCase, see <see cref="ContractJson"/>).
/// A snapshot of the payment after the change, so a consumer never has to call back into the web app.
/// No PAN, BIN or last four: consumers do not need card data.
/// </summary>
public sealed record PaymentEventV1(
    Guid PaymentId,
    string MerchantId,
    string MerchantOrderId,
    string Status,
    decimal Amount,
    string Currency,
    string Provider,
    decimal CapturedAmount,
    decimal RefundedAmount,
    string? FailureCode,
    string? ProviderReference,
    DateTimeOffset OccurredAt,
    PaymentEventExtra? Extra = null);

/// <summary>Event-specific details. Refund events fill the refund fields; a bank decline fills <see cref="RawCode"/>.</summary>
public sealed record PaymentEventExtra(
    Guid? RefundId = null,
    decimal? RefundAmount = null,
    string? Code = null,
    string? RawCode = null);
