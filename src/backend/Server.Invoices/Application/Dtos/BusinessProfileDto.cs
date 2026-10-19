namespace Server.Invoices.Application.Dtos;

public sealed record BusinessProfileDto(
    string BusinessType,
    string LegalName,
    string? TradingName,
    string Email,
    string? Phone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? County,
    string Postcode,
    string? CompanyNumber,
    string? VatNumber);

public sealed record BusinessProfileResponseDto(
    string BusinessType,
    string LegalName,
    string TradingName,
    string Email,
    string Phone,
    string AddressLine1,
    string AddressLine2,
    string City,
    string County,
    string Postcode,
    string CompanyNumber,
    string VatNumber,
    DateTimeOffset UpdatedAt);
