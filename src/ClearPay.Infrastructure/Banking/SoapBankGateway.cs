using ClearPay.Application.Banking;
using ClearPay.Application.Ports;
using Microsoft.Extensions.Configuration;

namespace ClearPay.Infrastructure.Banking;

/// <summary>OCP SOAP strategy. Same timeout/fail/success contract as REST. No WCF/real bank.</summary>
public sealed class SoapBankGateway : IBankGateway
{
    private readonly IConfiguration _configuration;

    public SoapBankGateway(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string StrategyName => "SOAP";

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

        var reference = $"SOAP-{request.CorrelationId:N}";
        return Task.FromResult(new BankGatewayResult(Succeeded: true, TimedOut: false, Reference: reference));
    }
}
