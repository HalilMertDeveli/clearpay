using ClearPay.Application.Funding;

namespace ClearPay.Application.Ports;

/// <summary>DIP: top-up/withdraw live here, not in a PageModel. Timeout must not post ledger.</summary>
public interface IFundingExecutor
{
    Task<FundingOutcome> ExecuteAsync(FundingCommand command, CancellationToken cancellationToken = default);
}
