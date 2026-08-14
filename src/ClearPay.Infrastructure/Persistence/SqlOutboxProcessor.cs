using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

public sealed class SqlOutboxProcessor : IOutboxProcessor
{
    private readonly ClearPayDbContext _db;
    private readonly IOutboxPublisher _publisher;
    private readonly IClock _clock;

    public SqlOutboxProcessor(ClearPayDbContext db, IOutboxPublisher publisher, IClock clock)
    {
        _db = db;
        _publisher = publisher;
        _clock = clock;
    }

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _db.OutboxMessages
            .Where(m => m.Status == OutboxStatus.Pending)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        pending = pending.OrderBy(m => m.OccurredAt).Take(20).ToList();

        var sent = 0;
        foreach (var message in pending)
        {
            try
            {
                await _publisher
                    .PublishAsync(message.Type, message.Payload, message.CorrelationId, cancellationToken)
                    .ConfigureAwait(false);
                message.Status = OutboxStatus.Sent;
                message.ProcessedAt = _clock.UtcNow;
                sent++;
            }
            catch (Exception)
            {
                message.Status = OutboxStatus.Failed;
                message.ProcessedAt = _clock.UtcNow;
            }
        }

        if (pending.Count > 0)
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return sent;
    }
}
