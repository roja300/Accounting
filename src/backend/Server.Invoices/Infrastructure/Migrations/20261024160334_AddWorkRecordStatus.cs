using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Invoices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkRecordStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "uninvoiced");

            migrationBuilder.AddCheckConstraint(
                name: "ck_invoices_status_supported",
                table: "invoices",
                sql: "status IN ('uninvoiced', 'invoiced', 'deleted')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_invoices_status_supported",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "status",
                table: "invoices");
        }
    }
}
