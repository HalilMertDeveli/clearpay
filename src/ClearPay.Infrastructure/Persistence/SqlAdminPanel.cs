using ClearPay.Application.Admin;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

public sealed class SqlAdminPanel : IAdminPanel
{
    private readonly ClearPayDbContext _db;
    private readonly IUserDirectory _users;
    private readonly IWalletSummaryCache _cache;
    private readonly IClock _clock;

    public SqlAdminPanel(
        ClearPayDbContext db,
        IUserDirectory users,
        IWalletSummaryCache cache,
        IClock clock)
    {
        _db = db;
        _users = users;
        _cache = cache;
        _clock = clock;
    }

    public async Task<bool> FreezeByEmailAsync(
        string email,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var userId = await _users.FindUserIdByEmailAsync(email.Trim(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (wallet is null)
        {
            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IsFrozen = true,
                CreatedAt = _clock.UtcNow
            };
            _db.Wallets.Add(wallet);
        }
        else
        {
            wallet.IsFrozen = true;
        }

        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId,
            Action = "wallet.freeze",
            CorrelationId = Guid.NewGuid(),
            Details = email.Trim(),
            CreatedAt = _clock.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await _cache.InvalidateAsync(userId, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<FailedOutboxItem>> ListFailedAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.OutboxMessages.AsNoTracking()
            .Where(m => m.Status == OutboxStatus.Failed)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows
            .OrderByDescending(m => m.OccurredAt)
            .Take(50)
            .Select(m => new FailedOutboxItem(m.Id, m.Type, m.CorrelationId, m.OccurredAt, m.Payload))
            .ToList();
    }

    public async Task<bool> RequeueAsync(Guid outboxId, CancellationToken cancellationToken = default)
    {
        var row = await _db.OutboxMessages.FirstOrDefaultAsync(m => m.Id == outboxId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return false;
        row.Status = OutboxStatus.Pending;
        row.ProcessedAt = null;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<AuditItem>> SearchAuditAsync(
        string? actorUserId,
        Guid? correlationId,
        DateTimeOffset? from,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.AuditLogs.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        IEnumerable<AuditLog> q = rows;
        if (!string.IsNullOrWhiteSpace(actorUserId))
            q = q.Where(a => a.ActorUserId == actorUserId);
        if (correlationId is Guid cid && cid != Guid.Empty)
            q = q.Where(a => a.CorrelationId == cid);
        if (from is DateTimeOffset start)
            q = q.Where(a => a.CreatedAt >= start);
        return q.OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .Select(a => new AuditItem(a.Id, a.ActorUserId, a.Action, a.CorrelationId, a.CreatedAt, a.Details))
            .ToList();
    }
}
