using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClearPay.Tests;

public sealed class ClearPayWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"clearpay-identity-{Guid.NewGuid():N}.db");
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={dbPath}");
        builder.UseSetting(
            "ConnectionStrings:ClearPay",
            "Server=127.0.0.1,1433;Database=ClearPay_Tests;User Id=sa;Password=unused;TrustServerCertificate=True;Encrypt=True;Connect Timeout=2");
        builder.UseSetting("ClearPay:ApplyLedgerMigrations", "false");
    }
}
