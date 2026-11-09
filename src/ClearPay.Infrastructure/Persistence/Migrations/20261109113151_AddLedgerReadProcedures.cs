using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearPay.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T-SQL perf work (docs/SQL-PERF.md): covering / pair / day indexes on LedgerEntry,
    /// plus the read procedures in Persistence/Sql/*.sql (CREATE OR ALTER, safe to re-run).
    /// </summary>
    public partial class AddLedgerReadProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_WalletId_CreatedAt",
                table: "LedgerEntry");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_CreatedAt",
                table: "LedgerEntry",
                column: "CreatedAt")
                .Annotation("SqlServer:Include", new[] { "Kind", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_PairId",
                table: "LedgerEntry",
                column: "PairId")
                .Annotation("SqlServer:Include", new[] { "WalletId", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_WalletId_CreatedAt",
                table: "LedgerEntry",
                columns: new[] { "WalletId", "CreatedAt" })
                .Annotation("SqlServer:Include", new[] { "Amount", "Kind", "CorrelationId", "PairId" });

            foreach (var procedure in LedgerSqlScripts.All)
                migrationBuilder.Sql(LedgerSqlScripts.Read(procedure));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var procedure in LedgerSqlScripts.All)
                migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS dbo.{procedure};");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_CreatedAt",
                table: "LedgerEntry");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_PairId",
                table: "LedgerEntry");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntry_WalletId_CreatedAt",
                table: "LedgerEntry");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntry_WalletId_CreatedAt",
                table: "LedgerEntry",
                columns: new[] { "WalletId", "CreatedAt" });
        }
    }
}
