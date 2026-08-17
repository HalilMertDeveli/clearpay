using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ClearPay.Application.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ClearPay.Infrastructure.Identity;

public sealed class FirebaseIdTokenVerifier : IFirebaseIdTokenVerifier
{
    public const string LoginProvider = "Firebase";

    private const string GoogleCertsUrl =
        "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com";

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _http;
    private readonly object _gate = new();
    private IReadOnlyList<SecurityKey>? _keys;
    private DateTimeOffset _keysUntil;

    public FirebaseIdTokenVerifier(IConfiguration configuration, IHttpClientFactory http)
    {
        _configuration = configuration;
        _http = http;
    }

    public bool IsConfigured =>
        _configuration.GetValue("Firebase:AllowTestTokens", false)
        || !string.IsNullOrWhiteSpace(_configuration["Firebase:ProjectId"]);

    public async Task<FirebasePrincipal?> VerifyAsync(string idToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(idToken))
            return null;

        if (_configuration.GetValue("Firebase:AllowTestTokens", false))
        {
            return idToken.StartsWith("test.", StringComparison.Ordinal)
                ? ParseTestToken(idToken)
                : null;
        }

        var projectId = _configuration["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId))
            return null;

        var keys = await GetSigningKeysAsync(cancellationToken).ConfigureAwait(false);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(idToken, parameters, out _);
            var uid = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst("user_id")?.Value;
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                ?? principal.FindFirst("email")?.Value;
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(email))
                return null;

            return new FirebasePrincipal(uid, email.Trim());
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    public static string CreateTestToken(string uid, string email)
    {
        var json = JsonSerializer.Serialize(new { sub = uid, email });
        return "test." + Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static FirebasePrincipal? ParseTestToken(string idToken)
    {
        var payload = idToken["test.".Length..].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2:
                payload += "==";
                break;
            case 3:
                payload += "=";
                break;
        }

        try
        {
            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = doc.RootElement;
            var uid = root.GetProperty("sub").GetString();
            var email = root.GetProperty("email").GetString();
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(email))
                return null;
            return new FirebasePrincipal(uid, email.Trim());
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_keys is not null && _keysUntil > DateTimeOffset.UtcNow)
                return _keys;
        }

        var client = _http.CreateClient(nameof(FirebaseIdTokenVerifier));
        using var response = await client.GetAsync(GoogleCertsUrl, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        using var doc = JsonDocument.Parse(json);
        var keys = new List<SecurityKey>();
        foreach (var property in doc.RootElement.EnumerateObject())
        {
            var pem = property.Value.GetString();
            if (string.IsNullOrWhiteSpace(pem))
                continue;
            keys.Add(new X509SecurityKey(X509Certificate2.CreateFromPem(pem)));
        }

        var until = DateTimeOffset.UtcNow.AddHours(1);
        if (response.Headers.CacheControl?.MaxAge is { } max && max > TimeSpan.Zero)
            until = DateTimeOffset.UtcNow.Add(max);

        lock (_gate)
        {
            _keys = keys;
            _keysUntil = until;
        }

        return keys;
    }
}
