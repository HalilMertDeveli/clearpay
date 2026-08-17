using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClearPay.Tests;

public sealed class ClearPayDbContextTests
{
    [Fact]
    public void Model_has_no_balance_column_and_plan_indexes()
    {
        using var db = CreateModelContext();
        var model = db.Model;

        var wallet = model.FindEntityType(typeof(Wallet));
        wallet.Should().NotBeNull();
        wallet!.FindProperty("Balance").Should().BeNull("UPDATE Balance is forbidden; net is ledger");
        wallet.GetTableName().Should().Be("Wallet");
        wallet.GetIndexes().Should().Contain(i =>
            i.IsUnique
            && i.GetDatabaseName() == LedgerSchema.WalletUserIdUnique
            && i.Properties.Any(p => p.Name == nameof(Wallet.UserId)));

        var entry = model.FindEntityType(typeof(LedgerEntry));
        entry.Should().NotBeNull();
        entry!.GetTableName().Should().Be("LedgerEntry");
        var amount = entry.FindProperty(nameof(LedgerEntry.Amount));
        amount!.GetPrecision().Should().Be(LedgerSchema.AmountPrecision);
        amount.GetScale().Should().Be(LedgerSchema.AmountScale);
        entry.GetIndexes().Should().Contain(i =>
            i.GetDatabaseName() == LedgerSchema.LedgerEntryWalletCreated
            && i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(LedgerEntry.WalletId),
                nameof(LedgerEntry.CreatedAt)
            }));

        var idempotency = model.FindEntityType(typeof(IdempotencyRecord));
        idempotency.Should().NotBeNull();
        idempotency!.FindPrimaryKey()!.Properties.Should()
            .ContainSingle(p => p.Name == nameof(IdempotencyRecord.Key));
        idempotency.GetIndexes().Should().Contain(i =>
            i.IsUnique && i.GetDatabaseName() == LedgerSchema.IdempotencyKeyUnique);

        var outbox = model.FindEntityType(typeof(OutboxMessage));
        outbox.Should().NotBeNull();
        outbox!.GetTableName().Should().Be("OutboxMessage");
        outbox.GetIndexes().Should().Contain(i =>
            i.GetDatabaseName() == LedgerSchema.OutboxStatusOccurred);

        model.FindEntityType(typeof(Transfer))!.GetTableName().Should().Be("Transfer");
        model.FindEntityType(typeof(AuditLog))!.GetTableName().Should().Be("AuditLog");

        var card = model.FindEntityType(typeof(LinkedInstrument));
        card.Should().NotBeNull();
        card!.GetTableName().Should().Be("LinkedInstrument");
        card.FindProperty("Pan").Should().BeNull();
        card.FindProperty("Cvv").Should().BeNull();
        card.GetIndexes().Should().Contain(i =>
            i.IsUnique
            && i.GetDatabaseName() == LedgerSchema.LinkedInstrumentUserLast4Unique);
    }

    [Fact]
    public void Create_script_includes_required_tables()
    {
        using var db = CreateModelContext();
        var script = db.Database.GenerateCreateScript();

        script.Should().Contain("CREATE TABLE [Wallet]");
        script.Should().Contain("CREATE TABLE [LedgerEntry]");
        script.Should().Contain("CREATE TABLE [Transfer]");
        script.Should().Contain("CREATE TABLE [IdempotencyRecord]");
        script.Should().Contain("CREATE TABLE [AuditLog]");
        script.Should().Contain("CREATE TABLE [OutboxMessage]");
        script.Should().Contain("CREATE TABLE [LinkedInstrument]");
        script.Should().Contain(LedgerSchema.LinkedInstrumentUserLast4Unique);
        script.Should().NotContain("[Balance]");
        script.Should().NotContain("Pan");
        script.Should().NotContain("CVV");
        script.Should().Contain(LedgerSchema.WalletUserIdUnique);
        script.Should().Contain(LedgerSchema.LedgerEntryWalletCreated);
        script.Should().Contain(LedgerSchema.IdempotencyKeyUnique);
        script.Should().Contain("CK_LedgerEntry_Amount_NotZero");
    }

    private static ClearPayDbContext CreateModelContext()
    {
        var options = new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=ClearPay_ModelCheck;TrustServerCertificate=True")
            .Options;
        return new ClearPayDbContext(options);
    }
}
