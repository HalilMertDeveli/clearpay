using ClearPay.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClearPay.Infrastructure.Identity;

public static class IdentitySeeder
{
    public const string DevelopmentAdminEmail = "admin@clearpay.test";

    public static async Task EnsureCreatedAndRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<AppIdentityDbContext>();
        await db.Database.EnsureCreatedAsync();

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
                FullName = "ClearPay Admin"
            };
            var created = await users.CreateAsync(admin, "Deneme123");
            if (!created.Succeeded)
                return;
        }

        if (!await users.IsInRoleAsync(admin, AppRoles.Admin))
            await users.AddToRoleAsync(admin, AppRoles.Admin);
        if (!await users.IsInRoleAsync(admin, AppRoles.Musteri))
            await users.AddToRoleAsync(admin, AppRoles.Musteri);
    }
}
