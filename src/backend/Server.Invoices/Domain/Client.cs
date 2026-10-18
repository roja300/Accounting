using Server.Shared.Common;

namespace Server.Invoices.Domain;

public class Client : Entity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public ICollection<Invoice> Invoices { get; set; } = [];
}
