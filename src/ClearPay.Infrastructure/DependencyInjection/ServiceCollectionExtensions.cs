using ClearPay.Application.Ports;
using ClearPay.Infrastructure.Banking;
using ClearPay.Infrastructure.Persistence;
using ClearPay.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IWalletReader, EmptyWalletReader>();
        services.AddScoped<ITransferExecutor, NotImplementedTransferExecutor>();
        services.AddScoped<IIdempotencyStore, NotImplementedIdempotencyStore>();

        services.AddScoped<RestBankGateway>();
        services.AddScoped<SoapBankGateway>();
        // Q1 default REST. TASK-08: bind IBankGateway to SoapBankGateway (or config switch).
        services.AddScoped<IBankGateway>(sp => sp.GetRequiredService<RestBankGateway>());

        return services;
    }
}
