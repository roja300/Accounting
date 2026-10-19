using Server.Shared.Common;

namespace Server.Invoices.Domain;

public sealed class GeneratedInvoice : Entity
{
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid ClientId { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly DueDate { get; set; }

    public string Currency { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = GeneratedInvoiceStatuses.Exported;

    public string BusinessName { get; set; } = string.Empty;

    public string BusinessEmail { get; set; } = string.Empty;

    public string BusinessPhone { get; set; } = string.Empty;

    public string BusinessAddress { get; set; } = string.Empty;

    public string CompanyNumber { get; set; } = string.Empty;

    public string VatNumber { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string ClientEmail { get; set; } = string.Empty;

    public string ClientPhone { get; set; } = string.Empty;

    public string ClientAddress { get; set; } = string.Empty;

    public ICollection<Invoice> WorkRecords { get; set; } = [];
}

public static class GeneratedInvoiceStatuses
{
    public const string Exported = "exported";
    public const string Sent = "sent";
    public const string PaymentReceived = "paymentReceived";
    public const string Deleted = "deleted";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [Exported, Sent, PaymentReceived, Deleted],
        StringComparer.Ordinal);
}
