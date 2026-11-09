using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ClearPay.Application.Payments;

/// <summary>Config: <c>Payments:ThreeDs</c>. Empty secret → a random per-process key (mock ACS runs in the same host).</summary>
public sealed class ThreeDsOptions
{
    public const string SectionName = "Payments:ThreeDs";

    public string AcsSecret { get; set; } = string.Empty;
}

/// <summary>
/// HMAC-SHA256 over <c>paymentId|threeDsTransactionId|result|amount</c>.
/// The ACS signs, the gateway verifies with a constant-time compare. Binding the amount means a
/// tampered or replayed result for another payment fails verification.
/// </summary>
public sealed class ThreeDsSigner
{
    private readonly byte[] _key;

    public ThreeDsSigner(ThreeDsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _key = string.IsNullOrWhiteSpace(options.AcsSecret)
            ? RandomNumberGenerator.GetBytes(32)
            : Encoding.UTF8.GetBytes(options.AcsSecret);
    }

    public static string Canonical(Guid paymentId, string threeDsTransactionId, string result, decimal amount) =>
        string.Join(
            '|',
            paymentId.ToString("N"),
            threeDsTransactionId,
            result,
            amount.ToString("F2", CultureInfo.InvariantCulture));

    public string Sign(Guid paymentId, string threeDsTransactionId, string result, decimal amount)
    {
        var data = Encoding.UTF8.GetBytes(Canonical(paymentId, threeDsTransactionId, result, amount));
        return Convert.ToHexString(HMACSHA256.HashData(_key, data));
    }

    public bool Verify(Guid paymentId, string threeDsTransactionId, string result, decimal amount, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
            return false;

        byte[] given;
        try
        {
            given = Convert.FromHexString(signature.Trim());
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = Convert.FromHexString(Sign(paymentId, threeDsTransactionId, result, amount));
        return CryptographicOperations.FixedTimeEquals(given, expected);
    }
}

/// <summary>ACS authentication results (EMV 3DS uses the same letters): Y = authenticated, N = not.</summary>
public static class ThreeDsResult
{
    public const string Authenticated = "Y";

    public const string NotAuthenticated = "N";
}
