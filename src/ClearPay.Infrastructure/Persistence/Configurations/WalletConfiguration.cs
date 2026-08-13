using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearPay.Infrastructure.Persistence.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallet");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.UserId)
            .IsRequired()
            .HasMaxLength(LedgerSchema.UserIdMaxLength);
        builder.HasIndex(w => w.UserId)
            .IsUnique()
            .HasDatabaseName(LedgerSchema.WalletUserIdUnique);
        builder.Property(w => w.IsFrozen).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Ignore(w => w.CanDebit);
    }
}
