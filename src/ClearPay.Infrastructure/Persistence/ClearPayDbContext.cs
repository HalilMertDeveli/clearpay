using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Infrastructure.Persistence;

/// <summary>
/// SQL Server ledger store. Identity is <c>AppIdentityDbContext</c> on the same SQL Server (SQLite only in tests).
/// No Balance column. PageModels must not take this type — use Application ports.
/// </summary>
public sealed class ClearPayDbContext : DbContext
{
    public ClearPayDbContext(DbContextOptions<ClearPayDbContext> options)
        : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<LinkedInstrument> LinkedInstruments => Set<LinkedInstrument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ClearPayDbContext).Assembly,
            type => type.Namespace == "ClearPay.Infrastructure.Persistence.Configurations");
    }
}
