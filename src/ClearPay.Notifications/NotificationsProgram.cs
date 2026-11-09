using ClearPay.Messaging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ClearPay.Notifications;

/// <summary>
/// Worker host. An explicit class (not top-level statements) so the test project can reference this
/// service and the reconciliation service without two global <c>Program</c> types colliding.
/// </summary>
public static class NotificationsProgram
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.IncludeScopes = true;
            o.TimestampFormat = "HH:mm:ss ";
        });

        AddNotifications(builder.Services, builder.Configuration);
        AddTracing(builder.Services, builder.Configuration);

        var host = builder.Build();
        await MigrateWithRetryAsync(host.Services, host.Services.GetRequiredService<ILogger<NotificationsDbContext>>());
        await host.RunAsync();
    }

    public static IServiceCollection AddNotifications(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(NotificationsOptions.SectionName).Get<NotificationsOptions>()
                      ?? new NotificationsOptions();
        services.AddSingleton(options);
        services.AddDbContext<NotificationsDbContext>(o => o.UseSqlServer(
            configuration.GetConnectionString("Notifications")
            ?? throw new InvalidOperationException("ConnectionStrings:Notifications is required."),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", NotificationsDbContext.Schema)));
        services.AddSingleton<INotificationSender, LoggingNotificationSender>();

        services.AddClearPayRabbit(configuration.GetConnectionString("RabbitMq"), "clearpay-notifications");
        services.AddRabbitConsumer<EventNotificationHandler>(new ConsumerQueueOptions
        {
            QueueName = EventNotificationHandler.QueueName,
            BindingKeys = EventNotificationHandler.BindingKeys,
            MaxAttempts = options.MaxAttempts,
            RetryDelay = TimeSpan.FromSeconds(options.RetryDelaySeconds)
        });
        return services;
    }

    private static void AddTracing(IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["Observability:OtlpEndpoint"];
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(configuration["Observability:ServiceName"] ?? "clearpay-notifications"))
            .WithTracing(t =>
            {
                // Consumer spans continue the trace id from the traceparent header the web app published.
                t.AddSource(MessagingTelemetry.SourceName);
                if (!string.IsNullOrWhiteSpace(endpoint))
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(endpoint));
            });
    }

    /// <summary>SQL Server in Compose needs ~15 s to accept logins; try for a minute before giving up.</summary>
    private static async Task MigrateWithRetryAsync(IServiceProvider services, ILogger logger)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<NotificationsDbContext>().Database.MigrateAsync();
                logger.LogInformation("notif schema is up to date");
                return;
            }
            catch (Exception ex) when (attempt < 30)
            {
                logger.LogWarning("Database not ready (attempt {Attempt}): {Reason}", attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
