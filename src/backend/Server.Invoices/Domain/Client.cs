using Server.Shared.Common;

namespace Server.Invoices.Domain;

public class Client : Entity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;

    public string AddressLine2 { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string County { get; set; } = string.Empty;

    public string Postcode { get; set; } = string.Empty;

    public ICollection<Invoice> Invoices { get; set; } = [];
}
