using ClearPay.Application.Transfers;

namespace ClearPay.Application.Ports;

/// <summary>Read a P2P transfer the actor sent or received. No ledger write.</summary>
public interface ITransferLookup
{
    Task<TransferLookupDto?> GetForActorAsync(
        string actorUserId,
        Guid transferId,
        CancellationToken cancellationToken = default);
}
