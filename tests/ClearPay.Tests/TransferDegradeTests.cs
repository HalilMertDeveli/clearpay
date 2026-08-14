using System.Diagnostics;
using ClearPay.Infrastructure.Caching;
using ClearPay.Infrastructure.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;

namespace ClearPay.Tests;

public sealed class TransferDegradeTests
{
    [Fact]
    public void MapClearPayHangfire_without_JobStorage_does_not_throw()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hangfire:Enabled"] = "true"
            })
            .Build();

        var act = () => ServiceCollectionExtensions.MapClearPayHangfire(services, config);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task Redis_get_when_disconnected_returns_null_quickly()
    {
        var options = ConfigurationOptions.Parse("127.0.0.1:1");
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 200;
        options.AsyncTimeout = 200;
        options.SyncTimeout = 200;
        options.ConnectRetry = 0;
        using var mux = ConnectionMultiplexer.Connect(options);
        var cache = new RedisWalletSummaryCache(mux, NullLogger<RedisWalletSummaryCache>.Instance);

        var sw = Stopwatch.StartNew();
        var result = await cache.GetAsync("user-hang");
        sw.Stop();

        result.Should().BeNull();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        mux.IsConnected.Should().BeFalse();
    }
}
