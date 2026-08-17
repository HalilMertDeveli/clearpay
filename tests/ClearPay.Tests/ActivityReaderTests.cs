using ClearPay.Application.Banking;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

public sealed class ActivityReaderTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ClearPayDbContext _db;
    private readonly SqlActivityReader _reader;

    public ActivityReaderTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _reader = new SqlActivityReader(_db, new StubDirectory());
    }

    [Fact]
    public async Task List_and_receipt_use_correlation_id_without_balance_column()
    {
        var a = new Wallet { Id = Guid.NewGuid(), UserId = "a", CreatedAt = DateTimeOffset.UtcNow };
        var b = new Wallet { Id = Guid.NewGuid(), UserId = "b", CreatedAt = DateTimeOffset.UtcNow };
        _db.Wallets.AddRange(a, b);
        var correlation = Guid.NewGuid();
        var (debit, credit) = LedgerPair.Create(a.Id, b.Id, 12.5m, correlation, LedgerEntryKind.Transfer, description: "çay");
        _db.LedgerEntries.AddRange(debit, credit);
        await _db.SaveChangesAsync();

        var page = await _reader.ListAsync("a", from: null, to: null, kind: "Transfer", page: 1);
        page.TotalCount.Should().Be(1);
        page.Items[0].CorrelationId.Should().Be(correlation);
        page.Items[0].SignedAmount.Should().Be(-12.5m);

        var receipt = await _reader.GetReceiptAsync("b", correlation);
        receipt.Should().NotBeNull();
        receipt!.Amount.Should().Be(12.5m);
        receipt.CorrelationId.Should().Be(correlation);
        receipt.DebitParty.Should().Be("a@clearpay.test");
        receipt.CreditParty.Should().Be("b@clearpay.test");

        (await _reader.GetReceiptAsync("stranger", correlation)).Should().BeNull();
    }

    [Fact]
    public async Task List_to_date_excludes_later_rows_and_receipt_shows_last4_hint()
    {
        var a = new Wallet { Id = Guid.NewGuid(), UserId = "a", CreatedAt = DateTimeOffset.UtcNow };
        var treasury = new Wallet { Id = Guid.NewGuid(), UserId = Treasury.UserId, CreatedAt = DateTimeOffset.UtcNow };
        _db.Wallets.AddRange(a, treasury);
        var oldCorr = Guid.NewGuid();
        var newCorr = Guid.NewGuid();
        var day = new DateTimeOffset(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);
        var later = new DateTimeOffset(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);
        var (debitOld, creditOld) = LedgerPair.Create(
            a.Id, treasury.Id, 5m, oldCorr, LedgerEntryKind.Withdraw, description: "****1234", at: day);
        var (debitNew, creditNew) = LedgerPair.Create(
            treasury.Id, a.Id, 8m, newCorr, LedgerEntryKind.TopUp, description: "****9999", at: later);
        _db.LedgerEntries.AddRange(debitOld, creditOld, debitNew, creditNew);
        await _db.SaveChangesAsync();

        var to = new DateTimeOffset(2026, 8, 11, 0, 0, 0, TimeSpan.Zero);
        var page = await _reader.ListAsync("a", from: null, to: to, kind: null, page: 1);
        page.TotalCount.Should().Be(1);
        page.Items[0].CorrelationId.Should().Be(oldCorr);

        var receipt = await _reader.GetReceiptAsync("a", oldCorr);
        receipt.Should().NotBeNull();
        receipt!.InstrumentHint.Should().Be("****1234");
        receipt.CorrelationId.Should().Be(oldCorr);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class StubDirectory : IUserDirectory
    {
        public Task<string?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);

        public Task<string?> FindEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(userId + "@clearpay.test");
    }
}
