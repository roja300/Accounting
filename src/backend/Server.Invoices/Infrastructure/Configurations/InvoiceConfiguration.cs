using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("ck_invoices_value_positive", "value > 0");
            tableBuilder.HasCheckConstraint(
                "ck_invoices_currency_supported",
                "currency IN ('GBP', 'EUR', 'USD')");
        });

        builder.HasKey(invoice => invoice.Id).HasName("pk_invoices");

        builder.Property(invoice => invoice.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(invoice => invoice.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(invoice => invoice.Value)
            .HasColumnName("value")
            .HasPrecision(12, 2)
            .IsRequired();
        builder.Property(invoice => invoice.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();
        builder.Property(invoice => invoice.Comments)
            .HasColumnName("comments")
            .HasMaxLength(1_000)
            .IsRequired();
        builder.Property(invoice => invoice.ClientId)
            .HasColumnName("client_id")
            .IsRequired();
        builder.Property(invoice => invoice.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();
        builder.Property(invoice => invoice.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(invoice => invoice.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(invoice => invoice.ClientId)
            .HasDatabaseName("ix_invoices_client_id");
        builder.HasIndex(invoice => invoice.ProjectId)
            .HasDatabaseName("ix_invoices_project_id");

        builder.HasOne(invoice => invoice.Client)
            .WithMany(client => client.Invoices)
            .HasForeignKey(invoice => invoice.ClientId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_invoices_clients_client_id");
        builder.HasOne(invoice => invoice.Project)
            .WithMany(project => project.Invoices)
            .HasForeignKey(invoice => invoice.ProjectId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_invoices_projects_project_id");
    }
}
