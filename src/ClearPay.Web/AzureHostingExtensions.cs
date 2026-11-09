using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

namespace ClearPay.Web;

/// <summary>
/// T-095: App Service sits behind a proxy. Persist DataProtection keys under HOME (Linux /home is durable).
/// </summary>
internal static class AzureHostingExtensions
{
    public static IServiceCollection AddClearPayAzureHosting(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        if (!environment.IsProduction())
            return services;

        var home = Environment.GetEnvironmentVariable("HOME");
        var keysPath = string.IsNullOrWhiteSpace(home)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "data-protection-keys")
            : Path.Combine(home, "data-protection-keys");
        Directory.CreateDirectory(keysPath);
        services.AddDataProtection()
            .SetApplicationName("ClearPay")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        return services;
    }

    public static WebApplication UseClearPayForwardedHeaders(this WebApplication app)
    {
        app.UseForwardedHeaders();
        return app;
    }
}
