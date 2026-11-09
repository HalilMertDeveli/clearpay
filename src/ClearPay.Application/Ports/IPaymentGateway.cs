using ClearPay.Application.Payments;

namespace ClearPay.Application.Ports;

/// <summary>
/// Card acquiring use cases (mock). Merchant API and the 3DS callback call this port;
/// the state machine lives in Domain <c>Payment</c>, the bank calls behind <see cref="IAcquirerAdapter"/>.
/// Every state change writes an outbox row in the same SaveChanges.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Created → Pending3DS. Returns the payment the ACS page will authenticate.</summary>
    Task<PaymentOutcome> CreateAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default);

    /// <summary>Verifies the HMAC, then Pending3DS → Authorized | Failed. A repeated callback is a no-op.</summary>
    Task<PaymentOutcome> CompleteThreeDsAsync(ThreeDsCallback callback, CancellationToken cancellationToken = default);

    Task<PaymentOutcome> CaptureAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default);

    Task<PaymentOutcome> VoidAsync(string merchantId, Guid paymentId, CancellationToken cancellationToken = default);

    Task<PaymentOutcome> RefundAsync(RefundPaymentCommand command, CancellationToken cancellationToken = default);

    /// <summary>Unscoped read (mock ACS page, callback redirect). Merchant endpoints check ownership.</summary>
    Task<PaymentView?> GetAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
