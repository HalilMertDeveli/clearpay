using ClearPay.Application.Banking;
using ClearPay.Application.Ports;
using Microsoft.Extensions.Configuration;

namespace ClearPay.Infrastructure.Banking;

/// <summary>Sahte REST BankGateway. Account hint TIMEOUT (or config) → timeout, no ledger.</summary>
public sealed class RestBankGateway : IBankGateway
{
    private readonly IConfiguration _configuration;

    public RestBankGateway(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string StrategyName => "REST";

    public Task<BankGatewayResult> SendAsync(
        BankGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var simulate = _configuration.GetValue("BankGateway:SimulateTimeout", false);
        var hint = request.AccountHint ?? string.Empty;
        if (simulate || hint.Contains("TIMEOUT", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new BankGatewayResult(Succeeded: false, TimedOut: true, Reference: null));
        }

        if (hint.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new BankGatewayResult(Succeeded: false, TimedOut: false, Reference: null));
        }

        var reference = $"REST-{request.CorrelationId:N}";
        return Task.FromResult(new BankGatewayResult(Succeeded: true, TimedOut: false, Reference: reference));
    }
}
