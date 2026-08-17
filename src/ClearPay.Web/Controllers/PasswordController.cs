using ClearPay.Application.Identity;
using ClearPay.Application.Ports;
using ClearPay.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Web.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class PasswordController : ControllerBase
{
    private static readonly object ForgotOk = new
    {
        ok = true,
        message = "Varsa e-posta kuyruğa alındı."
    };

    private readonly UserManager<ApplicationUser> _users;
    private readonly IAccountMailer _mailer;
    private readonly IValidator<ForgotPasswordRequest> _forgotValidator;
    private readonly IValidator<ResetPasswordRequest> _resetValidator;
    private readonly IHostEnvironment _environment;

    public PasswordController(
        UserManager<ApplicationUser> users,
        IAccountMailer mailer,
        IValidator<ForgotPasswordRequest> forgotValidator,
        IValidator<ResetPasswordRequest> resetValidator,
        IHostEnvironment environment)
    {
        _users = users;
        _mailer = mailer;
        _forgotValidator = forgotValidator;
        _resetValidator = resetValidator;
        _environment = environment;
    }

    [HttpPost("/api/password/forgot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Forgot([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        request ??= new ForgotPasswordRequest();
        var validation = await _forgotValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var user = await FindUserAsync(request, cancellationToken).ConfigureAwait(false);
        if (user is not null && !string.IsNullOrWhiteSpace(user.Email))
        {
            var token = await _users.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            var body = _environment.IsDevelopment()
                ? $"Development reset token (paste in Flutter; not shown in Production UI): {token}"
                : "A password reset was requested for this ClearPay demo account.";
            await _mailer.SendAsync(user.Email, "ClearPay şifre sıfırlama", body, cancellationToken)
                .ConfigureAwait(false);
        }

        return Ok(ForgotOk);
    }

    [HttpPost("/api/password/reset")]
    public async Task<IActionResult> Reset([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        request ??= new ResetPasswordRequest();
        var validation = await _resetValidator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var user = await _users.FindByEmailAsync(request.Email.Trim()).ConfigureAwait(false);
        if (user is null)
        {
            return Problem(
                title: "Reset failed",
                detail: "Sıfırlama kodu geçersiz.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await _users.ResetPasswordAsync(user, request.Token, request.NewPassword).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return Problem(
                title: "Reset failed",
                detail: "Sıfırlama kodu geçersiz.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(new { ok = true, message = "Şifre güncellendi." });
    }

    private async Task<ApplicationUser?> FindUserAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
            return await _users.FindByEmailAsync(request.Email.Trim()).ConfigureAwait(false);

        var phone = TurkishPhone.Normalize(request.Phone);
        if (phone is null)
            return null;

        return await _users.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken)
            .ConfigureAwait(false);
    }
}
