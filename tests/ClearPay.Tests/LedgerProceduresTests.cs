using ClearPay.Domain.Ledger;
using ClearPay.Infrastructure.Persistence;
using ClearPay.Infrastructure.Persistence.Migrations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace ClearPay.Tests;

/// <summary>
/// T-SQL perf work (docs/SQL-PERF.md): the procs ship with the migration and the indexes are in the EF model.
/// These run everywhere; the SQL Server behaviour itself is in <see cref="LedgerProceduresSqlServerTests"/>.
/// </summary>
public sealed class LedgerProceduresTests
{
    [Fact]
    public void Each_procedure_is_an_embedded_idempotent_create_or_alter()
    {
        LedgerSqlScripts.All.Should().BeEquivalentTo(
            "usp_Wallet_GetSummary", "usp_Wallet_GetStatementPage", "usp_Ledger_GetDailyTotals");

        foreach (var name in LedgerSqlScripts.All)
        {
            var sql = LedgerSqlScripts.Read(name);
            sql.Should().Contain($"CREATE OR ALTER PROCEDURE dbo.{name}");
            sql.Should().NotContain("UPDATE", "read procs never write; there is no balance column to update");
            sql.Split('\n').Should().NotContain(l => l.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void Migration_creates_indexes_then_procedures_and_down_drops_them()
    {
        var migration = new AddLedgerReadProcedures();

        var up = migration.UpOperations;
        up.OfType<CreateIndexOperation>().Select(i => i.Name).Should().BeEquivalentTo(
            LedgerSchema.LedgerEntryWalletCreated, LedgerSchema.LedgerEntryPairId, LedgerSchema.LedgerEntryCreatedAt);
        var procSql = up.OfType<SqlOperation>().Select(o => o.Sql).ToList();
        foreach (var name in LedgerSqlScripts.All)
            procSql.Should().ContainSingle(s => s.Contains($"CREATE OR ALTER PROCEDURE dbo.{name}"));

        migration.DownOperations.OfType<SqlOperation>().Select(o => o.Sql).Should().BeEquivalentTo(
            LedgerSqlScripts.All.Select(n => $"DROP PROCEDURE IF EXISTS dbo.{n};"));
    }

    [Fact]
    public void Ledger_indexes_cover_the_procedure_columns()
    {
        var options = new DbContextOptionsBuilder<ClearPayDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;
        using var db = new ClearPayDbContext(options);
        var model = db.GetService<IDesignTimeModel>().Model;
        var indexes = model.FindEntityType(typeof(LedgerEntry))!.GetIndexes()
            .ToDictionary(i => i.GetDatabaseName()!);

        indexes[LedgerSchema.LedgerEntryWalletCreated].Properties.Select(p => p.Name)
            .Should().Equal(nameof(LedgerEntry.WalletId), nameof(LedgerEntry.CreatedAt));
        indexes[LedgerSchema.LedgerEntryWalletCreated].GetIncludeProperties()
            .Should().BeEquivalentTo("Amount", "Kind", "CorrelationId", "PairId");
        indexes[LedgerSchema.LedgerEntryPairId].GetIncludeProperties()
            .Should().BeEquivalentTo("WalletId", "Amount");
        indexes[LedgerSchema.LedgerEntryCreatedAt].GetIncludeProperties()
            .Should().BeEquivalentTo("Kind", "Amount");
    }

    [Fact]
    public void Procedures_are_not_used_on_sqlite()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        using var db = new ClearPayDbContext(
            new DbContextOptionsBuilder<ClearPayDbContext>().UseSqlite(connection).Options);

        new LedgerProcedures(db).IsAvailable.Should().BeFalse();
    }
}
