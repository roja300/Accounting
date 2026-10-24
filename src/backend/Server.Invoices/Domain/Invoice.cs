using Server.Shared.Common;

namespace Server.Invoices.Domain;

/// <summary>
/// Shared type for an invoice.
/// </summary>
public class Invoice : Entity
{
    public string Description { get; set; } = string.Empty;
    
    public decimal Value { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;
    
    public Guid ClientId { get; set; }
    
    public Guid ProjectId { get; set; }

    public Guid? GeneratedInvoiceId { get; set; }
    
    public Client Client { get; set; } = null!;
    
    public Project Project { get; set; } = null!;

    public GeneratedInvoice? GeneratedInvoice { get; set; }
}
