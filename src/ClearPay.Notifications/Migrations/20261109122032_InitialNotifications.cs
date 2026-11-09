using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearPay.Notifications.Migrations
{
    /// <inheritdoc />
    public partial class InitialNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notif");

            migrationBuilder.CreateTable(
                name: "NotificationLog",
                schema: "notif",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessage",
                schema: "notif",
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

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_CreatedAt",
                schema: "notif",
                table: "NotificationLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLog_MessageId",
                schema: "notif",
                table: "NotificationLog",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "UX_ProcessedMessage_Consumer_MessageId",
                schema: "notif",
                table: "ProcessedMessage",
                columns: new[] { "Consumer", "MessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationLog",
                schema: "notif");

            migrationBuilder.DropTable(
                name: "ProcessedMessage",
                schema: "notif");
        }
    }
}
