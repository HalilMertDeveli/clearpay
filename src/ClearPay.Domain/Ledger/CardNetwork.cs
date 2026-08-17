namespace ClearPay.Domain.Ledger;

/// <summary>
/// Card scheme from BIN prefixes. Banks (Yapı Kredi, etc.) are nicknames, not schemes.
/// </summary>
public static class CardNetwork
{
    public const string Visa = "Visa";

    public const string Mastercard = "Mastercard";

    public const string Troy = "Troy";

    public const string Unknown = "Unknown";

    public static string Detect(string digits)
    {
        if (string.IsNullOrEmpty(digits))
            return Unknown;
        if (digits.StartsWith("9792", StringComparison.Ordinal))
            return Troy;
        if (digits[0] == '4')
            return Visa;
        if (IsMastercard(digits))
            return Mastercard;
        return Unknown;
    }

    private static bool IsMastercard(string digits)
    {
        if (digits.Length >= 2
            && int.TryParse(digits.AsSpan(0, 2), out var two)
            && two is >= 51 and <= 55)
        {
            return true;
        }

        return digits.Length >= 4
            && int.TryParse(digits.AsSpan(0, 4), out var four)
            && four is >= 2221 and <= 2720;
    }
}
