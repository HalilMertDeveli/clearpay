using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// Outbox relay (Hangfire, every minute). Publishes Pending rows oldest first and marks them Sent only
/// after the broker confirmed. A failed publish leaves the row Pending with <see cref="OutboxMessage.Attempts"/>
/// + 1, so a broker outage delays events instead of losing them; after <see cref="MaxAttempts"/> the row
/// becomes Failed and shows up in the admin queue for a human.
/// </summary>
public sealed class SqlOutboxProcessor : IOutboxProcessor
{
    public const int MaxAttempts = 10;
    private const int BatchSize = 20;

    private readonly ClearPayDbContext _db;
    private readonly IOutboxPublisher _publisher;
    private readonly IClock _clock;
    private readonly ILogger<SqlOutboxProcessor> _logger;

    public SqlOutboxProcessor(
        ClearPayDbContext db,
        IOutboxPublisher publisher,
        IClock clock,
        ILogger<SqlOutboxProcessor>? logger = null)
    {
        _db = db;
        _publisher = publisher;
        _clock = clock;
        _logger = logger ?? NullLogger<SqlOutboxProcessor>.Instance;
    }

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _db.OutboxMessages
            .Where(m => m.Status == OutboxStatus.Pending)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        // Ordered in memory: SQLite (tests) cannot ORDER BY DateTimeOffset. Pending is small by design.
        pending = pending.OrderBy(m => m.OccurredAt).Take(BatchSize).ToList();

        var sent = 0;
        foreach (var message in pending)
        {
            try
            {
                await _publisher
                    .PublishAsync(
                        new OutboxEnvelope(
                            message.Id,
                            message.Type,
                            message.Payload,
                            message.CorrelationId,
                            message.OccurredAt,
                            message.TraceParent),
                        cancellationToken)
                    .ConfigureAwait(false);
                message.Status = OutboxStatus.Sent;
                message.ProcessedAt = _clock.UtcNow;
                message.LastError = null;
                sent++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.Attempts++;
                message.LastError = Truncate($"{ex.GetType().Name}: {ex.Message}", 1000);
                if (message.Attempts >= MaxAttempts)
                {
                    message.Status = OutboxStatus.Failed;
                    message.ProcessedAt = _clock.UtcNow;
                    _logger.LogError(
                        "Outbox {MessageId} ({Type}) failed {Attempts} times; marked Failed: {Error}",
                        message.Id, message.Type, message.Attempts, message.LastError);
                }
                else
                {
                    _logger.LogWarning(
                        "Outbox {MessageId} ({Type}) publish attempt {Attempts} failed; stays Pending: {Error}",
                        message.Id, message.Type, message.Attempts, message.LastError);
                }
            }
        }

        if (pending.Count > 0)
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return sent;
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
