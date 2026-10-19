namespace Server.Invoices.Application.Dtos;

public sealed class InvoiceListQueryDto
{
    public DateOnly? DateAfter { get; init; }

    public DateOnly? DateBefore { get; init; }

    public string? Project { get; init; }

    public string? Client { get; init; }

    public string? Search { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 12;
}
