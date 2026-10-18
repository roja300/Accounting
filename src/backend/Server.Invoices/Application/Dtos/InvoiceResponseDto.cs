namespace Server.Invoices.Application.Dtos;

public sealed record InvoiceResponseDto(
    Guid Id,
    string Description,
    decimal Value,
    string Currency,
    string Comments,
    string Client,
    string Project,
    DateTimeOffset CreatedAt);
