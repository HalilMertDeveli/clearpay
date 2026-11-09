using ClearPay.Application.Payments;

namespace ClearPay.Infrastructure.Payments.Acquirers;

/// <summary>
/// Mock bank A: answers with ISO 8583 style two-character response codes
/// ("00" approved, "51" insufficient funds, "05" do not honor, "14" invalid card, "91" issuer down).
/// No network call — this is a stub that only shows normalization.
/// </summary>
public sealed class MockBankAAdapter : IAcquirerAdapter
{
    public const string ProviderName = "MockBankA";

    public string Name => ProviderName;

    public Task<AcquirerResult> AuthorizeAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var raw = MockScenarios.ForAuthorization(request.Amount, request.CardLast4) switch
        {
            MockScenario.InsufficientFunds => "51",
            MockScenario.Decline => "05",
            MockScenario.InvalidCard => "14",
            MockScenario.IssuerDown => "91",
            _ => "00"
        };
        return Task.FromResult(Normalize(raw, $"A-AUTH-{request.PaymentId:N}"[..20]));
    }

    public Task<AcquirerResult> CaptureAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Normalize("00", $"A-CAP-{request.PaymentId:N}"[..19]));
    }

    public Task<AcquirerResult> VoidAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Normalize("00", $"A-VOID-{request.PaymentId:N}"[..20]));
    }

    public Task<AcquirerResult> RefundAsync(
        AcquirerRequest request,
        decimal refundAmount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var raw = MockScenarios.ForRefund(refundAmount) == MockScenario.Decline ? "05" : "00";
        return Task.FromResult(Normalize(raw, $"A-REF-{Guid.NewGuid():N}"[..19]));
    }

    /// <summary>ISO 8583 response code → normalized code.</summary>
    public static AcquirerResult Normalize(string responseCode, string reference) => responseCode switch
    {
        "00" => AcquirerResult.Approved(responseCode, reference),
        "51" => AcquirerResult.Rejected(AcquirerResponseCode.InsufficientFunds, responseCode, "Insufficient funds"),
        "05" => AcquirerResult.Rejected(AcquirerResponseCode.Declined, responseCode, "Do not honor"),
        "14" => AcquirerResult.Rejected(AcquirerResponseCode.InvalidCard, responseCode, "Invalid card number"),
        "91" => AcquirerResult.Rejected(AcquirerResponseCode.IssuerUnavailable, responseCode, "Issuer unavailable"),
        _ => AcquirerResult.Rejected(AcquirerResponseCode.Unknown, responseCode, "Unmapped response code")
    };
}
