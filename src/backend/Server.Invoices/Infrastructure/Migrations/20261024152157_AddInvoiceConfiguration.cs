using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Invoices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoice_style",
                table: "generated_invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "basic");

            migrationBuilder.AddColumn<byte[]>(
                name: "logo_data",
                table: "generated_invoices",
                type: "bytea",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "invoice_configuration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    style = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    logo_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    logo_content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    logo_data = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_configuration", x => x.id);
                    table.CheckConstraint("ck_invoice_configuration_singleton", "id = '00000000-0000-0000-0000-000000000002'");
                    table.CheckConstraint("ck_invoice_configuration_style", "style IN ('basic', 'modern', 'minimal')");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_configuration");

            migrationBuilder.DropColumn(
                name: "invoice_style",
                table: "generated_invoices");

            migrationBuilder.DropColumn(
                name: "logo_data",
                table: "generated_invoices");
        }
    }
}
