using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Payments.Domain;

namespace Server.Payments.Infrastructure.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", table =>
        {
            table.HasCheckConstraint("ck_payments_amount_nonzero", "amount <> 0");
            table.HasCheckConstraint("ck_payments_currency", "char_length(currency) = 3");
        });
        builder.HasKey(payment => payment.Id).HasName("pk_payments");
        builder.Property(payment => payment.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(payment => payment.ImportId).HasColumnName("import_id").IsRequired();
        builder.Property(payment => payment.TransactionAt).HasColumnName("transaction_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(payment => payment.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(payment => payment.Counterparty).HasColumnName("counterparty").HasMaxLength(200).IsRequired();
        builder.Property(payment => payment.Reference).HasColumnName("reference").HasMaxLength(200).IsRequired();
        builder.Property(payment => payment.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(payment => payment.Balance).HasColumnName("balance").HasPrecision(18, 2);
        builder.Property(payment => payment.Currency).HasColumnName("currency").HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(payment => payment.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(payment => payment.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(payment => new { payment.TransactionAt, payment.Id }).HasDatabaseName("ix_payments_transaction_at_id");
        builder.HasIndex(payment => payment.ImportId).HasDatabaseName("ix_payments_import_id");
    }
}
