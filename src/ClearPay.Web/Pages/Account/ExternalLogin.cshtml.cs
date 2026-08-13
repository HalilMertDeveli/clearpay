using System.Security.Claims;
using ClearPay.Domain.Identity;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IStringLocalizer<SharedResource> localizer)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
        _localizer = localizer;
    }

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet() => RedirectToPage("/Account/Login");

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        if (!SocialLoginConfiguration.IsKnownProvider(provider)
            || !SocialLoginConfiguration.IsConfigured(_configuration, provider))
        {
            var label = string.Equals(provider, SocialLoginConfiguration.Apple, StringComparison.OrdinalIgnoreCase)
                ? "Apple"
                : string.Equals(provider, SocialLoginConfiguration.Google, StringComparison.OrdinalIgnoreCase)
                    ? "Google"
                    : provider;
            ErrorMessage = _localizer["SocialNotConfigured", string.IsNullOrWhiteSpace(label) ? "OAuth" : label];
            return Page();
        }

        var redirectUrl = Url.Page("/Account/ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ErrorMessage = _localizer["SocialLoginFailed"];
            return Page();
        }

        var existing = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (existing.Succeeded)
            return LocalRedirect(SafeReturnUrl(returnUrl));

        var email = info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = _localizer["SocialEmailRequired"];
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            var name = info.Principal.FindFirstValue(ClaimTypes.Name)
                ?? info.Principal.FindFirstValue(ClaimTypes.GivenName)
                ?? email;
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name
            };
            var created = await _userManager.CreateAsync(user);
            if (!created.Succeeded)
            {
                ErrorMessage = created.Errors.FirstOrDefault()?.Description ?? _localizer["SocialLoginFailed"];
                return Page();
            }

            await _userManager.AddToRoleAsync(user, AppRoles.Musteri);
        }

        var linked = await _userManager.AddLoginAsync(user, info);
        if (!linked.Succeeded && linked.Errors.All(e => e.Code != "LoginAlreadyAssociated"))
        {
            ErrorMessage = linked.Errors.FirstOrDefault()?.Description ?? _localizer["SocialLoginFailed"];
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(SafeReturnUrl(returnUrl));
    }

    private string SafeReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return returnUrl;
        return Url.Page("/Index") ?? "/";
    }
}
