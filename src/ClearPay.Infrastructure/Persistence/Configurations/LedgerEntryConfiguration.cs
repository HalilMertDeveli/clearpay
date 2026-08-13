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

        builder.HasIndex(e => new { e.WalletId, e.CreatedAt })
            .HasDatabaseName(LedgerSchema.LedgerEntryWalletCreated);

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
