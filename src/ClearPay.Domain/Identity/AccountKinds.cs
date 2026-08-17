namespace ClearPay.Domain.Identity;

/// <summary>Demo UI mode on the same 8 operations. Not a merchant POS.</summary>
public static class AccountKinds
{
    public const string Bireysel = "Bireysel";
    public const string Kurumsal = "Kurumsal";
    public const string JwtClaim = "account_kind";

    public static bool IsKnown(string? value) =>
        string.Equals(value, Bireysel, StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, Kurumsal, StringComparison.OrdinalIgnoreCase);

    public static string Normalize(string? value) =>
        string.Equals(value, Kurumsal, StringComparison.OrdinalIgnoreCase) ? Kurumsal : Bireysel;
}
