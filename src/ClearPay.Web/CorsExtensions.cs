namespace ClearPay.Web;

/// <summary>
/// T-061: browser/Flutter-web only. Native Flutter does not send Origin.
/// Production: Cors:Origins (T-104 live host). Development: localhost / 127.0.0.1 / 10.0.2.2.
/// </summary>
public static class CorsExtensions
{
    public const string PolicyName = "ClearPayClients";

    public static IServiceCollection AddClearPayCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var live = (configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
            .Where(static origin => !string.IsNullOrWhiteSpace(origin))
            .ToArray();
        if (live.Length == 0)
            live = ["https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net"];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                if (environment.IsDevelopment())
                {
                    policy.SetIsOriginAllowed(static origin =>
                    {
                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            return false;
                        return uri.Host is "localhost" or "127.0.0.1" or "10.0.2.2";
                    });
                    return;
                }

                policy.WithOrigins(live);
            });
        });
        return services;
    }
}
