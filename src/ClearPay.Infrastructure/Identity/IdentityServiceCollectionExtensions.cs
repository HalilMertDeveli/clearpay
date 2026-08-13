using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClearPay.Infrastructure.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddClearPayIdentity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = ResolveSqliteConnection(
            configuration.GetConnectionString("Identity"),
            environment.ContentRootPath);

        services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlite(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/Login";
            options.Cookie.Name = "ClearPay.Auth";
            options.Cookie.HttpOnly = true;
            options.SlidingExpiration = true;
        });

        return services;
    }

    private static string ResolveSqliteConnection(string? configured, string contentRoot)
    {
        const string prefix = "Data Source=";
        var raw = string.IsNullOrWhiteSpace(configured)
            ? prefix + Path.Combine("App_Data", "identity.db")
            : configured.Trim();

        if (!raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return raw;
        }

        var path = raw[prefix.Length..].Trim().Trim('"');
        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(Path.Combine(contentRoot, path));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return prefix + path;
    }
}
