using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace ClearPay.Web.Localization;

public static class LocalizationExtensions
{
    public static IServiceCollection AddClearPayLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = AppCultures.Codes.Select(c => new CultureInfo(c)).ToList();
            options.DefaultRequestCulture = new RequestCulture(AppCultures.Default, AppCultures.Default);
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders =
            [
                new CookieRequestCultureProvider()
            ];
        });
        return services;
    }
}
