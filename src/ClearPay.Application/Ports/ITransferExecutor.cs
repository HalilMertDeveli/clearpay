using ClearPay.Application.Transfers;

namespace ClearPay.Application.Ports;

/// <summary>
/// SRP/DIP: havale lives here, not in a PageModel. TASK-06 (Payments).
/// Same Idempotency-Key → 409; second debit forbidden.
/// </summary>
public interface ITransferExecutor
{
    Task<TransferOutcome> ExecuteAsync(TransferCommand command, CancellationToken cancellationToken = default);
}
