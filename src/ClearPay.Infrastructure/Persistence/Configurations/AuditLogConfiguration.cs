using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearPay.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ActorUserId)
            .IsRequired()
            .HasMaxLength(LedgerSchema.UserIdMaxLength);
        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(a => a.CorrelationId).IsRequired();
        builder.Property(a => a.Details).HasMaxLength(4000);
        builder.Property(a => a.CreatedAt).IsRequired();
    }
}
