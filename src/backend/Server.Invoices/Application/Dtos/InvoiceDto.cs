namespace Server.Invoices.Application.Dtos;

public sealed record InvoiceDto(
    string Description,
    decimal Value,
    string Currency,
    string Comments,
    string Client,
    string Project);

public sealed record InvoiceResponseDto(
    Guid Id,
    string Description,
    decimal Value,
    string Currency,
    string Comments,
    string Client,
    string Project,
    DateTimeOffset CreatedAt);

public sealed record InvoiceOptionsDto(
    IReadOnlyList<string> Clients,
    IReadOnlyList<string> Projects);
