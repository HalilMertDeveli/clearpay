using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClearPay.Application.Banking;
using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// Top-up / withdraw. Gateway timeout → no ledger, Pending outbox remains.
/// Clearing wallet is <see cref="Treasury.UserId"/> (no customer negative check on treasury).
/// </summary>
public sealed class SqlFundingExecutor : IFundingExecutor
{
    private readonly ClearPayDbContext _db;
    private readonly IBankGateway _gateway;
    private readonly IIdempotencyStore _idempotency;
    private readonly IClock _clock;
    private readonly IWalletSummaryCache _cache;
    private readonly IWalletLiveNotifier _live;

    public SqlFundingExecutor(
        ClearPayDbContext db,
        IBankGateway gateway,
        IIdempotencyStore idempotency,
        IClock clock,
        IWalletSummaryCache cache,
        IWalletLiveNotifier live)
    {
        _db = db;
        _gateway = gateway;
        _idempotency = idempotency;
        _clock = clock;
        _cache = cache;
        _live = live;
    }

    public async Task<FundingOutcome> ExecuteAsync(
        FundingCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            return FundingOutcome.Fail(FundingResultKind.MissingKey);

        if (command.Amount <= 0m || decimal.Round(command.Amount, 2) != command.Amount)
            return FundingOutcome.Fail(FundingResultKind.InvalidAmount);

        var scope = command.Operation == BankOperation.TopUp ? "topup" : "withdraw";
        var key = command.IdempotencyKey.Trim();
        var hash = Fingerprint(command);
        var existing = await _idempotency.FindAsync(key, scope, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return ReplayOf(existing);

        var customer = await EnsureWalletAsync(command.ActorUserId, cancellationToken).ConfigureAwait(false);
        if (command.Operation == BankOperation.Withdraw && customer.IsFrozen)
            return FundingOutcome.Fail(FundingResultKind.Frozen);

        if (command.Operation == BankOperation.Withdraw)
        {
            var amounts = await _db.LedgerEntries
                .Where(e => e.WalletId == customer.Id)
                .Select(e => e.Amount)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (LedgerPair.WouldGoNegative(amounts.Sum(), decimal.Negate(command.Amount)))
                return FundingOutcome.Fail(FundingResultKind.InsufficientFunds);
        }

        var correlationId = Guid.NewGuid();
        var gateway = await _gateway.SendAsync(
            new BankGatewayRequest(command.Operation, command.Amount, command.AccountHint, correlationId),
            cancellationToken).ConfigureAwait(false);

        if (gateway.TimedOut)
        {
            await PersistTimeoutAsync(command, scope, key, hash, correlationId, cancellationToken)
                .ConfigureAwait(false);
            return FundingOutcome.Fail(FundingResultKind.TimedOut, correlationId);
        }

        if (!gateway.Succeeded)
            return FundingOutcome.Fail(FundingResultKind.GatewayFailed, correlationId);

        var treasury = await EnsureWalletAsync(Treasury.UserId, cancellationToken).ConfigureAwait(false);
        var kind = command.Operation == BankOperation.TopUp
            ? LedgerEntryKind.TopUp
            : LedgerEntryKind.Withdraw;
        var debitWallet = command.Operation == BankOperation.TopUp ? treasury.Id : customer.Id;
        var creditWallet = command.Operation == BankOperation.TopUp ? customer.Id : treasury.Id;
        var now = _clock.UtcNow;
        var description = command.AccountHint.Trim();

        await using var tx = await _db.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            existing = await _idempotency.FindAsync(key, scope, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return ReplayOf(existing);
            }

            if (command.Operation == BankOperation.Withdraw)
            {
                await _db.Entry(customer).ReloadAsync(cancellationToken).ConfigureAwait(false);
                if (customer.IsFrozen)
                {
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return FundingOutcome.Fail(FundingResultKind.Frozen);
                }

                var live = await _db.LedgerEntries
                    .Where(e => e.WalletId == customer.Id)
                    .Select(e => e.Amount)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (LedgerPair.WouldGoNegative(live.Sum(), decimal.Negate(command.Amount)))
                {
                    await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    return FundingOutcome.Fail(FundingResultKind.InsufficientFunds);
                }
            }

            var (debit, credit) = LedgerPair.Create(
                debitWallet,
                creditWallet,
                command.Amount,
                correlationId,
                kind,
                transferId: null,
                description,
                now);

            _db.LedgerEntries.AddRange(debit, credit);
            await _idempotency
                .ReserveAsync(key, scope, hash, correlationId, cancellationToken)
                .ConfigureAwait(false);
            _db.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = command.ActorUserId,
                Action = scope,
                CorrelationId = correlationId,
                Details = JsonSerializer.Serialize(new
                {
                    amount = command.Amount,
                    reference = gateway.Reference,
                    strategy = _gateway.StrategyName
                }),
                CreatedAt = now
            });
            _db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = $"clearpay.{scope}.completed",
                Payload = JsonSerializer.Serialize(new
                {
                    correlationId,
                    amount = command.Amount,
                    userId = command.ActorUserId,
                    reference = gateway.Reference
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
                var raced = await _idempotency.FindAsync(key, scope, cancellationToken).ConfigureAwait(false);
                return raced is null
                    ? FundingOutcome.Fail(FundingResultKind.Replay, correlationId)
                    : ReplayOf(raced);
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }

        await _cache.InvalidateAsync(command.ActorUserId, cancellationToken).ConfigureAwait(false);
        await _live.NotifyAsync(
            new WalletLiveNotice(
                command.Operation == BankOperation.TopUp ? "topup" : "withdraw",
                correlationId,
                new[] { command.ActorUserId }),
            cancellationToken).ConfigureAwait(false);
        return FundingOutcome.Created(correlationId, gateway.Reference);
    }

    private async Task PersistTimeoutAsync(
        FundingCommand command,
        string scope,
        string key,
        string hash,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        await _idempotency.ReserveAsync(key, scope, "timeout:" + hash, correlationId, cancellationToken)
            .ConfigureAwait(false);
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = $"clearpay.{scope}.timeout",
            Payload = JsonSerializer.Serialize(new
            {
                correlationId,
                amount = command.Amount,
                userId = command.ActorUserId,
                accountHint = command.AccountHint,
                operation = command.Operation.ToString()
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
            _db.ChangeTracker.Clear();
        }
    }

    private static FundingOutcome ReplayOf(IdempotencyLookup existing)
    {
        var kind = existing.RequestHash is not null
            && existing.RequestHash.StartsWith("timeout:", StringComparison.Ordinal)
            ? FundingResultKind.TimedOut
            : FundingResultKind.Replay;
        return new FundingOutcome(kind, existing.ResourceId ?? Guid.Empty, null);
    }

    private static string Fingerprint(FundingCommand command)
    {
        var canonical =
            $"{command.Operation}\n{command.Amount.ToString("F2", CultureInfo.InvariantCulture)}\n{command.AccountHint.Trim()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
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
