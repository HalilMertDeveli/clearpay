using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// TASK-06: one SQL transaction — debit, credit, Transfer, idempotency, audit, outbox.
/// Duplicate Key → 409. No UPDATE Balance. Havale API not started here.
/// </summary>
public sealed class NotImplementedTransferExecutor : ITransferExecutor
{
    public Task<TransferOutcome> ExecuteAsync(TransferCommand command, CancellationToken cancellationToken = default)
    {
        _ = command;
        _ = cancellationToken;
        throw new NotImplementedException("TASK-06: ITransferExecutor — Payments; 409 + LedgerPair; not PageModel.");
    }
}
