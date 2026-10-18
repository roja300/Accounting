using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(project => project.Id).HasName("pk_projects");

        builder.Property(project => project.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(project => project.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(project => project.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(project => project.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(project => project.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(project => project.NormalizedName)
            .IsUnique()
            .HasDatabaseName("ux_projects_normalized_name");
    }
}
