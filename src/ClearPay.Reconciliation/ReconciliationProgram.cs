using System.Globalization;
using ClearPay.Messaging;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ClearPay.Reconciliation;

/// <summary>
/// Small web host: Hangfire server + dashboard (<c>/hangfire</c>), the payment projection consumer and a
/// read/trigger API. Explicit class instead of top-level statements so tests can reference two services.
/// The API has no authentication: it is meant for the internal network only (see docs/RECONCILIATION.md).
/// </summary>
public static class ReconciliationProgram
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.IncludeScopes = true;
            o.TimestampFormat = "HH:mm:ss ";
        });

        var options = AddReconciliation(builder.Services, builder.Configuration, builder.Environment.ContentRootPath);
        AddTracing(builder.Services, builder.Configuration);

        var app = builder.Build();
        await MigrateWithRetryAsync(app.Services, app.Logger);
        app.Services.GetRequiredService<RabbitConnectionProvider>().TryWarmUp();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "ClearPay reconciliation",
            Authorization = options.DashboardOpenToAll
                ? [new OpenDashboardFilter()]
                : [new LocalRequestsOnlyAuthorizationFilter()]
        });
        RegisterRecurringJob(app.Services, options);
        MapApi(app, options);
        await app.RunAsync();
    }

    public static ReconciliationOptions AddReconciliation(IServiceCollection services, IConfiguration configuration, string contentRoot)
    {
        var options = configuration.GetSection(ReconciliationOptions.SectionName).Get<ReconciliationOptions>()
                      ?? new ReconciliationOptions();
        // Binding a list into a non-empty default would append, so the default is applied after binding.
        options.Providers = options.Providers.Count == 0
            ? ReconciliationOptions.DefaultProviders.ToList()
            : options.Providers.Distinct(StringComparer.Ordinal).ToList();
        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);

        var sql = configuration.GetConnectionString("Reconciliation")
                  ?? throw new InvalidOperationException("ConnectionStrings:Reconciliation is required.");
        services.AddDbContext<ReconciliationDbContext>(o => o.UseSqlServer(
            sql,
            s => s.MigrationsHistoryTable("__EFMigrationsHistory", ReconciliationDbContext.Schema)));

        var folder = Path.IsPathRooted(options.SettlementDirectory)
            ? options.SettlementDirectory
            : Path.Combine(contentRoot, options.SettlementDirectory);
        services.AddSingleton(new FolderSettlementFileSource(folder));
        services.AddSingleton<ISettlementFileSource>(sp => sp.GetRequiredService<FolderSettlementFileSource>());
        services.AddSingleton<ILedgerDailyTotals>(new SqlLedgerDailyTotals(configuration.GetConnectionString("LedgerReadOnly")));
        services.AddScoped<ReconciliationService>();
        services.AddScoped<NightlyReconciliationJob>();

        services.AddClearPayRabbit(configuration.GetConnectionString("RabbitMq"), "clearpay-reconciliation");
        services.AddRabbitConsumer<PaymentProjectionHandler>(new ConsumerQueueOptions
        {
            QueueName = PaymentProjectionHandler.QueueName,
            BindingKeys = PaymentProjectionHandler.BindingKeys,
            MaxAttempts = options.MaxAttempts,
            RetryDelay = TimeSpan.FromSeconds(options.RetryDelaySeconds)
        });

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();
            if (options.UseMemoryHangfire)
            {
                config.UseMemoryStorage();
            }
            else
            {
                // Own schema: the web app's Hangfire tables (HangFire.*) and this service's never mix.
                config.UseSqlServerStorage(sql, new SqlServerStorageOptions
                {
                    SchemaName = "ReconHangfire",
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(5),
                    DisableGlobalLocks = true
                });
            }
        });
        services.AddHangfireServer(o => o.ServerName = "clearpay-reconciliation");
        return options;
    }

    private static void RegisterRecurringJob(IServiceProvider services, ReconciliationOptions options)
    {
        services.GetRequiredService<IRecurringJobManager>().AddOrUpdate<NightlyReconciliationJob>(
            NightlyReconciliationJob.RecurringJobId,
            job => job.RunPreviousDayAsync(),
            options.NightlyCron,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById(options.TimeZone) });
    }

    private static void MapApi(WebApplication app, ReconciliationOptions options)
    {
        app.MapGet("/api/health", (RabbitConnectionProvider rabbit) =>
            Results.Ok(new { status = "ok", service = "clearpay-reconciliation", rabbit = rabbit.Status }));

        // Run now (synchronously) for one Istanbul business day; default = yesterday, like the nightly job.
        app.MapPost("/api/reconciliation/runs", async (string? date, ReconciliationService service, TimeProvider time, CancellationToken ct) =>
        {
            DateOnly day;
            if (string.IsNullOrEmpty(date))
                day = BusinessDay.Previous(time.GetUtcNow());
            else if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day))
                return Results.BadRequest(new { error = "date must be yyyy-MM-dd" });

            var report = await service.RunAsync(day, "manual", ct);
            return Results.Ok(ToDto(report, includeLines: true));
        });

        app.MapGet("/api/reconciliation/reports", async (ReconciliationDbContext db, CancellationToken ct) =>
        {
            var reports = await db.Reports.AsNoTracking().ToListAsync(ct);
            // Ordered in memory: SQLite (tests) cannot ORDER BY datetimeoffset; the table is small.
            return Results.Ok(reports.OrderByDescending(r => r.CompletedAt).Take(20).Select(r => ToDto(r, includeLines: false)));
        });

        app.MapGet("/api/reconciliation/reports/{id:guid}", async (Guid id, ReconciliationDbContext db, CancellationToken ct) =>
        {
            var report = await db.Reports.AsNoTracking().Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, ct);
            return report is null ? Results.NotFound() : Results.Ok(ToDto(report, includeLines: true));
        });

        // The CSV the (mock) bank delivered for that day, as the job read it.
        app.MapGet("/api/reconciliation/settlement-files/{provider}/{date}", (string provider, string date, FolderSettlementFileSource files) =>
        {
            if (!options.Providers.Contains(provider, StringComparer.Ordinal)
                || !DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
                return Results.NotFound();
            var path = files.PathFor(provider, day);
            return File.Exists(path) ? Results.Text(File.ReadAllText(path), "text/csv") : Results.NotFound();
        });
    }

    private static object ToDto(ReconciliationReport r, bool includeLines) => new
    {
        r.Id,
        businessDate = r.BusinessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        r.Trigger,
        r.StartedAt,
        r.CompletedAt,
        r.MatchedCount,
        r.OnlyAtBankCount,
        r.OnlyInClearPayCount,
        r.AmountMismatchCount,
        r.LedgerImbalanceCount,
        r.BankNetTotal,
        r.ClearPayNetTotal,
        r.LedgerCheck,
        r.MismatchEventPublishedAt,
        lines = includeLines
            ? r.Lines.Select(l => new { l.Provider, l.Kind, l.Reference, l.Status, l.BankAmount, l.ClearPayAmount, l.Currency })
            : null
    };

    private static void AddTracing(IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["Observability:OtlpEndpoint"];
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(configuration["Observability:ServiceName"] ?? "clearpay-reconciliation"))
            .WithTracing(t =>
            {
                t.AddSource(MessagingTelemetry.SourceName).AddAspNetCoreInstrumentation();
                if (!string.IsNullOrWhiteSpace(endpoint))
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(endpoint));
            });
    }

    private static async Task MigrateWithRetryAsync(IServiceProvider services, ILogger logger)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ReconciliationDbContext>().Database.MigrateAsync();
                logger.LogInformation("recon schema is up to date");
                return;
            }
            catch (Exception ex) when (attempt < 30)
            {
                logger.LogWarning("Database not ready (attempt {Attempt}): {Reason}", attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }

    /// <summary>Local Docker demo only (Reconciliation:DashboardOpenToAll). Production: an authenticated filter.</summary>
    private sealed class OpenDashboardFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}
