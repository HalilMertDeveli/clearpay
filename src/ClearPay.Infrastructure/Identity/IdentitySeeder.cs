using ClearPay.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClearPay.Infrastructure.Identity;

public static class IdentitySeeder
{
    public const string DevelopmentAdminEmail = "admin@clearpay.test";

    /// <summary>Local 10 digits; stored as 905550000001. Demo recovery, not SMS OTP.</summary>
    public const string DevelopmentAdminPhoneLocal = "5550000001";

    public const string DevelopmentAdminPhone = "905550000001";

    public static async Task EnsureCreatedAndRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<AppIdentityDbContext>();
        if (db.Database.IsSqlite())
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.MigrateAsync();

        var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync(AppRoles.Musteri))
            await roles.CreateAsync(new IdentityRole(AppRoles.Musteri));
        if (!await roles.RoleExistsAsync(AppRoles.Admin))
            await roles.CreateAsync(new IdentityRole(AppRoles.Admin));

        var environment = provider.GetService<IHostEnvironment>();
        if (environment is null || !environment.IsDevelopment())
            return;

        var users = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(DevelopmentAdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DevelopmentAdminEmail,
                Email = DevelopmentAdminEmail,
                FullName = "ClearPay Admin",
                AccountKind = AccountKinds.Bireysel
            };
            var created = await users.CreateAsync(admin, "Deneme123");
            if (!created.Succeeded)
                return;
        }

        if (!await users.IsInRoleAsync(admin, AppRoles.Admin))
            await users.AddToRoleAsync(admin, AppRoles.Admin);
        if (!await users.IsInRoleAsync(admin, AppRoles.Musteri))
            await users.AddToRoleAsync(admin, AppRoles.Musteri);

        if (string.IsNullOrWhiteSpace(admin.PhoneNumber))
        {
            admin.PhoneNumber = DevelopmentAdminPhone;
            await users.UpdateAsync(admin);
        }
    }
}
