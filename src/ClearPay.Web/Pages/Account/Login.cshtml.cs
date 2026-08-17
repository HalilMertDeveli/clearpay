using ClearPay.Application.Identity;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Localization;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IValidator<LoginRequest> _validator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        IValidator<LoginRequest> validator,
        IStringLocalizer<SharedResource> localizer)
    {
        _signInManager = signInManager;
        _validator = validator;
        _localizer = localizer;
    }

    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet() => Input = new LoginRequest();

    public async Task<IActionResult> OnPostAsync()
    {
        var validation = await _validator.ValidateAsync(Input);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError($"Input.{error.PropertyName}", error.ErrorMessage);
            }

            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email.Trim(),
            Input.Password,
            isPersistent: Input.RememberMe,
            lockoutOnFailure: false);

        // #region agent log
        AgentDebugLog.Write("K", "Login.cshtml.cs:OnPostAsync", "cookie login", new { ok = result.Succeeded, locked = result.IsLockedOut });
        // #endregion

        if (result.Succeeded)
        {
            return LocalRedirect(SafeReturnUrl());
        }

        ModelState.AddModelError(string.Empty, _localizer["InvalidCredentials"]);
        return Page();
    }

    private string SafeReturnUrl()
    {
        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return ReturnUrl;
        }

        return Url.Page("/Index") ?? "/";
    }
}
