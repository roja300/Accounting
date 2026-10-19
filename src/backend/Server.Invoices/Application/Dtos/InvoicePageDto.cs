namespace Server.Invoices.Application.Dtos;

public sealed record InvoicePageDto(
    IReadOnlyList<InvoiceResponseDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
