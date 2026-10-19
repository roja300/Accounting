using Server.Shared.Common;

namespace Server.Invoices.Domain;

public sealed class BusinessProfile : Entity
{
    public string BusinessType { get; set; } = string.Empty;

    public string LegalName { get; set; } = string.Empty;

    public string TradingName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;

    public string AddressLine2 { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string County { get; set; } = string.Empty;

    public string Postcode { get; set; } = string.Empty;

    public string CompanyNumber { get; set; } = string.Empty;

    public string VatNumber { get; set; } = string.Empty;
}
