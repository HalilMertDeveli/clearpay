using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClearPay.Web.Realtime;

/// <summary>
/// T-071 chrome. Cookie (site) or JWT query <c>access_token</c> (Flutter). Not a 9th screen.
/// </summary>
[Authorize(AuthenticationSchemes = "Identity.Application,Bearer")]
public sealed class WalletHub : Hub
{
    public const string Path = "/hubs/wallet";
    public const string EventName = "WalletChanged";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        // #region agent log
        AgentDebugLog.Write("D", "WalletHub.cs:OnConnectedAsync", "hub connected", new
        {
            hasUserId = !string.IsNullOrWhiteSpace(userId),
            hasQueryToken = Context.GetHttpContext()?.Request.Query.ContainsKey("access_token") == true,
            path = Context.GetHttpContext()?.Request.Path.Value
        });
        // #endregion
        if (!string.IsNullOrWhiteSpace(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(userId)).ConfigureAwait(false);

        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public static string GroupName(string userId) => "user:" + userId;
}
