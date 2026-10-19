namespace Server.Invoices.Application.Dtos;

public sealed record ClientDto(
    string Name,
    string? ContactName,
    string Email,
    string? Phone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? County,
    string Postcode);

public sealed record ClientResponseDto(
    Guid Id,
    string Name,
    string ContactName,
    string Email,
    string Phone,
    string AddressLine1,
    string AddressLine2,
    string City,
    string County,
    string Postcode,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
