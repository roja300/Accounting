using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Tax.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTaxYearConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tax_year_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_year = table.Column<int>(type: "integer", nullable: false),
                    employment_income = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    employment_tax_paid = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    pension_contributions = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    allowable_expenses = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    additional_allowances = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    personal_allowance = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    personal_allowance_taper_threshold = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    basic_rate_limit = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    higher_rate_limit = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    basic_rate = table.Column<decimal>(type: "numeric(7,6)", precision: 7, scale: 6, nullable: false),
                    higher_rate = table.Column<decimal>(type: "numeric(7,6)", precision: 7, scale: 6, nullable: false),
                    additional_rate = table.Column<decimal>(type: "numeric(7,6)", precision: 7, scale: 6, nullable: false),
                    national_insurance_lower_limit = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    national_insurance_upper_limit = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    national_insurance_main_rate = table.Column<decimal>(type: "numeric(7,6)", precision: 7, scale: 6, nullable: false),
                    national_insurance_additional_rate = table.Column<decimal>(type: "numeric(7,6)", precision: 7, scale: 6, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_year_configurations", x => x.id);
                    table.CheckConstraint("ck_tax_year_amounts_non_negative", "employment_income >= 0 AND employment_tax_paid >= 0 AND pension_contributions >= 0 AND allowable_expenses >= 0 AND additional_allowances >= 0 AND personal_allowance >= 0 AND personal_allowance_taper_threshold >= 0 AND basic_rate_limit >= 0 AND higher_rate_limit >= basic_rate_limit AND national_insurance_lower_limit >= 0 AND national_insurance_upper_limit >= national_insurance_lower_limit");
                    table.CheckConstraint("ck_tax_year_rates", "basic_rate BETWEEN 0 AND 1 AND higher_rate BETWEEN 0 AND 1 AND additional_rate BETWEEN 0 AND 1 AND national_insurance_main_rate BETWEEN 0 AND 1 AND national_insurance_additional_rate BETWEEN 0 AND 1");
                    table.CheckConstraint("ck_tax_year_start", "start_year >= 2000 AND start_year <= 9998");
                });

            migrationBuilder.CreateIndex(
                name: "ux_tax_year_configurations_start_year",
                table: "tax_year_configurations",
                column: "start_year",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tax_year_configurations");
        }
    }
}
