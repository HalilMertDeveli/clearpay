using ClearPay.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClearPay.Infrastructure.Persistence.Configurations;

internal sealed class LinkedInstrumentConfiguration : IEntityTypeConfiguration<LinkedInstrument>
{
    public void Configure(EntityTypeBuilder<LinkedInstrument> builder)
    {
        builder.ToTable("LinkedInstrument");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(LedgerSchema.UserIdMaxLength);
        builder.Property(x => x.Last4)
            .IsRequired()
            .HasMaxLength(LedgerSchema.LinkedLast4Length);
        builder.Property(x => x.Label)
            .IsRequired()
            .HasMaxLength(LedgerSchema.LinkedLabelMaxLength);
        builder.Property(x => x.Scheme)
            .IsRequired()
            .HasMaxLength(LedgerSchema.LinkedSchemeMaxLength);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Ignore(x => x.AccountHint);
        builder.HasIndex(x => new { x.UserId, x.Last4 })
            .IsUnique()
            .HasDatabaseName(LedgerSchema.LinkedInstrumentUserLast4Unique);
    }
}
