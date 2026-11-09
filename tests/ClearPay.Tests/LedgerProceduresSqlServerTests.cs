using ClearPay.Application.Banking;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

/// <summary>
/// Runs the real migrations (indexes + procs) on a throwaway SQL Server database and checks that
/// the procedure path returns the same numbers the ledger rules demand (net of signed rows, pairs sum to 0).
/// Needs <c>CLEARPAY_TEST_SQLSERVER</c>; each test class run creates and drops its own database.
/// </summary>
public sealed class LedgerProceduresSqlServerTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly string? _server = Environment.GetEnvironmentVariable(SqlServerFactAttribute.Variable);
    private readonly string _database = "ClearPayTest_" + Guid.NewGuid().ToString("N");
    private ClearPayDbContext? _db;
    private Wallet _alice = null!;
    private Wallet _bob = null!;
    private Wallet _treasury = null!;

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(_server))
            return;

        var builder = new SqlConnectionStringBuilder(_server) { InitialCatalog = _database };
        _db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlServer(builder.ConnectionString)
            .Options);
        await _db.Database.MigrateAsync();
        await SeedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_db is null)
            return;
        await _db.Database.EnsureDeletedAsync();
        await _db.DisposeAsync();
    }

    [SqlServerFact]
    public async Task Summary_proc_matches_ledger_net_and_month_totals()
    {
        var all = await _db!.LedgerEntries.AsNoTracking().ToListAsync();
        var procs = new LedgerProcedures(_db);
        procs.IsAvailable.Should().BeTrue();

        var totals = await procs.GetWalletSummaryAsync(_alice.Id, new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero));

        totals.Balance.Should().Be(LedgerPair.NetOf(all, _alice.Id));
        totals.Balance.Should().Be(100m - 30m + 12.5m - 7m);
        totals.MonthOutgoing.Should().Be(30m + 7m);
        totals.MonthIncoming.Should().Be(12.5m);

        var reader = new SqlWalletReader(_db, new FixedClock(Now));
        var summary = await reader.GetByUserIdAsync("alice");
        summary!.Balance.Should().Be(totals.Balance);
        summary.MonthOutgoing.Should().Be(37m);
        summary.MonthIncoming.Should().Be(12.5m);
        summary.LastMovements.Select(m => m.Amount).Should().Equal(-7m, 12.5m, -30m, 100m);
        summary.LastMovements[0].Kind.Should().Be(nameof(LedgerEntryKind.Withdraw));
    }

    [SqlServerFact]
    public async Task Statement_proc_pages_filters_counts_and_names_counterparty()
    {
        var reader = new SqlActivityReader(_db!, new EmailDirectory());

        var first = await reader.ListAsync("alice", null, null, null, page: 1, pageSize: 3);
        first.TotalCount.Should().Be(4);
        first.Items.Select(i => i.SignedAmount).Should().Equal(-7m, 12.5m, -30m);
        first.Items[0].Counterparty.Should().Be("ClearPay treasury (demo)");
        first.Items[1].Counterparty.Should().Be("bob@clearpay.test");

        var second = await reader.ListAsync("alice", null, null, null, page: 2, pageSize: 3);
        second.Items.Should().ContainSingle().Which.SignedAmount.Should().Be(100m);
        second.TotalCount.Should().Be(4);

        var pastEnd = await reader.ListAsync("alice", null, null, null, page: 5, pageSize: 3);
        pastEnd.Items.Should().BeEmpty();
        pastEnd.TotalCount.Should().Be(4);

        var transfers = await reader.ListAsync("alice", null, null, "Transfer", page: 1);
        transfers.TotalCount.Should().Be(2);
        transfers.Items.Should().OnlyContain(i => i.Kind == nameof(LedgerEntryKind.Transfer));

        var october = await reader.ListAsync(
            "alice",
            from: new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero),
            to: new DateTimeOffset(2026, 10, 15, 0, 0, 0, TimeSpan.Zero),
            kind: null,
            page: 1);
        october.TotalCount.Should().Be(2);
        october.Items.Select(i => i.SignedAmount).Should().Equal(12.5m, -30m);
    }

    [SqlServerFact]
    public async Task Daily_totals_proc_returns_balanced_kinds_for_reconciliation()
    {
        var day = new DateTimeOffset(2026, 10, 5, 0, 0, 0, TimeSpan.Zero);

        var rows = await new LedgerProcedures(_db!).GetDailyTotalsAsync(day, day.AddDays(1));

        rows.Should().ContainSingle();
        var transfer = rows[0];
        transfer.Kind.Should().Be((int)LedgerEntryKind.Transfer);
        transfer.EntryCount.Should().Be(2);
        transfer.CreditTotal.Should().Be(30m);
        transfer.DebitTotal.Should().Be(30m);
        transfer.Net.Should().Be(0m, "every movement is a +/- pair");
    }

    private async Task SeedAsync()
    {
        _alice = NewWallet("alice");
        _bob = NewWallet("bob");
        _treasury = NewWallet(Treasury.UserId);
        _db!.Wallets.AddRange(_alice, _bob, _treasury);

        Add(_treasury, _alice, 100m, LedgerEntryKind.TopUp, new DateTimeOffset(2026, 9, 20, 9, 0, 0, TimeSpan.Zero));
        Add(_alice, _bob, 30m, LedgerEntryKind.Transfer, new DateTimeOffset(2026, 10, 5, 9, 0, 0, TimeSpan.Zero));
        Add(_bob, _alice, 12.5m, LedgerEntryKind.Transfer, new DateTimeOffset(2026, 10, 10, 9, 0, 0, TimeSpan.Zero));
        Add(_alice, _treasury, 7m, LedgerEntryKind.Withdraw, new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.Zero));
        await _db.SaveChangesAsync();
    }

    private void Add(Wallet from, Wallet to, decimal amount, LedgerEntryKind kind, DateTimeOffset at)
    {
        var (debit, credit) = LedgerPair.Create(from.Id, to.Id, amount, Guid.NewGuid(), kind, at: at);
        _db!.LedgerEntries.AddRange(debit, credit);
    }

    private static Wallet NewWallet(string userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CreatedAt = Now.AddYears(-1)
    };

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class EmailDirectory : IUserDirectory
    {
        public Task<string?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);

        public Task<string?> FindEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(userId + "@clearpay.test");
    }
}
