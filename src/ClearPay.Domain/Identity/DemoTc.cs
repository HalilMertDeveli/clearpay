namespace ClearPay.Domain.Identity;

/// <summary>Demo TCKN → Identity e-posta. Mernis / KYC yok.</summary>
public static class DemoTc
{
    public const string AdminNationalId = "10000000146";
    public const string AdminEmail = "admin@clearpay.test";

    public static string DigitsOnly(string? raw) =>
        raw is null ? string.Empty : new string(raw.Where(char.IsDigit).ToArray());

    public static string? ResolveEmail(string? raw)
    {
        var digits = DigitsOnly(raw);
        return digits == AdminNationalId ? AdminEmail : null;
    }
}
