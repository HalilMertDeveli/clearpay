using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ClearPay.Infrastructure.Observability;

/// <summary>
/// One place for ClearPay's own telemetry names. OpenTelemetry in Web subscribes to these
/// (<c>AddSource</c> / <c>AddMeter</c>); code here only depends on System.Diagnostics.
/// Tags are low-cardinality and never carry PAN, e-mail, IBAN or amounts per user.
/// </summary>
public static class ClearPayTelemetry
{
    public const string PaymentsSourceName = "ClearPay.Payments";
    public const string MessagingSourceName = "ClearPay.Messaging";
    public const string MeterName = "ClearPay.Payments";

    public static readonly ActivitySource Payments = new(PaymentsSourceName);
    public static readonly ActivitySource Messaging = new(MessagingSourceName);

    private static readonly Meter Meter = new(MeterName);

    /// <summary>Exported to Prometheus as <c>clearpay_payments_total</c>.</summary>
    public static readonly Counter<long> PaymentCount = Meter.CreateCounter<long>(
        "clearpay.payments",
        unit: "{payment}",
        description: "Payment attempts (transfer, top-up, withdraw) by kind and outcome.");

    /// <summary>Exported to Prometheus as <c>clearpay_payments_duration_milliseconds_*</c>.</summary>
    public static readonly Histogram<double> PaymentDuration = Meter.CreateHistogram<double>(
        "clearpay.payments.duration",
        unit: "ms",
        description: "Wall time of a payment use case, including SQL and the fake bank gateway.");

    public static void RecordPayment(string kind, string outcome, double elapsedMs)
    {
        var tags = new TagList
        {
            { "payment.kind", kind },
            { "payment.outcome", outcome }
        };
        PaymentCount.Add(1, tags);
        PaymentDuration.Record(elapsedMs, tags);
    }
}
