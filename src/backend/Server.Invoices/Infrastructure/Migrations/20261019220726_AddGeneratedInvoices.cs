using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Invoices.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "generated_invoice_id",
                table: "invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "generated_invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    business_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    business_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    business_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    business_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    company_number = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    vat_number = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    client_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    client_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    client_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    client_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_generated_invoices", x => x.id);
                    table.CheckConstraint("ck_generated_invoices_due_date", "due_date >= issue_date");
                    table.CheckConstraint("ck_generated_invoices_status", "status IN ('exported', 'sent', 'paymentReceived', 'deleted')");
                    table.CheckConstraint("ck_generated_invoices_subtotal_positive", "subtotal > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_invoices_generated_invoice_id",
                table: "invoices",
                column: "generated_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_generated_invoices_client_id",
                table: "generated_invoices",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_generated_invoices_status_issue_date",
                table: "generated_invoices",
                columns: new[] { "status", "issue_date" });

            migrationBuilder.CreateIndex(
                name: "ux_generated_invoices_invoice_number",
                table: "generated_invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_invoices_generated_invoices_generated_invoice_id",
                table: "invoices",
                column: "generated_invoice_id",
                principalTable: "generated_invoices",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_invoices_generated_invoices_generated_invoice_id",
                table: "invoices");

            migrationBuilder.DropTable(
                name: "generated_invoices");

            migrationBuilder.DropIndex(
                name: "ix_invoices_generated_invoice_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "generated_invoice_id",
                table: "invoices");
        }
    }
}
