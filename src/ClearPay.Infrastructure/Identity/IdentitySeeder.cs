using ClearPay.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ClearPay.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task EnsureCreatedAndRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var db = provider.GetRequiredService<AppIdentityDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roles = provider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync(AppRoles.Musteri))
        {
            await roles.CreateAsync(new IdentityRole(AppRoles.Musteri));
        }
    }
}
