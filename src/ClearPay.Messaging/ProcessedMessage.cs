using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClearPay.Messaging;

/// <summary>
/// Consumer idempotency: one row per (consumer, message id) that was handled successfully.
/// The unique index is the real guard; the "already there?" read before it is only a fast path.
/// </summary>
public sealed class ProcessedMessage
{
    public long Id { get; set; }

    public Guid MessageId { get; set; }

    /// <summary>Consumer (queue) name: two services may each process the same event once.</summary>
    public string Consumer { get; set; } = string.Empty;

    public string MessageType { get; set; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; set; }
}

public static class ProcessedMessageModel
{
    public const string UniqueIndexName = "UX_ProcessedMessage_Consumer_MessageId";

    /// <summary>Maps <see cref="ProcessedMessage"/> into the service's own schema.</summary>
    public static ModelBuilder AddProcessedMessages(this ModelBuilder modelBuilder, string? schema)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.Entity<ProcessedMessage>(b =>
        {
            b.ToTable("ProcessedMessage", schema);
            b.HasKey(p => p.Id);
            b.Property(p => p.Consumer).IsRequired().HasMaxLength(128);
            b.Property(p => p.MessageType).IsRequired().HasMaxLength(128);
            b.HasIndex(p => new { p.Consumer, p.MessageId }).IsUnique().HasDatabaseName(UniqueIndexName);
        });
        return modelBuilder;
    }
}

/// <summary>
/// Runs a handler's database work and the <see cref="ProcessedMessage"/> insert in one transaction:
/// either both commit or neither does. A redelivered message (broker retry, publisher re-send) finds
/// its row and is skipped; two copies racing on two consumers hit the unique index and one rolls back.
/// Side effects outside the database (a real e-mail) cannot join the transaction: they must be
/// idempotent on their own, e.g. by passing <see cref="ProcessedMessage.MessageId"/> to the provider.
/// </summary>
public static class IdempotentProcessing
{
    /// <returns>True when the work ran and committed; false when the message was already processed.</returns>
    public static async Task<bool> RunOnceAsync(
        DbContext db,
        string consumer,
        IncomingMessage message,
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(work);
        var processed = db.Set<ProcessedMessage>();

        if (await AlreadyProcessedAsync(processed, consumer, message.MessageId, cancellationToken).ConfigureAwait(false))
            return false;

        await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await work(cancellationToken).ConfigureAwait(false);
        processed.Add(new ProcessedMessage
        {
            MessageId = message.MessageId,
            Consumer = consumer,
            MessageType = message.Type,
            ProcessedAt = DateTimeOffset.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            db.ChangeTracker.Clear();
            // Lost the race against a parallel copy of the same message: that copy did the work.
            if (await AlreadyProcessedAsync(processed, consumer, message.MessageId, cancellationToken).ConfigureAwait(false))
                return false;
            throw;
        }
    }

    private static Task<bool> AlreadyProcessedAsync(
        DbSet<ProcessedMessage> processed,
        string consumer,
        Guid messageId,
        CancellationToken cancellationToken) =>
        processed.AsNoTracking().AnyAsync(p => p.Consumer == consumer && p.MessageId == messageId, cancellationToken);
}
