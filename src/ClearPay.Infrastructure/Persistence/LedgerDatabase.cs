using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// Applies ledger migrations when SQL Server is up. Identity SQLite is separate.
/// Unreachable SQL must not take down the cookie site (TASK-03).
/// Tests set <c>ClearPay:ApplyLedgerMigrations=false</c> (T-023).
/// </summary>
public static class LedgerDatabase
{
    public static async Task EnsureMigratedAsync(IServiceProvider services, ILogger logger)
    {
        var configuration = services.GetService<IConfiguration>();
        if (configuration?.GetValue("ClearPay:ApplyLedgerMigrations", true) == false)
        {
            return;
        }

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClearPayDbContext>();
        try
        {
            if (configuration?.GetValue("ClearPay:UseSqliteLedger", false) == true)
            {
                await db.Database.EnsureCreatedAsync();
                return;
            }

            var connection = db.Database.GetConnectionString();
            if (!string.IsNullOrWhiteSpace(connection))
            {
                var builder = new SqlConnectionStringBuilder(connection) { ConnectTimeout = 3 };
                db.Database.SetConnectionString(builder.ConnectionString);
            }

            if (!await db.Database.CanConnectAsync())
            {
                logger.LogWarning(
                    "ClearPay SQL Server is not reachable; ledger migrate skipped. Identity site continues. Start: docker compose up -d sql");
                return;
            }

            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Ledger SQL migrate skipped; Identity site continues. ConnectionStrings:ClearPay must point at Docker SQL.");
        }
    }
}
