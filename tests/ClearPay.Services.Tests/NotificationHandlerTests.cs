using ClearPay.Contracts;
using ClearPay.Messaging;
using ClearPay.Notifications;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClearPay.Services.Tests;

public sealed class NotificationHandlerTests : IDisposable
{
    private readonly SqliteDb _sqlite = new();
    private readonly CountingSender _sender = new();
    private readonly NotificationsOptions _options = new() { FailMerchantOrderPrefix = "fail-" };

    [Fact]
    public async Task Captured_payment_sends_one_merchant_webhook_and_records_it()
    {
        await using var db = _sqlite.Notifications();
        var message = Events.Payment(EventTypes.PaymentCaptured, 120.00m, "ORD-42");

        await Handler(db).HandleAsync(message, CancellationToken.None);

        var log = await db.NotificationLogs.AsNoTracking().SingleAsync();
        log.Channel.Should().Be("Webhook");
        log.Recipient.Should().Be("https://merchant.example/demo-shop/webhooks/clearpay");
        log.Subject.Should().Be("payment.captured order ORD-42 120.00 TRY status Captured");
        log.MessageId.Should().Be(message.MessageId);
        (await db.ProcessedMessages.CountAsync()).Should().Be(1);
        _sender.Count.Should().Be(1);
    }

    [Fact]
    public async Task Redelivered_message_does_not_notify_twice()
    {
        await using var db = _sqlite.Notifications();
        var message = Events.Payment(EventTypes.PaymentRefunded, 50m,
            extra: new PaymentEventExtra(RefundId: Guid.NewGuid(), RefundAmount: 20.10m, Code: "Approved"));

        await Handler(db).HandleAsync(message, CancellationToken.None);
        await Handler(db).HandleAsync(message with { Attempt = 2 }, CancellationToken.None);

        (await db.NotificationLogs.CountAsync()).Should().Be(2, "refund = merchant webhook + finance e-mail, once");
        (await db.NotificationLogs.AsNoTracking().ToListAsync()).Should()
            .Contain(n => n.Channel == "Email" && n.Subject == "Refund of 20.10 TRY for order ORD-1 approved");
        _sender.Count.Should().Be(2);
        (await db.ProcessedMessages.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Injected_fault_throws_and_leaves_nothing_behind_so_the_retry_can_succeed()
    {
        await using var db = _sqlite.Notifications();
        var message = Events.Payment(EventTypes.PaymentAuthorized, orderId: "fail-001");

        var act = () => Handler(db).HandleAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Injected fault*");
        (await db.NotificationLogs.CountAsync()).Should().Be(0);
        (await db.ProcessedMessages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Unreadable_body_is_poison()
    {
        await using var db = _sqlite.Notifications();
        var message = new IncomingMessage(Guid.NewGuid(), EventTypes.PaymentCaptured, "{not json", null, 1);

        var act = () => Handler(db).HandleAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<PoisonMessageException>();
    }

    [Fact]
    public void Mismatch_event_alerts_ops_by_email_and_sms()
    {
        var body = ContractJson.Serialize(new ReconciliationMismatchFoundV1(
            Guid.NewGuid(), new DateOnly(2026, 11, 9), 10, 1, 1, 1, 0, DateTimeOffset.UtcNow));
        var (planned, _) = EventNotificationHandler.Plan(
            new IncomingMessage(Guid.NewGuid(), EventTypes.ReconciliationMismatchFound, body, null, 1), "ops@clearpay.local");

        planned.Select(p => p.Channel).Should().Equal("Email", "Sms");
        planned[0].Recipient.Should().Be("ops@clearpay.local");
        planned[0].Subject.Should().Be(
            "Reconciliation 2026-11-09: 1 only at bank, 1 only in ClearPay, 1 amount differences, 0 ledger imbalances");
    }

    [Fact]
    public void Unknown_event_type_is_acked_without_notifications()
    {
        var (planned, _) = EventNotificationHandler.Plan(
            new IncomingMessage(Guid.NewGuid(), "clearpay.something.new", "{}", null, 1), "ops@x");
        planned.Should().BeEmpty();
    }

    private EventNotificationHandler Handler(NotificationsDbContext db) =>
        new(db, _sender, _options, NullLogger<EventNotificationHandler>.Instance);

    public void Dispose() => _sqlite.Dispose();

    private sealed class CountingSender : INotificationSender
    {
        public int Count { get; private set; }

        public Task SendAsync(PlannedNotification notification, Guid idempotencyKey, CancellationToken cancellationToken)
        {
            Count++;
            return Task.CompletedTask;
        }
    }
}
