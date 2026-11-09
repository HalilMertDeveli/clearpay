using ClearPay.Domain.Ledger;
using ClearPay.Domain.Payments;
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

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PaymentRefund> PaymentRefunds => Set<PaymentRefund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ClearPayDbContext).Assembly,
            type => type.Namespace == "ClearPay.Infrastructure.Persistence.Configurations");
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        BumpPaymentVersions();
        StampOutboxTraceContext();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        BumpPaymentVersions();
        StampOutboxTraceContext();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Every outbox insert remembers the current W3C trace (the HTTP request span). One place instead of
    /// four writers (transfer, top-up, withdraw, card payments), so no event can forget it.
    /// </summary>
    private void StampOutboxTraceContext()
    {
        var current = System.Diagnostics.Activity.Current;
        if (current is null || current.IdFormat != System.Diagnostics.ActivityIdFormat.W3C)
            return;
        foreach (var entry in ChangeTracker.Entries<OutboxMessage>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TraceParent is null)
                entry.Entity.TraceParent = current.Id;
        }
    }

    /// <summary>
    /// <see cref="Payment.Version"/> is the concurrency token. EF compares the original value in the
    /// UPDATE's WHERE clause; bumping it here means a second writer holding the old version gets
    /// <see cref="DbUpdateConcurrencyException"/> instead of silently overwriting.
    /// </summary>
    private void BumpPaymentVersions()
    {
        foreach (var entry in ChangeTracker.Entries<Payment>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.Version++;
        }
    }
}
