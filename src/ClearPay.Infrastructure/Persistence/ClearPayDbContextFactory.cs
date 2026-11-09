using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>dotnet-ef design-time factory. Runtime uses <c>AddClearPay</c> + appsettings.</summary>
public sealed class ClearPayDbContextFactory : IDesignTimeDbContextFactory<ClearPayDbContext>
{
    public ClearPayDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=ClearPay;Integrated Security=True;TrustServerCertificate=True;Encrypt=True")
            .Options;
        return new ClearPayDbContext(options);
    }
}
