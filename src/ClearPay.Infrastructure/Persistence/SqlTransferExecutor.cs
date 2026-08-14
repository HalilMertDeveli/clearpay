using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// One SQL transaction: debit, credit, Transfer, idempotency, audit, outbox.
/// Duplicate Key → 409. No UPDATE Balance. PageModels do not call this type.
/// </summary>
public sealed class SqlTransferExecutor : ITransferExecutor
{
    public const string TransferScope = "transfer";

    private readonly ClearPayDbContext _db;
    private readonly IUserDirectory _users;
    private readonly IIdempotencyStore _idempotency;
    private readonly IClock _clock;
    private readonly IWalletSummaryCache _cache;

    public SqlTransferExecutor(
        ClearPayDbContext db,
        IUserDirectory users,
        IIdempotencyStore idempotency,
        IClock clock,
        IWalletSummaryCache cache)
    {
        _db = db;
        _users = users;
        _idempotency = idempotency;
        _clock = clock;
        _cache = cache;
    }

    public async Task<TransferOutcome> ExecuteAsync(
        TransferCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            return TransferOutcome.Fail(TransferResultKind.MissingKey);

        if (command.Amount <= 0m || decimal.Round(command.Amount, 2) != command.Amount)
            return TransferOutcome.Fail(TransferResultKind.InvalidAmount);

        var recipientEmail = command.Recipient.Trim();
        var hash = RequestFingerprint.Hash(recipientEmail, command.Amount, command.Description);
        var key = command.IdempotencyKey.Trim();

        try
        {
            return await ExecuteCoreAsync(command, recipientEmail, hash, key, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (LedgerUnavailable.Matches(ex))
        {
            return TransferOutcome.Fail(TransferResultKind.Unavailable);
        }
    }

    private async Task<TransferOutcome> ExecuteCoreAsync(
        TransferCommand command,
        string recipientEmail,
        string hash,
        string key,
        CancellationToken cancellationToken)
    {
        var existing = await _idempotency
            .FindAsync(key, TransferScope, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
            return await ReplayOfAsync(existing, hash, cancellationToken).ConfigureAwait(false);

        var recipientUserId = await _users
            .FindUserIdByEmailAsync(recipientEmail, cancellationToken)
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(recipientUserId))
            return TransferOutcome.Fail(TransferResultKind.RecipientNotFound);

        if (string.Equals(command.ActorUserId, recipientUserId, StringComparison.Ordinal))
            return TransferOutcome.Fail(TransferResultKind.SelfTransfer);

        Guid transferId;
        Guid correlationId;

        await using var tx = await _db.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            existing = await _idempotency
                .FindAsync(key, TransferScope, cancellationToken)
                .ConfigureAwait(false);
            if (existing is not null)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return await ReplayOfAsync(existing, hash, cancellationToken).ConfigureAwait(false);
            }

            var sender = await EnsureWalletAsync(command.ActorUserId, cancellationToken).ConfigureAwait(false);
            var recipient = await EnsureWalletAsync(recipientUserId, cancellationToken).ConfigureAwait(false);

            if (sender.IsFrozen)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return TransferOutcome.Fail(TransferResultKind.FrozenSender);
            }

            var senderAmounts = await _db.LedgerEntries
                .Where(e => e.WalletId == sender.Id)
                .Select(e => e.Amount)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var senderNet = senderAmounts.Sum();
            if (LedgerPair.WouldGoNegative(senderNet, decimal.Negate(command.Amount)))
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return TransferOutcome.Fail(TransferResultKind.InsufficientFunds);
            }

            var now = _clock.UtcNow;
            correlationId = Guid.NewGuid();
            transferId = Guid.NewGuid();
            var description = string.IsNullOrWhiteSpace(command.Description)
                ? null
                : command.Description.Trim();
            var transfer = new Transfer
            {
                Id = transferId,
                FromWalletId = sender.Id,
                ToWalletId = recipient.Id,
                Amount = command.Amount,
                Description = description,
                Status = TransferStatus.Completed,
                CorrelationId = correlationId,
                CreatedAt = now
            };

            var (debit, credit) = LedgerPair.Create(
                sender.Id,
                recipient.Id,
                command.Amount,
                correlationId,
                LedgerEntryKind.Transfer,
                transferId,
                description,
                now);

            _db.Transfers.Add(transfer);
            _db.LedgerEntries.AddRange(debit, credit);
            await _idempotency
                .ReserveAsync(key, TransferScope, hash, transferId, cancellationToken)
                .ConfigureAwait(false);
            _db.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = command.ActorUserId,
                Action = "transfer",
                CorrelationId = correlationId,
                Details = JsonSerializer.Serialize(new
                {
                    transferId,
                    amount = command.Amount,
                    fromWalletId = sender.Id,
                    toWalletId = recipient.Id
                }),
                CreatedAt = now
            });
            _db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "clearpay.transfer.completed",
                Payload = JsonSerializer.Serialize(new
                {
                    transferId,
                    correlationId,
                    amount = command.Amount,
                    fromUserId = command.ActorUserId,
                    toUserId = recipientUserId
                }),
                CorrelationId = correlationId,
                OccurredAt = now,
                Status = OutboxStatus.Pending
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (UniqueConstraint.IsDuplicateKey(ex))
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _db.ChangeTracker.Clear();
                var raced = await _idempotency
                    .FindAsync(key, TransferScope, cancellationToken)
                    .ConfigureAwait(false);
                if (raced is not null)
                    return await ReplayOfAsync(raced, hash, cancellationToken).ConfigureAwait(false);
                return TransferOutcome.Fail(TransferResultKind.Replay);
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        await _cache.InvalidateAsync(command.ActorUserId, cancellationToken).ConfigureAwait(false);
        await _cache.InvalidateAsync(recipientUserId, cancellationToken).ConfigureAwait(false);
        return TransferOutcome.Created(transferId, correlationId);
    }

    private async Task<TransferOutcome> ReplayOfAsync(
        IdempotencyLookup existing,
        string requestHash,
        CancellationToken cancellationToken)
    {
        var kind = string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal)
            ? TransferResultKind.Replay
            : TransferResultKind.KeyPayloadMismatch;
        if (existing.ResourceId is Guid transferId)
        {
            var transfer = await _db.Transfers.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == transferId, cancellationToken)
                .ConfigureAwait(false);
            return new TransferOutcome(kind, transferId, transfer?.CorrelationId ?? Guid.Empty);
        }

        return new TransferOutcome(kind, existing.ResourceId, Guid.Empty);
    }

    private async Task<Wallet> EnsureWalletAsync(string userId, CancellationToken cancellationToken)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (wallet is not null)
            return wallet;

        wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsFrozen = false,
            CreatedAt = _clock.UtcNow
        };
        _db.Wallets.Add(wallet);
        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return wallet;
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            return await _db.Wallets.SingleAsync(w => w.UserId == userId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

internal static class RequestFingerprint
{
    public static string Hash(string recipientEmail, decimal amount, string? description)
    {
        var canonical =
            $"{recipientEmail.Trim().ToUpperInvariant()}\n{amount.ToString("F2", CultureInfo.InvariantCulture)}\n{description?.Trim() ?? string.Empty}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
