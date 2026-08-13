using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearPay.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecord");
        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key)
            .HasMaxLength(LedgerSchema.IdempotencyKeyMaxLength)
            .IsRequired();
        builder.HasIndex(r => r.Key)
            .IsUnique()
            .HasDatabaseName(LedgerSchema.IdempotencyKeyUnique);
        builder.Property(r => r.Scope)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(r => r.RequestHash).HasMaxLength(128);
        builder.Property(r => r.CreatedAt).IsRequired();
    }
}
