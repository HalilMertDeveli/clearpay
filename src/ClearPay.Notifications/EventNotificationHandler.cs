using System.Globalization;
using System.Text.Json;
using ClearPay.Contracts;
using ClearPay.Messaging;
using Microsoft.Extensions.Logging;

namespace ClearPay.Notifications;

public sealed class NotificationsOptions
{
    public const string SectionName = "Notifications";

    public int MaxAttempts { get; set; } = 3;

    public int RetryDelaySeconds { get; set; } = 5;

    public string OpsEmail { get; set; } = "ops@clearpay.local";

    /// <summary>
    /// Demo fault injection: a payment event whose merchant order id starts with this prefix makes the
    /// handler throw, so retry → DLQ can be shown live. Empty = off. Never set in production.
    /// </summary>
    public string FailMerchantOrderPrefix { get; set; } = string.Empty;
}

/// <summary>One notification to send. Recipients are fake addresses; nothing leaves the process.</summary>
public sealed record PlannedNotification(string Channel, string Recipient, string Subject);

/// <summary>
/// Consumer for <c>notifications.events</c>. Decides who hears about an event (pure <see cref="Plan"/>),
/// then writes the <see cref="NotificationLog"/> rows and the <see cref="ProcessedMessage"/> row in one
/// transaction, so a redelivered event does not notify twice.
/// </summary>
public sealed class EventNotificationHandler : IMessageHandler
{
    public const string QueueName = "notifications.events";

    public static readonly IReadOnlyList<string> BindingKeys =
    [
        EventTypes.PaymentAuthorized,
        EventTypes.PaymentFailed,
        EventTypes.PaymentCaptured,
        EventTypes.PaymentVoided,
        EventTypes.PaymentRefunded,
        EventTypes.PaymentRefundFailed,
        EventTypes.ReconciliationMismatchFound
    ];

    private readonly NotificationsDbContext _db;
    private readonly INotificationSender _sender;
    private readonly NotificationsOptions _options;
    private readonly ILogger<EventNotificationHandler> _logger;

    public EventNotificationHandler(
        NotificationsDbContext db,
        INotificationSender sender,
        NotificationsOptions options,
        ILogger<EventNotificationHandler> logger)
    {
        _db = db;
        _sender = sender;
        _options = options;
        _logger = logger;
    }

    public async Task HandleAsync(IncomingMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        var (planned, merchantOrderId) = Plan(message, _options.OpsEmail);

        if (!string.IsNullOrEmpty(_options.FailMerchantOrderPrefix)
            && merchantOrderId?.StartsWith(_options.FailMerchantOrderPrefix, StringComparison.Ordinal) == true)
        {
            throw new InvalidOperationException(
                $"Injected fault: merchant order '{merchantOrderId}' matches Notifications:FailMerchantOrderPrefix.");
        }

        var ran = await IdempotentProcessing.RunOnceAsync(_db, QueueName, message, async ct =>
        {
            foreach (var n in planned)
            {
                // At-least-once: a crash after this call and before commit sends again on redelivery.
                // A real e-mail/SMS/webhook provider would get message.MessageId as its idempotency key.
                await _sender.SendAsync(n, message.MessageId, ct).ConfigureAwait(false);
                _db.NotificationLogs.Add(new NotificationLog
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.MessageId,
                    EventType = message.Type,
                    Channel = n.Channel,
                    Recipient = n.Recipient,
                    Subject = n.Subject,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }, cancellationToken).ConfigureAwait(false);

        if (!ran)
            _logger.LogInformation("Duplicate {MessageType} {MessageId} ignored (already notified)", message.Type, message.MessageId);
    }

    /// <summary>Event → notifications. Throws <see cref="PoisonMessageException"/> for a body that cannot be read.</summary>
    public static (IReadOnlyList<PlannedNotification> Notifications, string? MerchantOrderId) Plan(IncomingMessage message, string opsEmail)
    {
        ArgumentNullException.ThrowIfNull(message);
        try
        {
            if (message.Type.StartsWith(EventTypes.PaymentPrefix, StringComparison.Ordinal))
            {
                var e = ContractJson.Deserialize<PaymentEventV1>(message.Body);
                if (e.PaymentId == Guid.Empty || string.IsNullOrWhiteSpace(e.MerchantId))
                    throw new PoisonMessageException("Payment event without paymentId or merchantId.");
                return (PlanPayment(message.Type, e), e.MerchantOrderId);
            }

            if (message.Type == EventTypes.ReconciliationMismatchFound)
            {
                var r = ContractJson.Deserialize<ReconciliationMismatchFoundV1>(message.Body);
                var summary = string.Create(
                    CultureInfo.InvariantCulture,
                    $"Reconciliation {r.BusinessDate:yyyy-MM-dd}: {r.OnlyAtBankCount} only at bank, {r.OnlyInClearPayCount} only in ClearPay, {r.AmountMismatchCount} amount differences, {r.LedgerImbalanceCount} ledger imbalances");
                return (
                [
                    new PlannedNotification("Email", opsEmail, summary),
                    new PlannedNotification("Sms", "+90 5** *** ** 00 (on-call)", summary)
                ], null);
            }
        }
        catch (JsonException ex)
        {
            throw new PoisonMessageException($"Body of {message.Type} is not valid JSON for its contract.", ex);
        }

        // Bound by mistake or a newer event type: nothing to send, ack it.
        return ([], null);
    }

    private static List<PlannedNotification> PlanPayment(string type, PaymentEventV1 e)
    {
        var name = type[EventTypes.PaymentPrefix.Length..];
        var webhook = $"https://merchant.example/{e.MerchantId}/webhooks/clearpay";
        var money = e.Amount.ToString("F2", CultureInfo.InvariantCulture) + " " + e.Currency;
        var list = new List<PlannedNotification>
        {
            new("Webhook", webhook, $"payment.{name} order {e.MerchantOrderId} {money} status {e.Status}")
        };

        if (type == EventTypes.PaymentRefunded || type == EventTypes.PaymentRefundFailed)
        {
            var refund = (e.Extra?.RefundAmount ?? 0m).ToString("F2", CultureInfo.InvariantCulture) + " " + e.Currency;
            var verb = type == EventTypes.PaymentRefunded ? "approved" : "declined by the bank";
            list.Add(new PlannedNotification(
                "Email",
                $"finance@{e.MerchantId}.merchant.example",
                $"Refund of {refund} for order {e.MerchantOrderId} {verb}"));
        }

        return list;
    }
}

public interface INotificationSender
{
    Task SendAsync(PlannedNotification notification, Guid idempotencyKey, CancellationToken cancellationToken);
}

/// <summary>Mock e-mail / SMS / webhook gateway: writes one log line. No SMTP, SMS API or HTTP call.</summary>
public sealed class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(PlannedNotification notification, Guid idempotencyKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        _logger.LogInformation(
            "FAKE {Channel} to {Recipient}: {Subject} (idempotency key {IdempotencyKey})",
            notification.Channel, notification.Recipient, notification.Subject, idempotencyKey);
        return Task.CompletedTask;
    }
}
