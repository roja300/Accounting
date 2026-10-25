using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Tax.Domain;

namespace Server.Tax.Infrastructure.Configurations;

public sealed class TaxYearConfigurationConfiguration : IEntityTypeConfiguration<TaxYearConfiguration>
{
    public void Configure(EntityTypeBuilder<TaxYearConfiguration> builder)
    {
        builder.ToTable("tax_year_configurations", table =>
        {
            table.HasCheckConstraint("ck_tax_year_start", "start_year >= 2000 AND start_year <= 9998");
            table.HasCheckConstraint(
                "ck_tax_year_amounts_non_negative",
                "employment_income >= 0 AND employment_tax_paid >= 0 AND pension_contributions >= 0 AND allowable_expenses >= 0 AND additional_allowances >= 0 AND personal_allowance >= 0 AND personal_allowance_taper_threshold >= 0 AND basic_rate_limit >= 0 AND higher_rate_limit >= basic_rate_limit AND national_insurance_lower_limit >= 0 AND national_insurance_upper_limit >= national_insurance_lower_limit");
            table.HasCheckConstraint(
                "ck_tax_year_rates",
                "basic_rate BETWEEN 0 AND 1 AND higher_rate BETWEEN 0 AND 1 AND additional_rate BETWEEN 0 AND 1 AND national_insurance_main_rate BETWEEN 0 AND 1 AND national_insurance_additional_rate BETWEEN 0 AND 1");
        });
        builder.HasKey(value => value.Id).HasName("pk_tax_year_configurations");
        builder.Property(value => value.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(value => value.StartYear).HasColumnName("start_year").IsRequired();
        Money(builder, value => value.EmploymentIncome, "employment_income");
        Money(builder, value => value.EmploymentTaxPaid, "employment_tax_paid");
        Money(builder, value => value.PensionContributions, "pension_contributions");
        Money(builder, value => value.AllowableExpenses, "allowable_expenses");
        Money(builder, value => value.AdditionalAllowances, "additional_allowances");
        Money(builder, value => value.PersonalAllowance, "personal_allowance");
        Money(builder, value => value.PersonalAllowanceTaperThreshold, "personal_allowance_taper_threshold");
        Money(builder, value => value.BasicRateLimit, "basic_rate_limit");
        Money(builder, value => value.HigherRateLimit, "higher_rate_limit");
        Rate(builder, value => value.BasicRate, "basic_rate");
        Rate(builder, value => value.HigherRate, "higher_rate");
        Rate(builder, value => value.AdditionalRate, "additional_rate");
        Money(builder, value => value.NationalInsuranceLowerLimit, "national_insurance_lower_limit");
        Money(builder, value => value.NationalInsuranceUpperLimit, "national_insurance_upper_limit");
        Rate(builder, value => value.NationalInsuranceMainRate, "national_insurance_main_rate");
        Rate(builder, value => value.NationalInsuranceAdditionalRate, "national_insurance_additional_rate");
        builder.Property(value => value.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(value => value.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(value => value.StartYear).IsUnique().HasDatabaseName("ux_tax_year_configurations_start_year");
    }

    private static void Money(
        EntityTypeBuilder<TaxYearConfiguration> builder,
        System.Linq.Expressions.Expression<Func<TaxYearConfiguration, decimal>> property,
        string column) =>
        builder.Property(property).HasColumnName(column).HasPrecision(14, 2).IsRequired();

    private static void Rate(
        EntityTypeBuilder<TaxYearConfiguration> builder,
        System.Linq.Expressions.Expression<Func<TaxYearConfiguration, decimal>> property,
        string column) =>
        builder.Property(property).HasColumnName(column).HasPrecision(7, 6).IsRequired();
}
