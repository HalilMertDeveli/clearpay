using System.Globalization;

namespace ClearPay.Web.Presentation;

/// <summary>n-tier presentation: format DTOs for Razor. Does not compute ledger net.</summary>
public static class MoneyDisplay
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string FormatTry(decimal amount) => amount.ToString("N2", Turkish) + " ₺";
}
