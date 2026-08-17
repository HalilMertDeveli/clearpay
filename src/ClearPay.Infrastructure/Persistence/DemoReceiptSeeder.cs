using ClearPay.Application.Banking;
using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// T-079 Development example: one TopUp pair for admin. Idempotent on CorrelationId.
/// Tests (UseSqliteLedger) skip. No UPDATE Balance.
/// </summary>
public static class DemoReceiptSeeder
{
    public static readonly Guid ExampleCorrelationId = Guid.Parse("aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001");
    public const decimal ExampleAmount = 25.00m;

    public static async Task EnsureExampleAsync(IServiceProvider services, ILogger logger)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var environment = services.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment())
            return;
        if (configuration.GetValue("ClearPay:UseSqliteLedger", false))
            return;
        if (configuration.GetValue("ClearPay:ApplyLedgerMigrations", true) == false)
            return;

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClearPayDbContext>();
        try
        {
            if (!await db.Database.CanConnectAsync().ConfigureAwait(false))
                return;

            var exists = await db.LedgerEntries.AsNoTracking()
                .AnyAsync(e => e.CorrelationId == ExampleCorrelationId)
                .ConfigureAwait(false);
            if (exists)
                return;

            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await users.FindByEmailAsync(IdentitySeeder.DevelopmentAdminEmail).ConfigureAwait(false);
            if (admin is null)
                return;

            var customer = await EnsureWalletAsync(db, admin.Id).ConfigureAwait(false);
            var treasury = await EnsureWalletAsync(db, Treasury.UserId).ConfigureAwait(false);
            var (debit, credit) = LedgerPair.Create(
                treasury.Id,
                customer.Id,
                ExampleAmount,
                ExampleCorrelationId,
                LedgerEntryKind.TopUp,
                description: "Demo örnek dekont");
            db.LedgerEntries.AddRange(debit, credit);
            await db.SaveChangesAsync().ConfigureAwait(false);
            logger.LogInformation(
                "Demo receipt seeded for {Email} correlation {CorrelationId}.",
                IdentitySeeder.DevelopmentAdminEmail,
                ExampleCorrelationId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Demo receipt seed skipped.");
        }
    }

    private static async Task<Wallet> EnsureWalletAsync(ClearPayDbContext db, string userId)
    {
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId).ConfigureAwait(false);
        if (wallet is not null)
            return wallet;

        wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync().ConfigureAwait(false);
        return wallet;
    }
}
