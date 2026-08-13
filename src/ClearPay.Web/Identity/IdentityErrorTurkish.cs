using Microsoft.AspNetCore.Identity;

namespace ClearPay.Web.Identity;

internal static class IdentityErrorTurkish
{
    public static string Localize(IdentityError error) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Bu e-posta zaten kayıtlı.",
        "PasswordTooShort" => "Şifre en az 8 karakter olmalıdır.",
        "PasswordRequiresDigit" => "Şifre en az bir rakam içermelidir.",
        "PasswordRequiresLower" => "Şifre en az bir küçük harf içermelidir.",
        "InvalidEmail" => "Geçerli bir e-posta girin.",
        _ => error.Description
    };
}
