using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Payments.Domain;

namespace Server.Payments.Infrastructure.Configurations;

public sealed class PaymentImportConfiguration : IEntityTypeConfiguration<PaymentImport>
{
    public void Configure(EntityTypeBuilder<PaymentImport> builder)
    {
        builder.ToTable("payment_imports", table => table.HasCheckConstraint("ck_payment_imports_row_count", "row_count > 0"));
        builder.HasKey(paymentImport => paymentImport.Id).HasName("pk_payment_imports");
        builder.Property(paymentImport => paymentImport.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(paymentImport => paymentImport.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(paymentImport => paymentImport.RowCount).HasColumnName("row_count").IsRequired();
        builder.Property(paymentImport => paymentImport.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(paymentImport => paymentImport.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasMany(paymentImport => paymentImport.Payments).WithOne(payment => payment.Import).HasForeignKey(payment => payment.ImportId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(paymentImport => paymentImport.CreatedAt).HasDatabaseName("ix_payment_imports_created_at");
    }
}
