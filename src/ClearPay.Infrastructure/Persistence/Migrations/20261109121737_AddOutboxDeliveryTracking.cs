using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearPay.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxDeliveryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                table: "OutboxMessage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "OutboxMessage",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceParent",
                table: "OutboxMessage",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attempts",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "TraceParent",
                table: "OutboxMessage");
        }
    }
}
