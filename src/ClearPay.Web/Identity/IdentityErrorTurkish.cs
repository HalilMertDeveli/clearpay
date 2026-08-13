using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClearPay.Web.Identity;

internal static class IdentityErrorTurkish
{
    public static string Localize(IdentityError error, IStringLocalizer localizer) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => localizer["DuplicateEmail"],
        "PasswordTooShort" => localizer["PasswordTooShort"],
        "PasswordRequiresDigit" => localizer["PasswordRequiresDigit"],
        "PasswordRequiresLower" => localizer["PasswordRequiresLower"],
        "InvalidEmail" => localizer["InvalidEmail"],
        _ => error.Description
    };
}
