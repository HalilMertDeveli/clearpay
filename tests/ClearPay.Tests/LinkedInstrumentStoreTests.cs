using ClearPay.Application.Funding;
using ClearPay.Application.Ports;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

public sealed class LinkedInstrumentStoreTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ClearPayDbContext _db;
    private readonly SqlLinkedInstrumentStore _store;

    public LinkedInstrumentStoreTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new ClearPayDbContext(new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();
        _store = new SqlLinkedInstrumentStore(_db, new TestClock());
    }

    [Fact]
    public async Task Add_stores_last4_without_pan()
    {
        var dto = await _store.AddAsync("user-1", "1234", "Maas");
        dto.Should().NotBeNull();
        dto!.Last4.Should().Be("1234");
        dto.AccountHint.Should().Be("****1234");
        dto.Label.Should().Be("Maas");
        dto.Scheme.Should().Be(CardNetwork.Unknown);

        var row = await _db.LinkedInstruments.AsNoTracking().SingleAsync();
        row.Last4.Should().Be("1234");
        typeof(LinkedInstrument).GetProperty("Pan").Should().BeNull();
        typeof(LinkedInstrument).GetProperty("Cvv").Should().BeNull();
    }

    [Fact]
    public async Task Add_stores_scheme_without_pan()
    {
        var dto = await _store.AddAsync("user-1", "1111", "Yapı Kredi", CardNetwork.Visa);
        dto.Should().NotBeNull();
        dto!.Scheme.Should().Be(CardNetwork.Visa);
        dto.Label.Should().Be("Yapı Kredi");
        (await _db.LinkedInstruments.AsNoTracking().SingleAsync()).Scheme.Should().Be(CardNetwork.Visa);
    }

    [Fact]
    public async Task Add_rejects_full_pan_and_non_digits()
    {
        (await _store.AddAsync("user-1", "4111111111111111", "x")).Should().BeNull();
        (await _store.AddAsync("user-1", "12ab", "x")).Should().BeNull();
        (await _store.AddAsync("user-1", "12", "x")).Should().BeNull();
    }

    [Fact]
    public async Task Add_duplicate_last4_returns_null()
    {
        (await _store.AddAsync("user-1", "9999", "A")).Should().NotBeNull();
        (await _store.AddAsync("user-1", "9999", "B")).Should().BeNull();
        (await _store.ListAsync("user-1")).Should().HaveCount(1);
    }

    [Fact]
    public async Task Add_rejects_sixth_card()
    {
        for (var i = 1; i <= 5; i++)
            (await _store.AddAsync("user-1", $"{i}{i}{i}{i}", "n")).Should().NotBeNull();

        (await _store.AddAsync("user-1", "0000", "n")).Should().BeNull();
        (await _store.ListAsync("user-1")).Should().HaveCount(5);
    }

    [Fact]
    public async Task List_is_per_user()
    {
        await _store.AddAsync("a", "1111", "A");
        await _store.AddAsync("b", "2222", "B");
        (await _store.ListAsync("a")).Should().ContainSingle(x => x.Last4 == "1111");
        (await _store.ListAsync("b")).Should().ContainSingle(x => x.Last4 == "2222");
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private sealed class TestClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 8, 14, 12, 0, 0, TimeSpan.Zero);
    }
}
