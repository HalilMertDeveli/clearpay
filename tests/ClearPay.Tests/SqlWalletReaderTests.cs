using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using ClearPay.Infrastructure.Time;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

public sealed class SqlWalletReaderTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ClearPayDbContext _db;
    private readonly FixedClock _clock;
    private readonly SqlWalletReader _reader;

    public SqlWalletReaderTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new ClearPayDbContext(options);
        _db.Database.EnsureCreated();
        _clock = new FixedClock(new DateTimeOffset(2026, 8, 13, 12, 0, 0, TimeSpan.Zero));
        _reader = new SqlWalletReader(_db, _clock);
    }

    [Fact]
    public async Task Missing_wallet_is_created_with_zero_net()
    {
        var userId = "user-a";
        var summary = await _reader.GetByUserIdAsync(userId);

        summary.Should().NotBeNull();
        summary!.UserId.Should().Be(userId);
        summary.WalletId.Should().NotBe(Guid.Empty);
        summary.Balance.Should().Be(0m);
        summary.MonthOutgoing.Should().Be(0m);
        summary.MonthIncoming.Should().Be(0m);
        summary.IsFrozen.Should().BeFalse();
        summary.LastMovements.Should().BeEmpty();
        _db.Wallets.Should().ContainSingle(w => w.UserId == userId);
    }

    [Fact]
    public async Task Balance_is_ledger_net_not_a_column()
    {
        var wallet = await SeedWalletAsync("user-b", frozen: false);
        var counterparty = await SeedWalletAsync("user-c", frozen: false);
        var (debit, credit) = LedgerPair.Create(
            wallet.Id, counterparty.Id, 40m, Guid.NewGuid(), LedgerEntryKind.Transfer, at: _clock.UtcNow);
        var (backDebit, backCredit) = LedgerPair.Create(
            counterparty.Id, wallet.Id, 15m, Guid.NewGuid(), LedgerEntryKind.Transfer, at: _clock.UtcNow);
        _db.LedgerEntries.AddRange(debit, credit, backDebit, backCredit);
        await _db.SaveChangesAsync();

        var summary = await _reader.GetByUserIdAsync("user-b");

        summary!.Balance.Should().Be(LedgerPair.NetOf(_db.LedgerEntries.ToList(), wallet.Id));
        summary.Balance.Should().Be(-25m);
        summary.MonthOutgoing.Should().Be(40m);
        summary.MonthIncoming.Should().Be(15m);
        summary.LastMovements.Should().HaveCount(2);
        summary.LastMovements[0].At.Should().BeOnOrAfter(summary.LastMovements[1].At);
    }

    [Fact]
    public async Task Frozen_badge_comes_from_wallet_not_balance_update()
    {
        await SeedWalletAsync("frozen-user", frozen: true);
        var summary = await _reader.GetByUserIdAsync("frozen-user");
        summary!.IsFrozen.Should().BeTrue();
        summary.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Last_movements_cap_at_five_newest()
    {
        var wallet = await SeedWalletAsync("busy", frozen: false);
        var other = await SeedWalletAsync("other", frozen: false);
        for (var i = 0; i < 6; i++)
        {
            var at = _clock.UtcNow.AddMinutes(i);
            var (debit, credit) = LedgerPair.Create(
                other.Id, wallet.Id, 1m + i, Guid.NewGuid(), LedgerEntryKind.TopUp, at: at);
            _db.LedgerEntries.AddRange(debit, credit);
        }

        await _db.SaveChangesAsync();
        var summary = await _reader.GetByUserIdAsync("busy");
        summary!.LastMovements.Should().HaveCount(5);
        summary.LastMovements.Select(m => m.Amount).Should().Equal(6m, 5m, 4m, 3m, 2m);
        summary.Balance.Should().Be(21m);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<Wallet> SeedWalletAsync(string userId, bool frozen)
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IsFrozen = frozen,
            CreatedAt = _clock.UtcNow
        };
        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;

        public DateTimeOffset UtcNow { get; }
    }
}
