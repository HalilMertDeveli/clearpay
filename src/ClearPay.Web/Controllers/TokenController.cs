using System.Security.Claims;
using ClearPay.Application.Ports;
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
            return Unauthorized();
        }

        var user = await _users.FindByEmailAsync(request.Email.Trim()).ConfigureAwait(false);
        if (user is null || !await _users.CheckPasswordAsync(user, request.Password).ConfigureAwait(false))
            return Unauthorized();

        var roles = await _users.GetRolesAsync(user).ConfigureAwait(false);
        var token = _tokens.Issue(user.Id, user.Email ?? request.Email.Trim(), roles.ToList());
        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = 28800
        });
    }
}
