namespace ClearPay.Web.Observability;

/// <summary>
/// <c>Observability</c> section. Empty URLs switch the sink/exporter off, so tests and a laptop
/// without Docker run with console logs only.
/// </summary>
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>OpenTelemetry <c>service.name</c>; shows up in Tempo, Seq and Prometheus labels.</summary>
    public string ServiceName { get; set; } = "clearpay-web";

    /// <summary>Seq ingestion URL for Serilog, e.g. <c>http://localhost:5341</c>.</summary>
    public string? SeqUrl { get; set; }

    /// <summary>OTLP gRPC endpoint (traces + metrics), e.g. <c>http://localhost:4317</c> (OpenTelemetry Collector).</summary>
    public string? OtlpEndpoint { get; set; }
}
