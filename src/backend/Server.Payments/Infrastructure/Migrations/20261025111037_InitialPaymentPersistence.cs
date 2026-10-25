using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Payments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPaymentPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_imports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_imports", x => x.id);
                    table.CheckConstraint("ck_payment_imports_row_count", "row_count > 0");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    import_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    counterparty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.CheckConstraint("ck_payments_amount_nonzero", "amount <> 0");
                    table.CheckConstraint("ck_payments_currency", "char_length(currency) = 3");
                    table.ForeignKey(
                        name: "FK_payments_payment_imports_import_id",
                        column: x => x.import_id,
                        principalTable: "payment_imports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_imports_created_at",
                table: "payment_imports",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_payments_import_id",
                table: "payments",
                column: "import_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_transaction_at_id",
                table: "payments",
                columns: new[] { "transaction_at", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "payment_imports");
        }
    }
}
