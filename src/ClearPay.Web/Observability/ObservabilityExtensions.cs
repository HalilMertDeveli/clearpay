using System.Security.Claims;
using ClearPay.Application.Ports;
using ClearPay.Infrastructure.Observability;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace ClearPay.Web.Observability;

/// <summary>
/// All logging / tracing / metrics wiring in one place so Program.cs only gets two lines:
/// <c>builder.AddClearPayObservability()</c> and <c>app.UseClearPayObservability()</c>.
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Serilog (console + optional Seq), OpenTelemetry traces and metrics (optional OTLP), and the
    /// payment span decorators. Call after <c>AddClearPay</c> so the executors exist to be decorated.
    /// </summary>
    public static WebApplicationBuilder AddClearPayObservability(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var options = builder.Configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
                      ?? new ObservabilityOptions();
        builder.Services.AddSingleton(options);

        builder.Host.UseSerilog(
            (context, _, logger) => ConfigureSerilog(logger, context.Configuration, options, context.HostingEnvironment.EnvironmentName),
            preserveStaticLogger: true);

        var version = typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString() ?? "0.0.0";
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(options.ServiceName, serviceVersion: version)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ClearPayTelemetry.PaymentsSourceName, ClearPayTelemetry.MessagingSourceName)
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.RecordException = true;
                        o.Filter = ctx => !IsNoise(ctx.Request.Path);
                    })
                    .AddHttpClientInstrumentation(o => o.RecordException = true)
                    // EF Core SQL Server goes through Microsoft.Data.SqlClient; this sees every command.
                    // The instrumentation records sanitised query text; EF parameterises values anyway,
                    // so amounts and e-mails do not end up on the span.
                    .AddSqlClientInstrumentation(o => o.RecordException = true);
                if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                    tracing.AddOtlpExporter(o => ConfigureOtlp(o, options.OtlpEndpoint));
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(ClearPayTelemetry.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                    metrics.AddOtlpExporter((o, reader) =>
                    {
                        ConfigureOtlp(o, options.OtlpEndpoint);
                        reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10_000;
                    });
            });

        builder.Services.Decorate<ITransferExecutor>(inner => new TracedTransferExecutor(inner));
        builder.Services.Decorate<IFundingExecutor>(inner => new TracedFundingExecutor(inner));
        return builder;
    }

    /// <summary>Correlation header + one structured log line per request (instead of ASP.NET's chatty defaults).</summary>
    public static WebApplication UseClearPayObservability(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging(o =>
        {
            o.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms";
            o.GetLevel = (ctx, _, ex) =>
                ex is not null || ctx.Response.StatusCode >= 500 ? LogEventLevel.Error
                : IsNoise(ctx.Request.Path) ? LogEventLevel.Verbose
                : LogEventLevel.Information;
            o.EnrichDiagnosticContext = (diagnostic, ctx) =>
            {
                // Opaque Identity id only. No e-mail, name, TC kimlik or IP address.
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                    diagnostic.Set("UserId", userId);
                var merchantId = ctx.User.FindFirstValue("merchant_id");
                if (!string.IsNullOrEmpty(merchantId))
                    diagnostic.Set("MerchantId", merchantId);
                diagnostic.Set("Endpoint", ctx.GetEndpoint()?.DisplayName ?? "(none)");
            };
        });
        return app;
    }

    internal static void ConfigureSerilog(
        LoggerConfiguration logger,
        IConfiguration configuration,
        ObservabilityOptions options,
        string environment)
    {
        logger
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
            .ReadFrom.Configuration(configuration) // optional "Serilog" section overrides the levels above
            .Enrich.FromLogContext()
            .Enrich.With<TraceEnricher>()
            .Enrich.With<SensitiveDataEnricher>()
            .Enrich.WithProperty("Application", options.ServiceName)
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {{TraceId={TraceId} CorrelationId={CorrelationId}}}{NewLine}{Exception}");

        if (!string.IsNullOrWhiteSpace(options.SeqUrl))
            logger.WriteTo.Seq(options.SeqUrl);
    }

    private static void ConfigureOtlp(OtlpExporterOptions o, string endpoint)
    {
        o.Endpoint = new Uri(endpoint);
        o.Protocol = OtlpExportProtocol.Grpc;
    }

    private static bool IsNoise(PathString path) =>
        path.StartsWithSegments("/api/health")
        || path.StartsWithSegments("/hangfire")
        || path.StartsWithSegments("/css")
        || path.StartsWithSegments("/js")
        || path.StartsWithSegments("/lib")
        || path.StartsWithSegments("/img")
        || path.StartsWithSegments("/favicon.ico");

    /// <summary>Minimal decorator helper (no Scrutor): wraps the last registration of <typeparamref name="TService"/>.</summary>
    internal static IServiceCollection Decorate<TService>(
        this IServiceCollection services,
        Func<TService, TService> decorate)
        where TService : class
    {
        var index = -1;
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType == typeof(TService))
            {
                index = i;
                break;
            }
        }

        if (index < 0)
            throw new InvalidOperationException(
                $"{typeof(TService).Name} is not registered; call AddClearPay before AddClearPayObservability.");

        var original = services[index];
        services[index] = ServiceDescriptor.Describe(
            typeof(TService),
            sp => decorate((TService)CreateOriginal(sp, original)),
            original.Lifetime);
        return services;
    }

    private static object CreateOriginal(IServiceProvider sp, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance is not null)
            return descriptor.ImplementationInstance;
        if (descriptor.ImplementationFactory is not null)
            return descriptor.ImplementationFactory(sp);
        return ActivatorUtilities.GetServiceOrCreateInstance(sp, descriptor.ImplementationType!);
    }
}
