using System.Globalization;

namespace ClearPay.Web.Localization;

public static class AppCultures
{
    public const string Default = "tr";

    public static readonly string[] Codes = ["tr", "en", "de", "fr"];

    public static bool IsSupported(string? culture) =>
        Codes.Contains(culture, StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string? culture) =>
        IsSupported(culture) ? culture!.ToLowerInvariant() : Default;

    public static string HtmlLang =>
        Normalize(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

    public static string OgLocale => HtmlLang switch
    {
        "en" => "en_US",
        "de" => "de_DE",
        "fr" => "fr_FR",
        _ => "tr_TR"
    };
}
