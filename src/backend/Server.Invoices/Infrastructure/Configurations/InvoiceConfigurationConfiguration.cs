using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Configurations;

public sealed class InvoiceConfigurationConfiguration : IEntityTypeConfiguration<Server.Invoices.Domain.InvoiceConfiguration>
{
    public void Configure(EntityTypeBuilder<Server.Invoices.Domain.InvoiceConfiguration> builder)
    {
        builder.ToTable("invoice_configuration", table =>
        {
            table.HasCheckConstraint(
                "ck_invoice_configuration_singleton",
                "id = '00000000-0000-0000-0000-000000000002'");
            table.HasCheckConstraint(
                "ck_invoice_configuration_style",
                "style IN ('basic', 'modern', 'minimal')");
        });
        builder.HasKey(configuration => configuration.Id).HasName("pk_invoice_configuration");
        builder.Property(configuration => configuration.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(configuration => configuration.Style).HasColumnName("style").HasMaxLength(20).IsRequired();
        builder.Property(configuration => configuration.LogoFileName).HasColumnName("logo_file_name").HasMaxLength(255).IsRequired();
        builder.Property(configuration => configuration.LogoContentType).HasColumnName("logo_content_type").HasMaxLength(50).IsRequired();
        builder.Property(configuration => configuration.LogoData).HasColumnName("logo_data").HasColumnType("bytea");
        builder.Property(configuration => configuration.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(configuration => configuration.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();
    }
}
