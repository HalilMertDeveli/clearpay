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
    public async Task Registered_wallet_reader_is_sql_and_returns_zero_when_ledger_unreachable()
    {
        using var scope = _factory.Services.CreateScope();
        var reader = scope.ServiceProvider.GetRequiredService<IWalletReader>();
        reader.Should().BeOfType<ClearPay.Infrastructure.Persistence.SqlWalletReader>();

        scope.ServiceProvider.GetRequiredService<ClearPay.Infrastructure.Persistence.ClearPayDbContext>()
            .Should().NotBeNull();

        var summary = await reader.GetByUserIdAsync("task-05-user");

        summary.Should().NotBeNull();
        summary!.Balance.Should().Be(0m);
        summary.MonthOutgoing.Should().Be(0m);
        summary.MonthIncoming.Should().Be(0m);
        summary.LastMovements.Should().BeEmpty();
        summary.UserId.Should().Be("task-05-user");
    }
}
