using System.Globalization;

namespace ClearPay.Web.Presentation;

/// <summary>n-tier presentation: format DTOs for Razor. Does not compute ledger net.</summary>
public static class MoneyDisplay
{
    public static string FormatTry(decimal amount)
    {
        var culture = CultureInfo.CurrentUICulture;
        var format = culture.Name switch
        {
            "tr" => CultureInfo.GetCultureInfo("tr-TR"),
            "en" => CultureInfo.GetCultureInfo("en-US"),
            "de" => CultureInfo.GetCultureInfo("de-DE"),
            "fr" => CultureInfo.GetCultureInfo("fr-FR"),
            _ when culture.IsNeutralCulture => CultureInfo.CreateSpecificCulture(culture.TwoLetterISOLanguageName),
            _ => culture
        };

        return amount.ToString("N2", format) + " ₺";
    }
}
