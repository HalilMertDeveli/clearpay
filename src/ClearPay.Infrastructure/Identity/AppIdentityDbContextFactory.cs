using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClearPay.Infrastructure.Identity;

/// <summary>dotnet-ef design-time factory. Runtime uses <c>AddClearPayIdentity</c>.</summary>
public sealed class AppIdentityDbContextFactory : IDesignTimeDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=ClearPay;Integrated Security=True;TrustServerCertificate=True;Encrypt=True",
                sql => sql.MigrationsHistoryTable(AppIdentityDbContext.SqlMigrationsHistoryTable))
            .Options;
        return new AppIdentityDbContext(options);
    }
}
