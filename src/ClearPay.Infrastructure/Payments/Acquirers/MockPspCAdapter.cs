using System.Text.Json;
using ClearPay.Application.Payments;

namespace ClearPay.Infrastructure.Payments.Acquirers;

/// <summary>
/// Mock PSP C: a modern JSON API style answer such as
/// <c>{"status":"failed","decline_code":"insufficient_funds","id":null}</c>.
/// String decline codes instead of numbers; the adapter maps them to the same normalized enum.
/// </summary>
public sealed class MockPspCAdapter : IAcquirerAdapter
{
    public const string ProviderName = "MockPspC";

    public string Name => ProviderName;

    public Task<AcquirerResult> AuthorizeAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var decline = MockScenarios.ForAuthorization(request.Amount, request.CardLast4) switch
        {
            MockScenario.InsufficientFunds => "insufficient_funds",
            MockScenario.Decline => "generic_decline",
            MockScenario.InvalidCard => "incorrect_number",
            MockScenario.IssuerDown => "issuer_not_available",
            _ => null
        };
        return Task.FromResult(Normalize(Respond(decline, "ch")));
    }

    public Task<AcquirerResult> CaptureAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Normalize(Respond(null, "cap")));
    }

    public Task<AcquirerResult> VoidAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Normalize(Respond(null, "void")));
    }

    public Task<AcquirerResult> RefundAsync(
        AcquirerRequest request,
        decimal refundAmount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var decline = MockScenarios.ForRefund(refundAmount) == MockScenario.Decline ? "generic_decline" : null;
        return Task.FromResult(Normalize(Respond(decline, "re")));
    }

    /// <summary>JSON body → normalized code.</summary>
    public static AcquirerResult Normalize(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var status = root.TryGetProperty("status", out var s) ? s.GetString() : null;
            if (status == "succeeded")
            {
                var id = root.TryGetProperty("id", out var i) ? i.GetString() : null;
                return AcquirerResult.Approved(status, id ?? string.Empty);
            }

            var declineCode = root.TryGetProperty("decline_code", out var d) ? d.GetString() ?? "none" : "none";
            var normalized = declineCode switch
            {
                "insufficient_funds" => AcquirerResponseCode.InsufficientFunds,
                "generic_decline" or "do_not_honor" => AcquirerResponseCode.Declined,
                "incorrect_number" or "invalid_number" => AcquirerResponseCode.InvalidCard,
                "issuer_not_available" => AcquirerResponseCode.IssuerUnavailable,
                _ => AcquirerResponseCode.Unknown
            };
            return AcquirerResult.Rejected(normalized, declineCode, $"PSP C declined: {declineCode}");
        }
        catch (JsonException)
        {
            return AcquirerResult.Rejected(AcquirerResponseCode.Unknown, "JSON", "Unreadable PSP response");
        }
    }

    private static string Respond(string? declineCode, string idPrefix) =>
        JsonSerializer.Serialize(new Dictionary<string, string?>
        {
            ["status"] = declineCode is null ? "succeeded" : "failed",
            ["decline_code"] = declineCode,
            ["id"] = declineCode is null ? $"{idPrefix}_{Guid.NewGuid():N}"[..(idPrefix.Length + 17)] : null
        });
}
