using Microsoft.Extensions.Configuration;

namespace ClearPay.Infrastructure.Identity;

/// <summary>
/// Reads Google/Apple OAuth from user-secrets (<c>Authentication:*</c>) or env
/// (<c>Google__ClientId</c> / <c>Apple__*</c>). Empty = not configured; never log values.
/// </summary>
public static class SocialLoginConfiguration
{
    public const string Google = "Google";
    public const string Apple = "Apple";

    public static string? Read(IConfiguration configuration, params string[] keys)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        foreach (var key in keys)
        {
            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    public static bool IsGoogleConfigured(IConfiguration configuration) =>
        Read(configuration, "Authentication:Google:ClientId", "Google:ClientId") is not null
        && Read(configuration, "Authentication:Google:ClientSecret", "Google:ClientSecret") is not null;

    public static bool IsAppleConfigured(IConfiguration configuration) =>
        Read(configuration, "Authentication:Apple:ClientId", "Apple:ClientId") is not null
        && Read(configuration, "Authentication:Apple:TeamId", "Apple:TeamId") is not null
        && Read(configuration, "Authentication:Apple:KeyId", "Apple:KeyId") is not null
        && Read(configuration, "Authentication:Apple:PrivateKey", "Apple:PrivateKey") is not null;

    public static bool IsKnownProvider(string? provider) =>
        string.Equals(provider, Google, StringComparison.OrdinalIgnoreCase)
        || string.Equals(provider, Apple, StringComparison.OrdinalIgnoreCase);

    public static bool IsConfigured(IConfiguration configuration, string? provider)
    {
        if (string.Equals(provider, Google, StringComparison.OrdinalIgnoreCase))
            return IsGoogleConfigured(configuration);
        if (string.Equals(provider, Apple, StringComparison.OrdinalIgnoreCase))
            return IsAppleConfigured(configuration);
        return false;
    }

    public static string NormalizePem(string raw)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(raw);
        var key = raw.Replace("\\n", "\n", StringComparison.Ordinal).Trim();
        if (!key.Contains("BEGIN", StringComparison.Ordinal))
            key = "-----BEGIN PRIVATE KEY-----\n" + key + "\n-----END PRIVATE KEY-----";
        return key;
    }
}
