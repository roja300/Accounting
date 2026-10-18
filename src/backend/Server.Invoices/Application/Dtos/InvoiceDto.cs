namespace Server.Invoices.Application.Dtos;

public sealed record InvoiceDto(
    string Description,
    decimal Value,
    string Currency,
    string Comments,
    string Client,
    string Project);
