namespace ClearPay.Application.Identity;

/// <summary>Demo TR mobile digits. Not KYC / Mernis.</summary>
public static class TurkishPhone
{
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal))
            digits = digits[2..];

        if (digits.Length == 12 && digits.StartsWith("90", StringComparison.Ordinal) && digits[2] == '5')
            return digits;

        if (digits.Length == 11 && digits.StartsWith("05", StringComparison.Ordinal))
            return "90" + digits[1..];

        if (digits.Length == 10 && digits.StartsWith("5", StringComparison.Ordinal))
            return "90" + digits;

        return null;
    }

    public static bool IsValid(string? raw) => Normalize(raw) is not null;
}
