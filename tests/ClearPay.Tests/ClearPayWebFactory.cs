using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClearPay.Tests;

public sealed class ClearPayWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var identityPath = Path.Combine(Path.GetTempPath(), $"clearpay-identity-{Guid.NewGuid():N}.db");
        var ledgerPath = Path.Combine(Path.GetTempPath(), $"clearpay-ledger-{Guid.NewGuid():N}.db");
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={identityPath}");
        builder.UseSetting("ConnectionStrings:ClearPay", $"Data Source={ledgerPath}");
        builder.UseSetting("ClearPay:UseSqliteLedger", "true");
        builder.UseSetting("ClearPay:ApplyLedgerMigrations", "true");
        builder.UseSetting("ConnectionStrings:Redis", "");
        builder.UseSetting("ConnectionStrings:RabbitMq", "");
        builder.UseSetting("Jwt:SigningKey", "ClearPay-Dev-Jwt-Signing-Key-32b!!");
        builder.UseSetting("Hangfire:Enabled", "false");
    }
}
