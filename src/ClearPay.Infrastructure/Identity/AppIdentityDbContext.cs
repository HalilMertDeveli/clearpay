using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Identity;

public sealed class AppIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public const string SqlMigrationsHistoryTable = "__EFMigrationsHistoryIdentity";

    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName).HasMaxLength(200);
        });
    }
}
