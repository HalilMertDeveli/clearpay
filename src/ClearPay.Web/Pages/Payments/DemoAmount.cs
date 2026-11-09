using System.Globalization;

namespace ClearPay.Web.Pages.Payments;

/// <summary>Demo form amount: accepts "249.90" or "249,90" regardless of UI culture. Unparseable → 0 (gateway rejects).</summary>
internal static class DemoAmount
{
    public static decimal Parse(string? raw)
    {
        var text = (raw ?? string.Empty).Trim().Replace(',', '.');
        return decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0m;
    }
}
