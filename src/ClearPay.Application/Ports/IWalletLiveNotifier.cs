namespace ClearPay.Application.Ports;

/// <summary>
/// T-071: after ledger commit, ping connected clients. Not a second balance.
/// Payload is a refresh hint; money stays in SQL.
/// </summary>
public interface IWalletLiveNotifier
{
    Task NotifyAsync(WalletLiveNotice notice, CancellationToken cancellationToken = default);
}

public sealed record WalletLiveNotice(
    string Reason,
    Guid? CorrelationId,
    IReadOnlyList<string> UserIds);
