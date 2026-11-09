using ClearPay.Application.Ports;
using ClearPay.Application.Transfers;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

public sealed class SqlTransferLookup : ITransferLookup
{
    private readonly ClearPayDbContext _db;

    public SqlTransferLookup(ClearPayDbContext db)
    {
        _db = db;
    }

    public async Task<TransferLookupDto?> GetForActorAsync(
        string actorUserId,
        Guid transferId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (transferId == Guid.Empty || string.IsNullOrWhiteSpace(actorUserId))
            return null;

        var transfer = await _db.Transfers.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transferId, cancellationToken)
            .ConfigureAwait(false);
        if (transfer is null)
            return null;

        var actorWallet = await _db.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == actorUserId, cancellationToken)
            .ConfigureAwait(false);
        if (actorWallet is null)
            return null;

        if (transfer.FromWalletId != actorWallet.Id && transfer.ToWalletId != actorWallet.Id)
            return null;

        return new TransferLookupDto(
            transfer.Id,
            transfer.CorrelationId,
            transfer.Amount,
            transfer.Description,
            transfer.Status.ToString(),
            transfer.CreatedAt,
            transfer.FromWalletId,
            transfer.ToWalletId);
    }
}
