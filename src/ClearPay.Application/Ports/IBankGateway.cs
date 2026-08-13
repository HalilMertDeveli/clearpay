using ClearPay.Application.Banking;

namespace ClearPay.Application.Ports;

/// <summary>
/// OCP: new bank channel = new strategy, not a switch in Web.
/// Q1 default REST (TASK-07); SOAP (TASK-08) implements the same contract.
/// </summary>
public interface IBankGateway
{
    string StrategyName { get; }

    Task<BankGatewayResult> SendAsync(BankGatewayRequest request, CancellationToken cancellationToken = default);
}
