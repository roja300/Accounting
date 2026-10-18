namespace Server.Invoices.Application.Dtos;

public sealed record InvoiceOptionsDto(
    IReadOnlyList<string> Clients,
    IReadOnlyList<string> Projects);
