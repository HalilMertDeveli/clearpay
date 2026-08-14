using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using ClearPay.Application.Wallets;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

public sealed class TransferExecutorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ClearPayDbContext _db;
    private readonly RecordingCache _cache;
    private readonly MapDirectory _users;
    private readonly SqlTransferExecutor _executor;
    private readonly FixedClock _clock;

    public TransferExecutorTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new ClearPayDbContext(options);
        _db.Database.EnsureCreated();
        _clock = new FixedClock(new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero));
        _cache = new RecordingCache();
        _users = new MapDirectory();
        var store = new SqlIdempotencyStore(_db, _clock);
        _executor = new SqlTransferExecutor(_db, _users, store, _clock, _cache);
    }

    [Fact]
    public async Task Duplicate_key_is_replay_and_does_not_debit_twice()
    {
        var senderId = "sender-1";
        var recipientId = "recipient-1";
        _users.Map("sender@clearpay.test", senderId);
        _users.Map("alici@clearpay.test", recipientId);
        await FundAsync(senderId, 100m);

        var command = new TransferCommand(senderId, "alici@clearpay.test", 40m, "çay", "key-409");
        var first = await _executor.ExecuteAsync(command);
        var second = await _executor.ExecuteAsync(command);

        first.IsSuccess.Should().BeTrue();
        second.IsReplay.Should().BeTrue();
        second.Kind.Should().Be(TransferResultKind.Replay);
        second.TransferId.Should().Be(first.TransferId);

        var senderWallet = await _db.Wallets.SingleAsync(w => w.UserId == senderId);
        var recipientWallet = await _db.Wallets.SingleAsync(w => w.UserId == recipientId);
        var rows = await _db.LedgerEntries.AsNoTracking().ToListAsync();
        LedgerPair.NetOf(rows, senderWallet.Id).Should().Be(60m);
        LedgerPair.NetOf(rows, recipientWallet.Id).Should().Be(40m);
        (await _db.Transfers.CountAsync()).Should().Be(1);
        (await _db.IdempotencyRecords.CountAsync()).Should().Be(1);
        (await _db.OutboxMessages.CountAsync(m => m.Status == OutboxStatus.Pending)).Should().Be(1);
        (await _db.AuditLogs.CountAsync()).Should().Be(1);
        _cache.Invalidated.Should().Equal(senderId, recipientId);
    }

    [Fact]
    public async Task Duplicate_key_different_payload_is_mismatch_and_does_not_debit_twice()
    {
        var senderId = "sender-hash";
        var recipientId = "recipient-hash";
        _users.Map("sender@clearpay.test", senderId);
        _users.Map("alici@clearpay.test", recipientId);
        await FundAsync(senderId, 100m);

        var first = await _executor.ExecuteAsync(
            new TransferCommand(senderId, "alici@clearpay.test", 40m, "çay", "key-mix"));
        var second = await _executor.ExecuteAsync(
            new TransferCommand(senderId, "alici@clearpay.test", 70m, "çay", "key-mix"));

        first.IsSuccess.Should().BeTrue();
        second.Kind.Should().Be(TransferResultKind.KeyPayloadMismatch);
        second.TransferId.Should().Be(first.TransferId);

        var senderWallet = await _db.Wallets.SingleAsync(w => w.UserId == senderId);
        var rows = await _db.LedgerEntries.AsNoTracking().ToListAsync();
        LedgerPair.NetOf(rows, senderWallet.Id).Should().Be(60m);
        rows.Sum(e => e.Amount).Should().Be(0m);
        (await _db.Transfers.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Insufficient_funds_writes_nothing()
    {
        var senderId = "sender-2";
        var recipientId = "recipient-2";
        _users.Map("a@clearpay.test", senderId);
        _users.Map("b@clearpay.test", recipientId);
        await FundAsync(senderId, 5m);

        var outcome = await _executor.ExecuteAsync(
            new TransferCommand(senderId, "b@clearpay.test", 10m, null, "key-poor"));

        outcome.Kind.Should().Be(TransferResultKind.InsufficientFunds);
        (await _db.Transfers.CountAsync()).Should().Be(0);
        (await _db.LedgerEntries.CountAsync(e => e.Kind == LedgerEntryKind.Transfer)).Should().Be(0);
        _cache.Invalidated.Should().BeEmpty();
    }

    [Fact]
    public async Task Frozen_sender_cannot_send()
    {
        var senderId = "frozen";
        var recipientId = "ok";
        _users.Map("frozen@clearpay.test", senderId);
        _users.Map("ok@clearpay.test", recipientId);
        var sender = await FundAsync(senderId, 50m);
        sender.IsFrozen = true;
        await _db.SaveChangesAsync();

        var outcome = await _executor.ExecuteAsync(
            new TransferCommand(senderId, "ok@clearpay.test", 10m, null, "key-freeze"));

        outcome.Kind.Should().Be(TransferResultKind.FrozenSender);
        (await _db.Transfers.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Self_and_missing_recipient_do_not_move_money()
    {
        var senderId = "solo";
        _users.Map("solo@clearpay.test", senderId);
        await FundAsync(senderId, 20m);

        var self = await _executor.ExecuteAsync(
            new TransferCommand(senderId, "solo@clearpay.test", 5m, null, "key-self"));
        var missing = await _executor.ExecuteAsync(
            new TransferCommand(senderId, "yok@clearpay.test", 5m, null, "key-miss"));

        self.Kind.Should().Be(TransferResultKind.SelfTransfer);
        missing.Kind.Should().Be(TransferResultKind.RecipientNotFound);
        (await _db.Transfers.CountAsync()).Should().Be(0);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<Wallet> FundAsync(string userId, decimal amount)
    {
        var sender = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = _clock.UtcNow
        };
        var funder = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = "fund-" + Guid.NewGuid().ToString("N"),
            CreatedAt = _clock.UtcNow
        };
        _db.Wallets.AddRange(funder, sender);
        var (debit, credit) = LedgerPair.Create(
            funder.Id, sender.Id, amount, Guid.NewGuid(), LedgerEntryKind.TopUp, at: _clock.UtcNow);
        _db.LedgerEntries.AddRange(debit, credit);
        await _db.SaveChangesAsync();
        return sender;
    }

    private sealed class MapDirectory : IUserDirectory
    {
        private readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase);

        public void Map(string email, string userId) => _map[email] = userId;

        public Task<string?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_map.TryGetValue(email.Trim(), out var id) ? id : null);
        }

        public Task<string?> FindEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var match = _map.FirstOrDefault(p => p.Value == userId);
            return Task.FromResult(match.Key is null ? null : match.Key);
        }
    }

    private sealed class RecordingCache : IWalletSummaryCache
    {
        public List<string> Invalidated { get; } = [];

        public Task<WalletSummary?> GetAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<WalletSummary?>(null);

        public Task SetAsync(WalletSummary summary, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task InvalidateAsync(string userId, CancellationToken cancellationToken = default)
        {
            Invalidated.Add(userId);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;

        public DateTimeOffset UtcNow { get; }
    }
}
