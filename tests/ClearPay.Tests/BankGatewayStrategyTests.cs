using ClearPay.Application.Banking;
using ClearPay.Infrastructure.Banking;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ClearPay.Tests;

public sealed class BankGatewayStrategyTests
{
    [Theory]
    [InlineData(typeof(RestBankGateway), "REST", "REST-")]
    [InlineData(typeof(SoapBankGateway), "SOAP", "SOAP-")]
    public async Task Success_and_timeout_share_the_same_result_shape(
        Type gatewayType,
        string strategyName,
        string referencePrefix)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var gateway = (ClearPay.Application.Ports.IBankGateway)Activator.CreateInstance(gatewayType, config)!;
        gateway.StrategyName.Should().Be(strategyName);

        var correlation = Guid.NewGuid();
        var ok = await gateway.SendAsync(
            new BankGatewayRequest(BankOperation.TopUp, 10m, "TR00", correlation));
        ok.Succeeded.Should().BeTrue();
        ok.TimedOut.Should().BeFalse();
        ok.Reference.Should().StartWith(referencePrefix);

        var timeout = await gateway.SendAsync(
            new BankGatewayRequest(BankOperation.Withdraw, 10m, "TIMEOUT", correlation));
        timeout.Succeeded.Should().BeFalse();
        timeout.TimedOut.Should().BeTrue();
        timeout.Reference.Should().BeNull();
    }
}
