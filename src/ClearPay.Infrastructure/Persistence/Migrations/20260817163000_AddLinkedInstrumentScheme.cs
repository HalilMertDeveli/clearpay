using System;
using ClearPay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearPay.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ClearPayDbContext))]
    [Migration("20260817163000_AddLinkedInstrumentScheme")]
    public class AddLinkedInstrumentScheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scheme",
                table: "LinkedInstrument",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Unknown");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scheme",
                table: "LinkedInstrument");
        }
    }
}
