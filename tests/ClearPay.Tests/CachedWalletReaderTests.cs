using ClearPay.Application.Ports;
using ClearPay.Application.Wallets;
using ClearPay.Infrastructure.Caching;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class CachedWalletReaderTests
{
    [Fact]
    public async Task Second_read_uses_cache_not_sql()
    {
        var inner = new StubWalletReader(Summary("user-1", Guid.NewGuid(), 12m));
        var cache = new MemoryWalletSummaryCache();
        var sut = new CachedWalletReader(inner, cache);

        var first = await sut.GetByUserIdAsync("user-1");
        var second = await sut.GetByUserIdAsync("user-1");

        first!.Balance.Should().Be(12m);
        second!.Balance.Should().Be(12m);
        inner.Calls.Should().Be(1);
    }

    [Fact]
    public async Task Sql_down_empty_summary_is_not_cached()
    {
        var inner = new StubWalletReader(Summary("user-2", Guid.Empty, 0m));
        var cache = new MemoryWalletSummaryCache();
        var sut = new CachedWalletReader(inner, cache);

        await sut.GetByUserIdAsync("user-2");
        await sut.GetByUserIdAsync("user-2");

        inner.Calls.Should().Be(2);
        (await cache.GetAsync("user-2")).Should().BeNull();
    }

    [Fact]
    public async Task Invalidate_forces_sql_again()
    {
        var inner = new StubWalletReader(Summary("user-3", Guid.NewGuid(), 5m));
        var cache = new MemoryWalletSummaryCache();
        var sut = new CachedWalletReader(inner, cache);

        await sut.GetByUserIdAsync("user-3");
        await cache.InvalidateAsync("user-3");
        await sut.GetByUserIdAsync("user-3");

        inner.Calls.Should().Be(2);
    }

    [Fact]
    public async Task Redis_key_prefix_is_stable_for_task06_invalidate()
    {
        RedisWalletSummaryCache.Key("abc").Should().Be("clearpay:wallet-summary:abc");
    }

    private static WalletSummary Summary(string userId, Guid walletId, decimal balance) => new(
        walletId,
        userId,
        balance,
        0m,
        0m,
        false,
        Array.Empty<WalletMovement>());

    private sealed class StubWalletReader : IWalletReader
    {
        private readonly WalletSummary _summary;

        public StubWalletReader(WalletSummary summary) => _summary = summary;

        public int Calls { get; private set; }

        public Task<WalletSummary?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult<WalletSummary?>(_summary);
        }
    }

    private sealed class MemoryWalletSummaryCache : IWalletSummaryCache
    {
        private readonly Dictionary<string, WalletSummary> _store = new(StringComparer.Ordinal);

        public Task<WalletSummary?> GetAsync(string userId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(userId, out var value);
            return Task.FromResult(value);
        }

        public Task SetAsync(WalletSummary summary, CancellationToken cancellationToken = default)
        {
            _store[summary.UserId] = summary;
            return Task.CompletedTask;
        }

        public Task InvalidateAsync(string userId, CancellationToken cancellationToken = default)
        {
            _store.Remove(userId);
            return Task.CompletedTask;
        }
    }
}
