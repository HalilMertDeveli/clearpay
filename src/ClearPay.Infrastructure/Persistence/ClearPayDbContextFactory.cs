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
                "Server=localhost,1433;Database=ClearPay;User Id=sa;Password=ClearPay_Dev1!;TrustServerCertificate=True;Encrypt=True")
            .Options;
        return new ClearPayDbContext(options);
    }
}
