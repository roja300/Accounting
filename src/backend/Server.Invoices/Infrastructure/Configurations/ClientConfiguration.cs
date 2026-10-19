using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(client => client.Id).HasName("pk_clients");

        builder.Property(client => client.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(client => client.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(client => client.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(client => client.ContactName)
            .HasColumnName("contact_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(client => client.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();
        builder.Property(client => client.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(client => client.AddressLine1)
            .HasColumnName("address_line_1")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(client => client.AddressLine2)
            .HasColumnName("address_line_2")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(client => client.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(client => client.County)
            .HasColumnName("county")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(client => client.Postcode)
            .HasColumnName("postcode")
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(client => client.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(client => client.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(client => client.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_clients_normalized_name");
    }
}
