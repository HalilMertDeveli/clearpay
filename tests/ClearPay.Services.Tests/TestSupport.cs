using ClearPay.Contracts;
using ClearPay.Messaging;
using ClearPay.Notifications;
using ClearPay.Reconciliation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Services.Tests;

/// <summary>Real SQLite in-memory databases (one open connection = one database) for the services' own schemas.</summary>
internal sealed class SqliteDb : IDisposable
{
    public SqliteDb()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();
    }

    public SqliteConnection Connection { get; }

    public NotificationsDbContext Notifications()
    {
        var db = new NotificationsDbContext(new DbContextOptionsBuilder<NotificationsDbContext>().UseSqlite(Connection).Options);
        db.Database.EnsureCreated();
        return db;
    }

    public ReconciliationDbContext Reconciliation()
    {
        var db = new ReconciliationDbContext(new DbContextOptionsBuilder<ReconciliationDbContext>().UseSqlite(Connection).Options);
        db.Database.EnsureCreated();
        return db;
    }

    public void Dispose() => Connection.Dispose();
}

internal static class Events
{
    public static IncomingMessage Payment(
        string type,
        decimal amount = 120.00m,
        string orderId = "ORD-1",
        string provider = "MockBankA",
        DateTimeOffset? occurredAt = null,
        PaymentEventExtra? extra = null,
        Guid? paymentId = null,
        Guid? messageId = null)
    {
        var e = new PaymentEventV1(
            paymentId ?? Guid.NewGuid(),
            "demo-shop",
            orderId,
            "Captured",
            amount,
            "TRY",
            provider,
            type == EventTypes.PaymentCaptured || type == EventTypes.PaymentRefunded ? amount : 0m,
            extra?.RefundAmount ?? 0m,
            null,
            "A-AUTH-123",
            occurredAt ?? new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.Zero),
            extra);
        return new IncomingMessage(messageId ?? Guid.NewGuid(), type, ContractJson.Serialize(e), e.PaymentId, 1);
    }
}

/// <summary>Records what would have gone to RabbitMQ; can be told to fail like a down broker.</summary>
internal sealed class RecordingEventPublisher : IEventPublisher
{
    public List<OutgoingMessage> Published { get; } = [];

    public bool Down { get; set; }

    public Task PublishAsync(OutgoingMessage message, CancellationToken cancellationToken = default)
    {
        if (Down)
            throw new BrokerUnavailableException("test broker down");
        Published.Add(message);
        return Task.CompletedTask;
    }
}
