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

    public string Status { get; set; } = WorkRecordStatuses.Uninvoiced;
    
    public Guid ClientId { get; set; }
    
    public Guid ProjectId { get; set; }
    
    public Client Client { get; set; } = null!;
    
    public Project Project { get; set; } = null!;
}

public static class WorkRecordStatuses
{
    public const string Uninvoiced = "uninvoiced";
    public const string Invoiced = "invoiced";
    public const string Deleted = "deleted";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [Uninvoiced, Invoiced, Deleted],
        StringComparer.Ordinal);
}
