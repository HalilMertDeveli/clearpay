using ClearPay.Application.Identity;
using ClearPay.Domain.Identity;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClearPay.Web.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IValidator<RegisterRequest> _validator;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IValidator<RegisterRequest> validator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
    }

    [BindProperty]
    public RegisterRequest Input { get; set; } = new();

    public void OnGet() => Input = new RegisterRequest();

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

        var email = Input.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = Input.FullName.Trim()
        };

        var created = await _userManager.CreateAsync(user, Input.Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors)
            {
                ModelState.AddModelError(string.Empty, IdentityErrorTurkish.Localize(error));
            }

            return Page();
        }

        await _userManager.AddToRoleAsync(user, AppRoles.Musteri);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Index");
    }
}
