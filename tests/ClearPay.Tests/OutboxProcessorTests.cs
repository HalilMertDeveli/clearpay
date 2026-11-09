using System.Diagnostics;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using ClearPay.Infrastructure.Time;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

public sealed class OutboxProcessorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ClearPayDbContext _db;
    private readonly RecordingPublisher _publisher;
    private readonly SqlOutboxProcessor _processor;

    public OutboxProcessorTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _publisher = new RecordingPublisher();
        _processor = new SqlOutboxProcessor(_db, _publisher, new SystemClock());
    }

    [Fact]
    public async Task Pending_is_marked_sent_after_publish_so_timeout_does_not_lose_the_row()
    {
        var id = Guid.NewGuid();
        _db.OutboxMessages.Add(NewMessage(id, "clearpay.transfer.completed"));
        await _db.SaveChangesAsync();

        var sent = await _processor.ProcessPendingAsync();
        sent.Should().Be(1);
        _publisher.Sent.Should().ContainSingle().Which.MessageId.Should().Be(id, "MessageId = outbox row id, stable across re-sends");
        var row = await _db.OutboxMessages.SingleAsync();
        row.Status.Should().Be(OutboxStatus.Sent);
        row.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Broker_failure_keeps_the_row_pending_and_counts_the_attempt()
    {
        _publisher.Fail = true;
        _db.OutboxMessages.Add(NewMessage(Guid.NewGuid(), "clearpay.topup.timeout"));
        await _db.SaveChangesAsync();

        (await _processor.ProcessPendingAsync()).Should().Be(0);

        var row = await _db.OutboxMessages.SingleAsync();
        row.Status.Should().Be(OutboxStatus.Pending, "a broker outage must delay the event, not drop it");
        row.Attempts.Should().Be(1);
        row.LastError.Should().Contain("broker down");
        row.ProcessedAt.Should().BeNull();

        _publisher.Fail = false;
        (await _processor.ProcessPendingAsync()).Should().Be(1);
        row = await _db.OutboxMessages.AsNoTracking().SingleAsync();
        row.Status.Should().Be(OutboxStatus.Sent);
        row.LastError.Should().BeNull();
    }

    [Fact]
    public async Task Row_becomes_failed_after_max_attempts()
    {
        _publisher.Fail = true;
        _db.OutboxMessages.Add(NewMessage(Guid.NewGuid(), "clearpay.payment.captured"));
        await _db.SaveChangesAsync();

        for (var i = 0; i < SqlOutboxProcessor.MaxAttempts; i++)
            await _processor.ProcessPendingAsync();

        var row = await _db.OutboxMessages.SingleAsync();
        row.Status.Should().Be(OutboxStatus.Failed);
        row.Attempts.Should().Be(SqlOutboxProcessor.MaxAttempts);
        row.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Insert_inside_a_request_span_stores_its_traceparent_and_the_publisher_gets_it()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "outbox-test",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);
        using var source = new ActivitySource("outbox-test");

        string expected;
        using (var request = source.StartActivity("POST /api/payments"))
        {
            request.Should().NotBeNull();
            expected = request!.Id!;
            _db.OutboxMessages.Add(NewMessage(Guid.NewGuid(), "clearpay.payment.authorized"));
            await _db.SaveChangesAsync();
        }

        (await _db.OutboxMessages.AsNoTracking().SingleAsync()).TraceParent.Should().Be(expected);
        await _processor.ProcessPendingAsync();
        _publisher.Sent.Single().TraceParent.Should().Be(expected);
    }

    private static OutboxMessage NewMessage(Guid id, string type) => new()
    {
        Id = id,
        Type = type,
        Payload = "{\"ok\":true}",
        CorrelationId = Guid.NewGuid(),
        OccurredAt = DateTimeOffset.UtcNow,
        Status = OutboxStatus.Pending
    };

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class RecordingPublisher : IOutboxPublisher
    {
        public List<OutboxEnvelope> Sent { get; } = [];

        public bool Fail { get; set; }

        public Task PublishAsync(OutboxEnvelope message, CancellationToken cancellationToken = default)
        {
            if (Fail)
                throw new InvalidOperationException("broker down");
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }
}
