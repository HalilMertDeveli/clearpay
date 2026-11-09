using ClearPay.Application.Identity;
using ClearPay.Application.Ports;
using ClearPay.Domain.Identity;
using ClearPay.Infrastructure.Identity;
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
public sealed class RegisterController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IJwtTokenIssuer _tokens;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RegisterController(
        UserManager<ApplicationUser> users,
        IJwtTokenIssuer tokens,
        IValidator<RegisterRequest> validator,
        IStringLocalizer<SharedResource> localizer)
    {
        _users = users;
        _tokens = tokens;
        _validator = validator;
        _localizer = localizer;
    }

    [HttpPost("/api/register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request ??= new RegisterRequest();
        var validation = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var email = request.Email.Trim();
        var phone = TurkishPhone.Normalize(request.Phone);
        if (phone is not null)
        {
            var taken = await _users.Users.AnyAsync(u => u.PhoneNumber == phone, cancellationToken)
                .ConfigureAwait(false);
            if (taken)
            {
                return Problem(
                    title: "Conflict",
                    detail: "Bu telefon başka bir hesapta kayıtlı.",
                    statusCode: StatusCodes.Status409Conflict);
            }
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = phone,
            AccountKind = AccountKinds.Normalize(request.AccountKind)
        };
        var created = await _users.CreateAsync(user, request.Password).ConfigureAwait(false);
        if (!created.Succeeded)
        {
            var detail = string.Join(" ", created.Errors.Select(e => IdentityErrorTurkish.Localize(e, _localizer)));
            var duplicate = created.Errors.Any(e =>
                e.Code is "DuplicateEmail" or "DuplicateUserName");
            return Problem(
                title: duplicate ? "Conflict" : "Registration failed",
                detail: detail,
                statusCode: duplicate ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest);
        }

        await _users.AddToRoleAsync(user, AppRoles.Musteri).ConfigureAwait(false);
        var roles = await _users.GetRolesAsync(user).ConfigureAwait(false);
        var token = _tokens.Issue(user.Id, email, roles.ToList(), user.AccountKind);
        return Created("/api/wallet", new
        {
            access_token = token,
            token_type = "Bearer",
            expires_in = 28800
        });
    }
}
