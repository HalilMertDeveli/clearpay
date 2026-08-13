using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearPay.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfer");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Amount)
            .HasPrecision(LedgerSchema.AmountPrecision, LedgerSchema.AmountScale)
            .IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.Status).IsRequired();
        builder.Property(t => t.CorrelationId).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasOne<Wallet>()
            .WithMany()
            .HasForeignKey(t => t.FromWalletId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("FK_Transfer_Wallet_From");

        builder.HasOne<Wallet>()
            .WithMany()
            .HasForeignKey(t => t.ToWalletId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired()
            .HasConstraintName("FK_Transfer_Wallet_To");
    }
}
