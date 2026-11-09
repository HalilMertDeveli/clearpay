using System.Xml.Linq;
using ClearPay.Application.Payments;

namespace ClearPay.Infrastructure.Payments.Acquirers;

/// <summary>
/// Mock bank B: a SOAP/XML style answer such as
/// <c>&lt;PosResponse&gt;&lt;Result&gt;Error&lt;/Result&gt;&lt;ErrorCode&gt;1051&lt;/ErrorCode&gt;...</c>.
/// Numeric error codes in the 1000 range; 0 means success. The adapter parses the XML and normalizes it.
/// </summary>
public sealed class MockBankBAdapter : IAcquirerAdapter
{
    public const string ProviderName = "MockBankB";

    public string Name => ProviderName;

    public Task<AcquirerResult> AuthorizeAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var errorCode = MockScenarios.ForAuthorization(request.Amount, request.CardLast4) switch
        {
            MockScenario.InsufficientFunds => 1051,
            MockScenario.Decline => 1005,
            MockScenario.InvalidCard => 1014,
            MockScenario.IssuerDown => 9999,
            _ => 0
        };
        return Task.FromResult(Normalize(Respond(errorCode, "AUTH", request.PaymentId)));
    }

    public Task<AcquirerResult> CaptureAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Normalize(Respond(0, "POSTAUTH", request.PaymentId)));
    }

    public Task<AcquirerResult> VoidAsync(AcquirerRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Normalize(Respond(0, "VOID", request.PaymentId)));
    }

    public Task<AcquirerResult> RefundAsync(
        AcquirerRequest request,
        decimal refundAmount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var errorCode = MockScenarios.ForRefund(refundAmount) == MockScenario.Decline ? 1005 : 0;
        return Task.FromResult(Normalize(Respond(errorCode, "CREDIT", Guid.NewGuid())));
    }

    /// <summary>XML body → normalized code. Unparseable XML is <see cref="AcquirerResponseCode.Unknown"/>, never "approved".</summary>
    public static AcquirerResult Normalize(string xml)
    {
        XElement root;
        try
        {
            root = XElement.Parse(xml);
        }
        catch (System.Xml.XmlException)
        {
            return AcquirerResult.Rejected(AcquirerResponseCode.Unknown, "XML", "Unreadable bank response");
        }

        var result = (string?)root.Element("Result") ?? string.Empty;
        var code = (string?)root.Element("ErrorCode") ?? string.Empty;
        var message = (string?)root.Element("ErrorMessage");
        if (string.Equals(result, "Approved", StringComparison.OrdinalIgnoreCase) && code == "0")
            return AcquirerResult.Approved(code, (string?)root.Element("HostRefNum") ?? string.Empty);

        var normalized = code switch
        {
            "1051" => AcquirerResponseCode.InsufficientFunds,
            "1005" => AcquirerResponseCode.Declined,
            "1014" => AcquirerResponseCode.InvalidCard,
            "9999" => AcquirerResponseCode.IssuerUnavailable,
            _ => AcquirerResponseCode.Unknown
        };
        return AcquirerResult.Rejected(normalized, code, message);
    }

    private static string Respond(int errorCode, string transactionType, Guid id)
    {
        var approved = errorCode == 0;
        return new XElement(
            "PosResponse",
            new XElement("TransactionType", transactionType),
            new XElement("Result", approved ? "Approved" : "Error"),
            new XElement("ErrorCode", errorCode),
            new XElement("ErrorMessage", approved ? string.Empty : $"Bank B error {errorCode}"),
            new XElement("HostRefNum", approved ? $"B{id:N}"[..13] : string.Empty)).ToString();
    }
}
