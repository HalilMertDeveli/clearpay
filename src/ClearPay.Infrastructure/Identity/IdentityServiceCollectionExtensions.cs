using Microsoft.AspNetCore.Http;
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
        if (environment.IsProduction())
        {
            var sql = configuration.GetConnectionString("ClearPay");
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:ClearPay is required in Production (Azure SQL). SQLite App_Data is not used live.");
            }

            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(sql));
        }
        else
        {
            var connectionString = ResolveSqliteConnection(
                configuration.GetConnectionString("Identity"),
                environment.ContentRootPath);
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlite(connectionString));
        }

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
            options.LoginPath = "/giris";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/giris";
            options.Cookie.Name = "ClearPay.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
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
        if (!string.IsNullOrWhiteSpace(path) && !Path.IsPathRooted(path))
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
