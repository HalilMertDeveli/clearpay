using ClearPay.Application.Banking;
using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Banking;
using ClearPay.Infrastructure.Persistence;
using ClearPay.Infrastructure.Realtime;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ClearPay.Tests;

public sealed class FundingExecutorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ClearPayDbContext _db;
    private readonly SqlFundingExecutor _executor;
    private readonly FixedClock _clock;

    public FundingExecutorTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();
        _clock = new FixedClock(new DateTimeOffset(2026, 8, 14, 12, 0, 0, TimeSpan.Zero));
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _executor = new SqlFundingExecutor(
            _db,
            new RestBankGateway(config),
            new SqlIdempotencyStore(_db, _clock),
            _clock,
            new NoopCache(),
            new NoOpWalletLiveNotifier());
    }

    [Fact]
    public async Task Top_up_credits_customer_from_treasury_without_balance_column()
    {
        var userId = "user-top";
        var outcome = await _executor.ExecuteAsync(
            new FundingCommand(userId, BankOperation.TopUp, 50m, "TR00", "top-1"));

        outcome.IsSuccess.Should().BeTrue();
        var customer = await _db.Wallets.SingleAsync(w => w.UserId == userId);
        var treasury = await _db.Wallets.SingleAsync(w => w.UserId == Treasury.UserId);
        var rows = await _db.LedgerEntries.AsNoTracking().ToListAsync();
        LedgerPair.NetOf(rows, customer.Id).Should().Be(50m);
        LedgerPair.NetOf(rows, treasury.Id).Should().Be(-50m);
        rows.Should().OnlyContain(e => e.Kind == LedgerEntryKind.TopUp);
        (await _db.Transfers.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Timeout_does_not_post_ledger_but_keeps_outbox()
    {
        var userId = "user-to";
        var outcome = await _executor.ExecuteAsync(
            new FundingCommand(userId, BankOperation.TopUp, 20m, "TIMEOUT", "to-1"));

        outcome.Kind.Should().Be(FundingResultKind.TimedOut);
        (await _db.LedgerEntries.CountAsync()).Should().Be(0);
        (await _db.OutboxMessages.CountAsync(m => m.Status == OutboxStatus.Pending)).Should().Be(1);
        var replay = await _executor.ExecuteAsync(
            new FundingCommand(userId, BankOperation.TopUp, 20m, "TIMEOUT", "to-1"));
        replay.Kind.Should().Be(FundingResultKind.TimedOut);
        (await _db.OutboxMessages.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Withdraw_requires_funds_and_frozen_blocks()
    {
        var userId = "user-wd";
        (await _executor.ExecuteAsync(
            new FundingCommand(userId, BankOperation.Withdraw, 5m, "TR00", "wd-poor")))
            .Kind.Should().Be(FundingResultKind.InsufficientFunds);

        (await _executor.ExecuteAsync(
            new FundingCommand(userId, BankOperation.TopUp, 30m, "TR00", "wd-fund")))
            .IsSuccess.Should().BeTrue();

        var wallet = await _db.Wallets.SingleAsync(w => w.UserId == userId);
        wallet.IsFrozen = true;
        await _db.SaveChangesAsync();

        (await _executor.ExecuteAsync(
            new FundingCommand(userId, BankOperation.Withdraw, 5m, "TR00", "wd-fr")))
            .Kind.Should().Be(FundingResultKind.Frozen);

        wallet.IsFrozen = false;
        await _db.SaveChangesAsync();

        var ok = await _executor.ExecuteAsync(
            new FundingCommand(userId, BankOperation.Withdraw, 10m, "TR00", "wd-ok"));
        ok.IsSuccess.Should().BeTrue();
        var rows = await _db.LedgerEntries.AsNoTracking().ToListAsync();
        LedgerPair.NetOf(rows, wallet.Id).Should().Be(20m);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class NoopCache : IWalletSummaryCache
    {
        public Task<WalletSummary?> GetAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<WalletSummary?>(null);

        public Task SetAsync(WalletSummary summary, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task InvalidateAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;

        public DateTimeOffset UtcNow { get; }
    }
}
