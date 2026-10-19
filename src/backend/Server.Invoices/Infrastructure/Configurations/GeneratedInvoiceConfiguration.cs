using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Configurations;

public sealed class GeneratedInvoiceConfiguration : IEntityTypeConfiguration<GeneratedInvoice>
{
    public void Configure(EntityTypeBuilder<GeneratedInvoice> builder)
    {
        builder.ToTable("generated_invoices", table =>
        {
            table.HasCheckConstraint("ck_generated_invoices_subtotal_positive", "subtotal > 0");
            table.HasCheckConstraint(
                "ck_generated_invoices_status",
                "status IN ('exported', 'sent', 'paymentReceived', 'deleted')");
            table.HasCheckConstraint("ck_generated_invoices_due_date", "due_date >= issue_date");
        });
        builder.HasKey(invoice => invoice.Id).HasName("pk_generated_invoices");
        builder.Property(invoice => invoice.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(invoice => invoice.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(30).IsRequired();
        builder.Property(invoice => invoice.ClientId).HasColumnName("client_id").IsRequired();
        builder.Property(invoice => invoice.IssueDate).HasColumnName("issue_date").HasColumnType("date").IsRequired();
        builder.Property(invoice => invoice.DueDate).HasColumnName("due_date").HasColumnType("date").IsRequired();
        builder.Property(invoice => invoice.Currency).HasColumnName("currency").HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(invoice => invoice.Subtotal).HasColumnName("subtotal").HasPrecision(12, 2).IsRequired();
        builder.Property(invoice => invoice.Notes).HasColumnName("notes").HasMaxLength(1_000).IsRequired();
        builder.Property(invoice => invoice.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(invoice => invoice.BusinessName).HasColumnName("business_name").HasMaxLength(150).IsRequired();
        builder.Property(invoice => invoice.BusinessEmail).HasColumnName("business_email").HasMaxLength(254).IsRequired();
        builder.Property(invoice => invoice.BusinessPhone).HasColumnName("business_phone").HasMaxLength(30).IsRequired();
        builder.Property(invoice => invoice.BusinessAddress).HasColumnName("business_address").HasMaxLength(500).IsRequired();
        builder.Property(invoice => invoice.CompanyNumber).HasColumnName("company_number").HasMaxLength(8).IsRequired();
        builder.Property(invoice => invoice.VatNumber).HasColumnName("vat_number").HasMaxLength(12).IsRequired();
        builder.Property(invoice => invoice.ClientName).HasColumnName("client_name").HasMaxLength(100).IsRequired();
        builder.Property(invoice => invoice.ClientEmail).HasColumnName("client_email").HasMaxLength(254).IsRequired();
        builder.Property(invoice => invoice.ClientPhone).HasColumnName("client_phone").HasMaxLength(30).IsRequired();
        builder.Property(invoice => invoice.ClientAddress).HasColumnName("client_address").HasMaxLength(500).IsRequired();
        builder.Property(invoice => invoice.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(invoice => invoice.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(invoice => invoice.InvoiceNumber).IsUnique().HasDatabaseName("ux_generated_invoices_invoice_number");
        builder.HasIndex(invoice => invoice.ClientId).HasDatabaseName("ix_generated_invoices_client_id");
        builder.HasIndex(invoice => new { invoice.Status, invoice.IssueDate }).HasDatabaseName("ix_generated_invoices_status_issue_date");
    }
}
