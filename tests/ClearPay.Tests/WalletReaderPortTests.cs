using ClearPay.Application.Ports;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ClearPay.Tests;

public sealed class WalletReaderPortTests : IClassFixture<ClearPayWebFactory>
{
    private readonly ClearPayWebFactory _factory;

    public WalletReaderPortTests(ClearPayWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Registered_wallet_reader_returns_empty_summary_without_sql()
    {
        using var scope = _factory.Services.CreateScope();
        var reader = scope.ServiceProvider.GetRequiredService<IWalletReader>();

        var summary = await reader.GetByUserIdAsync("task-03-user");

        summary.Should().NotBeNull();
        summary!.Balance.Should().Be(0m);
        summary.MonthOutgoing.Should().Be(0m);
        summary.MonthIncoming.Should().Be(0m);
        summary.IsFrozen.Should().BeFalse();
        summary.LastMovements.Should().BeEmpty();
        summary.UserId.Should().Be("task-03-user");
    }
}
