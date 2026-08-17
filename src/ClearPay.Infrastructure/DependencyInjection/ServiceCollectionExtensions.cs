using ClearPay.Application.Ports;
using ClearPay.Infrastructure.Banking;
using ClearPay.Infrastructure.Caching;
using ClearPay.Infrastructure.Identity;
using ClearPay.Infrastructure.Documents;
using ClearPay.Infrastructure.Jobs;
using ClearPay.Infrastructure.Messaging;
using ClearPay.Infrastructure.Persistence;
using ClearPay.Infrastructure.Realtime;
using ClearPay.Infrastructure.Time;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace ClearPay.Infrastructure.DependencyInjection;

/// <summary>
/// Composition root helper. Web Program.cs should call <see cref="AddClearPay"/>.
/// PageModels inject Application ports only — not these concrete types.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClearPay(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<global::ClearPay.Infrastructure.SqlOptions>(
            configuration.GetSection(global::ClearPay.Infrastructure.SqlOptions.SectionName));

        var ledgerConnection = configuration.GetConnectionString("ClearPay")
            ?? throw new InvalidOperationException("ConnectionStrings:ClearPay is required for ledger SQL Server.");
        var useSqliteLedger = configuration.GetValue("ClearPay:UseSqliteLedger", false);
        if (useSqliteLedger)
        {
            services.AddDbContext<ClearPayDbContext>(options => options.UseSqlite(ledgerConnection));
        }
        else
        {
            services.AddDbContext<ClearPayDbContext>(options =>
                options.UseSqlServer(ledgerConnection, sql => sql.CommandTimeout(8)));
        }

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IReceiptPdf, SimplePdfReceiptRenderer>();
        services.AddScoped<IWalletLiveNotifier, NoOpWalletLiveNotifier>();
        AddRedisCache(services, configuration);
        AddRabbit(services, configuration);
        services.AddScoped<SqlWalletReader>();
        services.AddScoped<IWalletReader>(sp => new CachedWalletReader(
            sp.GetRequiredService<SqlWalletReader>(),
            sp.GetRequiredService<IWalletSummaryCache>()));
        services.AddScoped<IIdempotencyStore, SqlIdempotencyStore>();
        services.AddScoped<ITransferExecutor, SqlTransferExecutor>();
        services.AddScoped<ITransferLookup, SqlTransferLookup>();
        services.AddScoped<IFundingExecutor, SqlFundingExecutor>();
        services.AddScoped<ILinkedInstrumentStore, SqlLinkedInstrumentStore>();
        services.AddScoped<IActivityReader, SqlActivityReader>();
        services.AddScoped<IAdminPanel, SqlAdminPanel>();
        services.AddScoped<IOutboxProcessor, SqlOutboxProcessor>();
        services.AddScoped<OutboxHangfireJob>();

        services.AddScoped<RestBankGateway>();
        services.AddScoped<SoapBankGateway>();
        var strategy = configuration["BankGateway:Strategy"] ?? "REST";
        if (string.Equals(strategy, "SOAP", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IBankGateway>(sp => sp.GetRequiredService<SoapBankGateway>());
        }
        else
        {
            services.AddScoped<IBankGateway>(sp => sp.GetRequiredService<RestBankGateway>());
        }

        return services;
    }

    public static IServiceCollection AddClearPayHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (!configuration.GetValue("Hangfire:Enabled", true))
            return services;

        var useMemory = configuration.GetValue("ClearPay:UseSqliteLedger", false)
            || configuration.GetValue("Hangfire:UseMemoryStorage", false);
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();
            if (useMemory)
            {
                config.UseMemoryStorage();
            }
            else
            {
                var sql = configuration.GetConnectionString("ClearPay")
                    ?? throw new InvalidOperationException("ConnectionStrings:ClearPay is required for Hangfire SQL.");
                config.UseSqlServerStorage(sql, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromSeconds(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
            }
        });
        services.AddHangfireServer();
        return services;
    }

    public static void MapClearPayHangfire(this IServiceProvider services, IConfiguration configuration)
    {
        if (!configuration.GetValue("Hangfire:Enabled", true))
            return;

        try
        {
            var manager = services.GetService<IRecurringJobManager>();
            if (manager is null)
                return;

            manager.AddOrUpdate<OutboxHangfireJob>(
                "clearpay-outbox",
                job => job.Run(),
                Cron.Minutely);
        }
        catch (Exception)
        {
            // Boot must not die; Pending outbox stays in SQL until the next successful start.
        }
    }

    private static void AddRedisCache(IServiceCollection services, IConfiguration configuration)
    {
        var mux = RedisMultiplexerFactory.TryCreate(configuration.GetConnectionString("Redis"));
        services.AddSingleton(new RedisRuntimeStatus(mux));
        if (mux is null)
        {
            services.AddSingleton<IWalletSummaryCache, NoOpWalletSummaryCache>();
            return;
        }

        services.AddSingleton<IConnectionMultiplexer>(mux);
        services.AddSingleton<IWalletSummaryCache, RedisWalletSummaryCache>();
    }

    private static void AddRabbit(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<LoggingOutboxPublisher>();
        var connection = RabbitConnectionFactory.TryCreate(configuration.GetConnectionString("RabbitMq"));
        if (connection is null)
        {
            var empty = configuration.GetConnectionString("RabbitMq");
            services.AddSingleton(new RabbitRuntimeStatus(string.IsNullOrWhiteSpace(empty) ? "off" : "down"));
            services.AddScoped<IOutboxPublisher>(sp => sp.GetRequiredService<LoggingOutboxPublisher>());
            return;
        }

        services.AddSingleton(connection);
        services.AddSingleton(new RabbitRuntimeStatus("up"));
        services.AddSingleton<IOutboxPublisher>(sp => new RabbitOutboxPublisher(
            sp.GetRequiredService<IConnection>(),
            sp.GetRequiredService<ILogger<RabbitOutboxPublisher>>(),
            sp.GetRequiredService<LoggingOutboxPublisher>()));
    }
}
