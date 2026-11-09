using System.Text.Json;
using ClearPay.Contracts;
using ClearPay.Messaging;

namespace ClearPay.Reconciliation;

/// <summary>
/// Consumer for <c>reconciliation.payments</c>: turns capture and refund events into
/// <see cref="PaymentTransaction"/> rows (our side of the settlement). Insert + ProcessedMessage in one transaction.
/// </summary>
public sealed class PaymentProjectionHandler : IMessageHandler
{
    public const string QueueName = "reconciliation.payments";

    public static readonly IReadOnlyList<string> BindingKeys = [EventTypes.PaymentCaptured, EventTypes.PaymentRefunded];

    private readonly ReconciliationDbContext _db;

    public PaymentProjectionHandler(ReconciliationDbContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(IncomingMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        var row = ToTransaction(message);
        if (row is null)
            return;

        await IdempotentProcessing.RunOnceAsync(_db, QueueName, message, _ =>
        {
            _db.PaymentTransactions.Add(row);
            return Task.CompletedTask;
        }, cancellationToken).ConfigureAwait(false);
    }

    public static PaymentTransaction? ToTransaction(IncomingMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Type is not (EventTypes.PaymentCaptured or EventTypes.PaymentRefunded))
            return null;

        PaymentEventV1 e;
        try
        {
            e = ContractJson.Deserialize<PaymentEventV1>(message.Body);
        }
        catch (JsonException ex)
        {
            throw new PoisonMessageException($"Body of {message.Type} is not a PaymentEventV1.", ex);
        }

        if (e.PaymentId == Guid.Empty || string.IsNullOrWhiteSpace(e.Provider) || e.OccurredAt == default)
            throw new PoisonMessageException("Payment event without paymentId, provider or occurredAt.");

        var isRefund = message.Type == EventTypes.PaymentRefunded;
        if (isRefund && (e.Extra?.RefundId is null || e.Extra.RefundAmount is null))
            throw new PoisonMessageException("Refund event without extra.refundId / extra.refundAmount.");

        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            Kind = isRefund ? PaymentTransaction.Refund : PaymentTransaction.Sale,
            PaymentId = e.PaymentId,
            ReferenceId = isRefund ? e.Extra!.RefundId!.Value : e.PaymentId,
            Provider = e.Provider,
            MerchantId = e.MerchantId,
            Amount = isRefund ? e.Extra!.RefundAmount!.Value : e.CapturedAmount,
            Currency = e.Currency,
            OccurredAt = e.OccurredAt,
            BusinessDate = BusinessDay.Of(e.OccurredAt),
            SourceMessageId = message.MessageId
        };
    }
}
