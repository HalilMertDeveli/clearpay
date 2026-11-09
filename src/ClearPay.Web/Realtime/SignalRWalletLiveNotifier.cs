using ClearPay.Application.Ports;
using Microsoft.AspNetCore.SignalR;

namespace ClearPay.Web.Realtime;

/// <summary>
/// Push a refresh hint. Does not send amounts. Hub failure must not roll back money.
/// </summary>
public sealed class SignalRWalletLiveNotifier : IWalletLiveNotifier
{
    private readonly IHubContext<WalletHub> _hub;
    private readonly ILogger<SignalRWalletLiveNotifier> _logger;

    public SignalRWalletLiveNotifier(
        IHubContext<WalletHub> hub,
        ILogger<SignalRWalletLiveNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotifyAsync(WalletLiveNotice notice, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notice);
        cancellationToken.ThrowIfCancellationRequested();

        var ids = notice.UserIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (ids.Length == 0)
            return;

        var payload = new
        {
            reason = notice.Reason,
            correlationId = notice.CorrelationId
        };

        try
        {
            foreach (var userId in ids)
            {
                await _hub.Clients
                    .Group(WalletHub.GroupName(userId))
                    .SendAsync(WalletHub.EventName, payload, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Wallet live notify failed; ledger already committed.");
        }
    }
}
