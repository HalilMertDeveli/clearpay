using ClearPay.Application.Ports;

namespace ClearPay.Infrastructure.Realtime;

/// <summary>Tests and boot before Web registers SignalR.</summary>
public sealed class NoOpWalletLiveNotifier : IWalletLiveNotifier
{
    public Task NotifyAsync(WalletLiveNotice notice, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notice);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
