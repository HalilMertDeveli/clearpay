using ClearPay.Application.Banking;
using ClearPay.Application.Ports;

namespace ClearPay.Infrastructure.Banking;

/// <summary>OCP strategy. TASK-08: same IBankGateway contract as REST.</summary>
public sealed class SoapBankGateway : IBankGateway
{
    public string StrategyName => "SOAP";

    public Task<BankGatewayResult> SendAsync(BankGatewayRequest request, CancellationToken cancellationToken = default)
    {
        _ = request;
        _ = cancellationToken;
        throw new NotImplementedException("TASK-08: SoapBankGateway — aynı sözleşme, SOAP strategy.");
    }
}
