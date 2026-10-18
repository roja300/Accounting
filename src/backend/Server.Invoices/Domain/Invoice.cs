using Server.Shared.Common;

namespace Server.Invoices.Domain;

/// <summary>
/// Shared type for an invoice.
/// </summary>
public class Invoice : Entity
{
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public decimal Value { get; set; }
    
    public Guid ClientId { get; set; }
    
    public Guid ProjectId { get; set; }
    
    public Client Client { get; set; }
    
    public Project Project { get; set; }
}