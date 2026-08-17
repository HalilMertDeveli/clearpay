using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

public sealed class AdminPanelTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ClearPayDbContext _db;
    private readonly SqlAdminPanel _admin;

    public AdminPanelTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _admin = new SqlAdminPanel(_db, new Dir(), new NoopCache(), new Clock());
    }

    [Fact]
    public async Task Freeze_sets_flag_without_updating_balance()
    {
        var ok = await _admin.FreezeByEmailAsync("a@clearpay.test", "admin-1");
        ok.Should().BeTrue();
        var wallet = await _db.Wallets.SingleAsync(w => w.UserId == "user-a");
        wallet.IsFrozen.Should().BeTrue();
        _db.Model.FindEntityType(typeof(Wallet))!.FindProperty("Balance").Should().BeNull();
        (await _db.AuditLogs.CountAsync(a => a.Action == "wallet.freeze")).Should().Be(1);
    }

    [Fact]
    public async Task Unfreeze_clears_flag_without_updating_balance()
    {
        (await _admin.FreezeByEmailAsync("a@clearpay.test", "admin-1")).Should().BeTrue();
        (await _admin.UnfreezeByEmailAsync("a@clearpay.test", "admin-1")).Should().BeTrue();
        var wallet = await _db.Wallets.SingleAsync(w => w.UserId == "user-a");
        wallet.IsFrozen.Should().BeFalse();
        _db.Model.FindEntityType(typeof(Wallet))!.FindProperty("Balance").Should().BeNull();
        (await _db.AuditLogs.CountAsync(a => a.Action == "wallet.unfreeze")).Should().Be(1);
        (await _admin.UnfreezeByEmailAsync("missing@clearpay.test", "admin-1")).Should().BeFalse();
    }

    [Fact]
    public async Task Requeue_moves_failed_to_pending()
    {
        var msg = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "clearpay.topup.timeout",
            Payload = "{}",
            CorrelationId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
            Status = OutboxStatus.Failed
        };
        _db.OutboxMessages.Add(msg);
        await _db.SaveChangesAsync();

        (await _admin.ListFailedAsync()).Should().ContainSingle(x => x.Id == msg.Id);
        (await _admin.RequeueAsync(msg.Id)).Should().BeTrue();
        (await _db.OutboxMessages.SingleAsync()).Status.Should().Be(OutboxStatus.Pending);
        (await _admin.ListFailedAsync()).Should().BeEmpty();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class Dir : IUserDirectory
    {
        public Task<string?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(email.Contains("a@") ? "user-a" : null);

        public Task<string?> FindEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>("a@clearpay.test");
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

    private sealed class Clock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
