using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearPay.Infrastructure.Persistence.Configurations;

internal sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntry", t =>
            t.HasCheckConstraint("CK_LedgerEntry_Amount_NotZero", "[Amount] <> 0"));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Amount)
            .HasPrecision(LedgerSchema.AmountPrecision, LedgerSchema.AmountScale)
            .IsRequired();
        builder.Property(e => e.PairId).IsRequired();
        builder.Property(e => e.CorrelationId).IsRequired();
        builder.Property(e => e.Kind).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Ignore(e => e.IsDebit);
        builder.Ignore(e => e.IsCredit);

        // docs/SQL-PERF.md: covering index for the summary + statement procs (no key lookups).
        builder.HasIndex(e => new { e.WalletId, e.CreatedAt })
            .HasDatabaseName(LedgerSchema.LedgerEntryWalletCreated)
            .IncludeProperties(e => new { e.Amount, e.Kind, e.CorrelationId, e.PairId });

        // Counterparty lookup (statement page, receipt): pair -> other wallet.
        builder.HasIndex(e => e.PairId)
            .HasDatabaseName(LedgerSchema.LedgerEntryPairId)
            .IncludeProperties(e => new { e.WalletId, e.Amount });

        // Day-level totals for reconciliation (usp_Ledger_GetDailyTotals).
        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName(LedgerSchema.LedgerEntryCreatedAt)
            .IncludeProperties(e => new { e.Kind, e.Amount });

        builder.HasOne<Wallet>()
            .WithMany()
            .HasForeignKey(e => e.WalletId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne<Transfer>()
            .WithMany()
            .HasForeignKey(e => e.TransferId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
