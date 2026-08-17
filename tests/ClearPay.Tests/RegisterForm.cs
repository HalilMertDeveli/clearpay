namespace ClearPay.Tests;

internal static class RegisterForm
{
    public static Dictionary<string, string> Cookie(
        string antiforgery,
        string email,
        string name,
        string? phone = null,
        string accountKind = "Bireysel")
    {
        return new Dictionary<string, string>
        {
            ["Input.FullName"] = name,
            ["Input.Email"] = email,
            ["Input.Phone"] = phone ?? UniquePhone(),
            ["Input.Password"] = "Deneme123",
            ["Input.ConfirmPassword"] = "Deneme123",
            ["Input.AccountKind"] = accountKind,
            ["__RequestVerificationToken"] = antiforgery
        };
    }

    public static string UniquePhone()
    {
        var n = Random.Shared.Next(100_000_000, 999_999_999);
        return "5" + n;
    }
}
