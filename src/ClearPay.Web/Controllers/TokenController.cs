using ClearPay.Application.Identity;
using ClearPay.Application.Ports;
using ClearPay.Domain.Identity;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Api;
using ClearPay.Web.Identity;
using ClearPay.Web.Localization;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class TokenController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IJwtTokenIssuer _tokens;
    private readonly IFirebaseIdTokenVerifier _firebase;
    private readonly IValidator<FirebaseTokenRequest> _firebaseValidator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public TokenController(
        UserManager<ApplicationUser> users,
        IJwtTokenIssuer tokens,
        IFirebaseIdTokenVerifier firebase,
        IValidator<FirebaseTokenRequest> firebaseValidator,
        IStringLocalizer<SharedResource> localizer)
    {
        _users = users;
        _tokens = tokens;
        _firebase = firebase;
        _firebaseValidator = firebaseValidator;
        _localizer = localizer;
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

        var identifier = request.Email.Trim();
        var email = DemoTc.ResolveEmail(identifier) ?? identifier;
        var user = await _users.FindByEmailAsync(email).ConfigureAwait(false);
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
            user.Email ?? email,
            roles.ToList(),
            user.AccountKind);
        // #region agent log
        AgentDebugLog.Write("F", "TokenController.cs:Post", "token issued", new { status = 200, hasKind = AccountKinds.IsKnown(user.AccountKind), ua = TruncUa(Request.Headers.UserAgent.ToString()) });
        // #endregion
        return Ok(new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = 28800
        });
    }

    [HttpPost("/api/token/firebase")]
    public async Task<IActionResult> PostFirebase(
        [FromBody] FirebaseTokenRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request ??= new FirebaseTokenRequest();
        if (!_firebase.IsConfigured)
        {
            return Problem(
                title: "Firebase yapılandırılmadı",
                detail: "Firebase yapılandırılmadı",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var validation = await _firebaseValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var principal = await _firebase.VerifyAsync(request.IdToken, cancellationToken).ConfigureAwait(false);
        if (principal is null)
            return Unauthorized();

        var email = principal.Email.Trim();
        var user = await _users.FindByLoginAsync(FirebaseIdTokenVerifier.LoginProvider, principal.Uid)
            .ConfigureAwait(false);
        user ??= await _users.FindByEmailAsync(email).ConfigureAwait(false);

        var phone = TurkishPhone.Normalize(request.Phone);
        if (user is null)
        {
            if (phone is null)
            {
                return Problem(
                    title: "Registration failed",
                    detail: "Telefon zorunludur.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (await PhoneTakenAsync(phone, exceptUserId: null, cancellationToken).ConfigureAwait(false))
            {
                return Problem(
                    title: "Conflict",
                    detail: "Bu telefon başka bir hesapta kayıtlı.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            var name = string.IsNullOrWhiteSpace(request.FullName) ? email : request.FullName.Trim();
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                PhoneNumber = phone,
                AccountKind = AccountKinds.Normalize(request.AccountKind)
            };
            var created = await _users.CreateAsync(user).ConfigureAwait(false);
            if (!created.Succeeded)
            {
                var detail = string.Join(" ", created.Errors.Select(e => IdentityErrorTurkish.Localize(e, _localizer)));
                var duplicate = created.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName");
                return Problem(
                    title: duplicate ? "Conflict" : "Registration failed",
                    detail: detail,
                    statusCode: duplicate ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest);
            }

            await _users.AddToRoleAsync(user, AppRoles.Musteri).ConfigureAwait(false);
            await _users.AddLoginAsync(
                    user,
                    new UserLoginInfo(FirebaseIdTokenVerifier.LoginProvider, principal.Uid, "Firebase"))
                .ConfigureAwait(false);
        }
        else
        {
            var login = await _users.FindByLoginAsync(FirebaseIdTokenVerifier.LoginProvider, principal.Uid)
                .ConfigureAwait(false);
            if (login is null)
            {
                await _users.AddLoginAsync(
                        user,
                        new UserLoginInfo(FirebaseIdTokenVerifier.LoginProvider, principal.Uid, "Firebase"))
                    .ConfigureAwait(false);
            }

            var changed = false;
            if (AccountKinds.IsKnown(request.AccountKind))
            {
                user.AccountKind = AccountKinds.Normalize(request.AccountKind);
                changed = true;
            }

            if (phone is not null && !string.Equals(user.PhoneNumber, phone, StringComparison.Ordinal))
            {
                if (await PhoneTakenAsync(phone, user.Id, cancellationToken).ConfigureAwait(false))
                {
                    return Problem(
                        title: "Conflict",
                        detail: "Bu telefon başka bir hesapta kayıtlı.",
                        statusCode: StatusCodes.Status409Conflict);
                }

                user.PhoneNumber = phone;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(request.FullName) && string.IsNullOrWhiteSpace(user.FullName))
            {
                user.FullName = request.FullName.Trim();
                changed = true;
            }

            if (changed)
                await _users.UpdateAsync(user).ConfigureAwait(false);
        }

        var roles = await _users.GetRolesAsync(user).ConfigureAwait(false);
        var jwt = _tokens.Issue(user.Id, user.Email ?? email, roles.ToList(), user.AccountKind);
        return Ok(new
        {
            access_token = jwt,
            token_type = "Bearer",
            expires_in = 28800
        });
    }

    private async Task<bool> PhoneTakenAsync(string phone, string? exceptUserId, CancellationToken cancellationToken)
    {
        return await _users.Users.AnyAsync(
                u => u.PhoneNumber == phone && (exceptUserId == null || u.Id != exceptUserId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static string TruncUa(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "none";
        return raw.Length <= 80 ? raw : raw[..80];
    }
}
