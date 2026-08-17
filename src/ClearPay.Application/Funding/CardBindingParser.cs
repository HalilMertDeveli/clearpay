using ClearPay.Domain.Ledger;

namespace ClearPay.Application.Funding;

/// <summary>
/// Turns a typed demo PAN (or last4) into persistable fields. Never returns the full number.
/// </summary>
public static class CardBindingParser
{
    public static bool TryParse(string? panOrLast4, string? label, out string last4, out string scheme, out string nickname)
    {
        last4 = string.Empty;
        scheme = CardNetwork.Unknown;
        nickname = string.Empty;

        var digits = new string((panOrLast4 ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == LedgerSchema.LinkedLast4Length)
        {
            last4 = digits;
        }
        else if (digits.Length is >= 13 and <= 19)
        {
            last4 = digits[^LedgerSchema.LinkedLast4Length..];
            scheme = CardNetwork.Detect(digits);
        }
        else
        {
            return false;
        }

        nickname = string.IsNullOrWhiteSpace(label)
            ? "ClearPay Demo"
            : label.Trim();
        if (nickname.Length > LedgerSchema.LinkedLabelMaxLength)
            nickname = nickname[..LedgerSchema.LinkedLabelMaxLength];
        return true;
    }
}
