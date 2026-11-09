using ClearPay.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClearPay.Reconciliation;

/// <summary>
/// The reconciliation service's own tables in the <c>recon</c> schema. "Our side" of the reconciliation is
/// <see cref="PaymentTransaction"/>, a projection built from <c>clearpay.payment.captured / refunded</c>
/// events, not a query on the web app's <c>dbo.Payment</c> table.
/// </summary>
public sealed class ReconciliationDbContext : DbContext
{
    public const string Schema = "recon";

    public ReconciliationDbContext(DbContextOptions<ReconciliationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    public DbSet<ReconciliationReport> Reports => Set<ReconciliationReport>();

    public DbSet<ReconciliationReportLine> ReportLines => Set<ReconciliationReportLine>();

    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.AddProcessedMessages(Schema);

        modelBuilder.Entity<PaymentTransaction>(b =>
        {
            b.ToTable("PaymentTransaction");
            b.HasKey(t => t.Id);
            b.Property(t => t.Kind).IsRequired().HasMaxLength(8);
            b.Property(t => t.Provider).IsRequired().HasMaxLength(64);
            b.Property(t => t.MerchantId).IsRequired().HasMaxLength(64);
            b.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            b.Property(t => t.Amount).HasPrecision(18, 2);
            // One SALE per payment, one REFUND per refund id: a second event for the same money is a bug, not data.
            b.HasIndex(t => new { t.Kind, t.ReferenceId }).IsUnique().HasDatabaseName("UX_PaymentTransaction_Kind_Reference");
            // The nightly job reads one business day per provider.
            b.HasIndex(t => new { t.BusinessDate, t.Provider }).HasDatabaseName("IX_PaymentTransaction_BusinessDate_Provider");
        });

        modelBuilder.Entity<ReconciliationReport>(b =>
        {
            b.ToTable("ReconciliationReport");
            b.HasKey(r => r.Id);
            b.Property(r => r.Trigger).IsRequired().HasMaxLength(16);
            b.Property(r => r.LedgerCheck).IsRequired().HasMaxLength(16);
            b.Property(r => r.BankNetTotal).HasPrecision(18, 2);
            b.Property(r => r.ClearPayNetTotal).HasPrecision(18, 2);
            b.HasIndex(r => r.BusinessDate).HasDatabaseName("IX_ReconciliationReport_BusinessDate");
            b.HasMany(r => r.Lines).WithOne().HasForeignKey(l => l.ReportId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReconciliationReportLine>(b =>
        {
            b.ToTable("ReconciliationReportLine");
            b.HasKey(l => l.Id);
            b.Property(l => l.Provider).IsRequired().HasMaxLength(64);
            b.Property(l => l.Kind).IsRequired().HasMaxLength(32);
            b.Property(l => l.Reference).IsRequired().HasMaxLength(64);
            b.Property(l => l.Status).IsRequired().HasMaxLength(20);
            b.Property(l => l.Currency).HasMaxLength(3);
            b.Property(l => l.BankAmount).HasPrecision(18, 2);
            b.Property(l => l.ClearPayAmount).HasPrecision(18, 2);
        });
    }
}

/// <summary>
/// A money movement ClearPay expects to see in an acquirer's settlement file.
/// SALE = a capture (amount = captured amount), REFUND = an approved refund.
/// </summary>
public sealed class PaymentTransaction
{
    public const string Sale = "SALE";
    public const string Refund = "REFUND";

    public Guid Id { get; set; }

    public string Kind { get; set; } = Sale;

    public Guid PaymentId { get; set; }

    /// <summary>Payment id for a SALE, refund id for a REFUND. This is the reference the bank file carries.</summary>
    public Guid ReferenceId { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string MerchantId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "TRY";

    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Istanbul calendar day of <see cref="OccurredAt"/>, stored so the job filters on a date column.</summary>
    public DateOnly BusinessDate { get; set; }

    public Guid SourceMessageId { get; set; }
}

public sealed class ReconciliationReport
{
    public Guid Id { get; set; }

    public DateOnly BusinessDate { get; set; }

    /// <summary><c>nightly</c> (Hangfire) or <c>manual</c> (API).</summary>
    public string Trigger { get; set; } = "manual";

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    public int MatchedCount { get; set; }

    public int OnlyAtBankCount { get; set; }

    public int OnlyInClearPayCount { get; set; }

    public int AmountMismatchCount { get; set; }

    public int LedgerImbalanceCount { get; set; }

    /// <summary>SALE − REFUND as the banks reported it.</summary>
    public decimal BankNetTotal { get; set; }

    /// <summary>SALE − REFUND as ClearPay recorded it.</summary>
    public decimal ClearPayNetTotal { get; set; }

    /// <summary><c>Skipped</c> (no read-only ledger connection), <c>Balanced</c> or <c>Imbalanced</c>.</summary>
    public string LedgerCheck { get; set; } = "Skipped";

    /// <summary>Null until the mismatch event is confirmed by RabbitMQ (acts as this service's tiny outbox).</summary>
    public DateTimeOffset? MismatchEventPublishedAt { get; set; }

    public List<ReconciliationReportLine> Lines { get; set; } = [];

    public bool HasDifferences =>
        OnlyAtBankCount + OnlyInClearPayCount + AmountMismatchCount + LedgerImbalanceCount > 0;
}

public sealed class ReconciliationReportLine
{
    public long Id { get; set; }

    public Guid ReportId { get; set; }

    public string Provider { get; set; } = string.Empty;

    /// <summary>SALE / REFUND, or a ledger kind for <see cref="MatchStatus.LedgerImbalance"/> lines.</summary>
    public string Kind { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal? BankAmount { get; set; }

    public decimal? ClearPayAmount { get; set; }

    public string? Currency { get; set; }
}

/// <summary>For <c>dotnet ef migrations add</c>; the connection string is never opened.</summary>
public sealed class ReconciliationDesignTimeFactory : IDesignTimeDbContextFactory<ReconciliationDbContext>
{
    public ReconciliationDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ReconciliationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=ClearPay;Integrated Security=True;TrustServerCertificate=True",
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", ReconciliationDbContext.Schema))
            .Options);
}
