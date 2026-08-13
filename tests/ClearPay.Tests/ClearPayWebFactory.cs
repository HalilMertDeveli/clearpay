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
    }
}
