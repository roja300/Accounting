using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Configurations;

public sealed class BusinessProfileConfiguration : IEntityTypeConfiguration<BusinessProfile>
{
    public void Configure(EntityTypeBuilder<BusinessProfile> builder)
    {
        builder.ToTable("business_profiles", table =>
            table.HasCheckConstraint(
                "ck_business_profiles_singleton",
                "id = '00000000-0000-0000-0000-000000000001'"));
        builder.HasKey(profile => profile.Id).HasName("pk_business_profiles");

        builder.Property(profile => profile.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(profile => profile.BusinessType).HasColumnName("business_type").HasMaxLength(20).IsRequired();
        builder.Property(profile => profile.LegalName).HasColumnName("legal_name").HasMaxLength(150).IsRequired();
        builder.Property(profile => profile.TradingName).HasColumnName("trading_name").HasMaxLength(150).IsRequired();
        builder.Property(profile => profile.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.Property(profile => profile.Phone).HasColumnName("phone").HasMaxLength(30).IsRequired();
        builder.Property(profile => profile.AddressLine1).HasColumnName("address_line_1").HasMaxLength(100).IsRequired();
        builder.Property(profile => profile.AddressLine2).HasColumnName("address_line_2").HasMaxLength(100).IsRequired();
        builder.Property(profile => profile.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        builder.Property(profile => profile.County).HasColumnName("county").HasMaxLength(100).IsRequired();
        builder.Property(profile => profile.Postcode).HasColumnName("postcode").HasMaxLength(10).IsRequired();
        builder.Property(profile => profile.CompanyNumber).HasColumnName("company_number").HasMaxLength(8).IsRequired();
        builder.Property(profile => profile.VatNumber).HasColumnName("vat_number").HasMaxLength(12).IsRequired();
        builder.Property(profile => profile.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(profile => profile.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();
    }
}
