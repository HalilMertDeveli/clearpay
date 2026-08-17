using ClearPay.Application.Identity;
using ClearPay.Domain.Identity;
using ClearPay.Infrastructure.Identity;
using ClearPay.Web.Identity;
using ClearPay.Web.Localization;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IValidator<RegisterRequest> validator,
        IStringLocalizer<SharedResource> localizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
        _localizer = localizer;
    }

    [BindProperty]
    public RegisterRequest Input { get; set; } = new();

    public void OnGet() => Input = new RegisterRequest { AccountKind = AccountKinds.Bireysel };

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

        var phone = TurkishPhone.Normalize(Input.Phone);
        if (phone is null)
        {
            ModelState.AddModelError("Input.Phone", _localizer["PhoneRequired"]);
            return Page();
        }

        var taken = await _userManager.Users.AnyAsync(u => u.PhoneNumber == phone);
        if (taken)
        {
            ModelState.AddModelError("Input.Phone", _localizer["PhoneTaken"]);
            return Page();
        }

        var email = Input.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = Input.FullName.Trim(),
            PhoneNumber = phone,
            AccountKind = AccountKinds.Normalize(Input.AccountKind)
        };

        var created = await _userManager.CreateAsync(user, Input.Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors)
            {
                ModelState.AddModelError(string.Empty, IdentityErrorTurkish.Localize(error, _localizer));
            }

            return Page();
        }

        await _userManager.AddToRoleAsync(user, AppRoles.Musteri);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Index");
    }
}
