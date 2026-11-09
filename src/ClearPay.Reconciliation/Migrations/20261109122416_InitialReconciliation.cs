using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearPay.Reconciliation.Migrations
{
    /// <inheritdoc />
    public partial class InitialReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "recon");

            migrationBuilder.CreateTable(
                name: "PaymentTransaction",
                schema: "recon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransaction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessage",
                schema: "recon",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Consumer = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationReport",
                schema: "recon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Trigger = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MatchedCount = table.Column<int>(type: "int", nullable: false),
                    OnlyAtBankCount = table.Column<int>(type: "int", nullable: false),
                    OnlyInClearPayCount = table.Column<int>(type: "int", nullable: false),
                    AmountMismatchCount = table.Column<int>(type: "int", nullable: false),
                    LedgerImbalanceCount = table.Column<int>(type: "int", nullable: false),
                    BankNetTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClearPayNetTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LedgerCheck = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    MismatchEventPublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationReport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationReportLine",
                schema: "recon",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BankAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ClearPayAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationReportLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationReportLine_ReconciliationReport_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "recon",
                        principalTable: "ReconciliationReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransaction_BusinessDate_Provider",
                schema: "recon",
                table: "PaymentTransaction",
                columns: new[] { "BusinessDate", "Provider" });

            migrationBuilder.CreateIndex(
                name: "UX_PaymentTransaction_Kind_Reference",
                schema: "recon",
                table: "PaymentTransaction",
                columns: new[] { "Kind", "ReferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ProcessedMessage_Consumer_MessageId",
                schema: "recon",
                table: "ProcessedMessage",
                columns: new[] { "Consumer", "MessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationReport_BusinessDate",
                schema: "recon",
                table: "ReconciliationReport",
                column: "BusinessDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationReportLine_ReportId",
                schema: "recon",
                table: "ReconciliationReportLine",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTransaction",
                schema: "recon");

            migrationBuilder.DropTable(
                name: "ProcessedMessage",
                schema: "recon");

            migrationBuilder.DropTable(
                name: "ReconciliationReportLine",
                schema: "recon");

            migrationBuilder.DropTable(
                name: "ReconciliationReport",
                schema: "recon");
        }
    }
}
