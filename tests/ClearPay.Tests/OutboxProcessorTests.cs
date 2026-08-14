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
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = id,
            Type = "clearpay.transfer.completed",
            Payload = "{\"ok\":true}",
            CorrelationId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Status = OutboxStatus.Pending
        });
        await _db.SaveChangesAsync();

        var sent = await _processor.ProcessPendingAsync();
        sent.Should().Be(1);
        _publisher.Count.Should().Be(1);
        (await _db.OutboxMessages.SingleAsync()).Status.Should().Be(OutboxStatus.Sent);
        (await _db.OutboxMessages.SingleAsync()).ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Publisher_failure_marks_failed_and_row_remains()
    {
        _publisher.Fail = true;
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "clearpay.topup.timeout",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Status = OutboxStatus.Pending
        });
        await _db.SaveChangesAsync();

        await _processor.ProcessPendingAsync();
        var row = await _db.OutboxMessages.SingleAsync();
        row.Status.Should().Be(OutboxStatus.Failed);
        row.Id.Should().NotBe(Guid.Empty);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class RecordingPublisher : IOutboxPublisher
    {
        public int Count { get; private set; }

        public bool Fail { get; set; }

        public Task PublishAsync(string type, string payload, Guid correlationId, CancellationToken cancellationToken = default)
        {
            if (Fail)
                throw new InvalidOperationException("broker down");
            Count++;
            return Task.CompletedTask;
        }
    }
}
