using ClearPay.Application.Ports;
using ClearPay.Domain.Identity;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClearPay.Web.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class TokenController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IJwtTokenIssuer _tokens;

    public TokenController(UserManager<ApplicationUser> users, IJwtTokenIssuer tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    [HttpPost("/api/token")]
    public async Task<IActionResult> Post([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request is null
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            // #region agent log
            AgentDebugLog.Write("F", "TokenController.cs:Post", "token rejected", new { status = 401, reason = "empty", hasKind = AccountKinds.IsKnown(request?.AccountKind) });
            // #endregion
            return Unauthorized();
        }

        var user = await _users.FindByEmailAsync(request.Email.Trim()).ConfigureAwait(false);
        if (user is null || !await _users.CheckPasswordAsync(user, request.Password).ConfigureAwait(false))
        {
            // #region agent log
            AgentDebugLog.Write("F", "TokenController.cs:Post", "token rejected", new { status = 401, reason = "bad_credentials", hasKind = AccountKinds.IsKnown(request.AccountKind) });
            // #endregion
            return Unauthorized();
        }

        if (AccountKinds.IsKnown(request.AccountKind))
        {
            user.AccountKind = AccountKinds.Normalize(request.AccountKind);
            await _users.UpdateAsync(user).ConfigureAwait(false);
        }

        var roles = await _users.GetRolesAsync(user).ConfigureAwait(false);
        var token = _tokens.Issue(
            user.Id,
            user.Email ?? request.Email.Trim(),
            roles.ToList(),
            user.AccountKind);
        // #region agent log
        AgentDebugLog.Write("F", "TokenController.cs:Post", "token issued", new { status = 200, hasKind = AccountKinds.IsKnown(user.AccountKind), remote = HttpContext.Connection.RemoteIpAddress?.AddressFamily.ToString() });
        // #endregion
        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = 28800
        });
    }
}
