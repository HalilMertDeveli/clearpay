using ClearPay.Application.Identity;
using ClearPay.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IValidator<LoginRequest> _validator;

    public LoginModel(SignInManager<ApplicationUser> signInManager, IValidator<LoginRequest> validator)
    {
        _signInManager = signInManager;
        _validator = validator;
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
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return LocalRedirect(SafeReturnUrl());
        }

        ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
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
