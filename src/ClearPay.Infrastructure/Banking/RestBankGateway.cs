using ClearPay.Application.Banking;
using ClearPay.Application.Ports;

namespace ClearPay.Infrastructure.Banking;

/// <summary>OCP strategy. TASK-07: sahte REST; timeout must not commit ledger.</summary>
public sealed class RestBankGateway : IBankGateway
{
    public string StrategyName => "REST";

    public Task<BankGatewayResult> SendAsync(BankGatewayRequest request, CancellationToken cancellationToken = default)
    {
        _ = request;
        _ = cancellationToken;
        throw new NotImplementedException("TASK-07: RestBankGateway — sahte REST BankGateway.");
    }
}
