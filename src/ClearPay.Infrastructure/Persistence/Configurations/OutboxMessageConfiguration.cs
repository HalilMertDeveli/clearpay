using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearPay.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessage");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.CorrelationId).IsRequired();
        builder.Property(m => m.OccurredAt).IsRequired();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.Attempts).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.LastError).HasMaxLength(1000);
        // W3C traceparent is 55 chars ("00-<32 hex>-<16 hex>-01").
        builder.Property(m => m.TraceParent).HasMaxLength(64);
        builder.HasIndex(m => new { m.Status, m.OccurredAt })
            .HasDatabaseName(LedgerSchema.OutboxStatusOccurred);
    }
}
