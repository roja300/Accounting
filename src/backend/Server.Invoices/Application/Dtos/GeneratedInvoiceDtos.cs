namespace Server.Invoices.Application.Dtos;

public sealed record GenerateInvoiceDto(
    Guid ClientId,
    Guid? ProjectId,
    string Currency,
    DateOnly IssueDate,
    DateOnly DueDate,
    string? Notes);

public sealed record UpdateGeneratedInvoiceStatusDto(string Status);

public sealed record GeneratedInvoiceLineDto(
    Guid WorkRecordId,
    string Description,
    string Project,
    decimal Value);

public sealed record GeneratedInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid ClientId,
    string ClientName,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    decimal Subtotal,
    string Notes,
    string Status,
    IReadOnlyList<GeneratedInvoiceLineDto> Lines,
    DateTimeOffset CreatedAt);

public sealed record GeneratedInvoicePageDto(
    IReadOnlyList<GeneratedInvoiceDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record ExportOptionDto(Guid Id, string Name);

public sealed record GenerateInvoiceOptionsDto(
    IReadOnlyList<ExportOptionDto> Clients,
    IReadOnlyList<ExportOptionDto> Projects);

public sealed class GeneratedInvoiceListQueryDto
{
    public DateOnly? DateAfter { get; init; }

    public DateOnly? DateBefore { get; init; }

    public Guid? ClientId { get; init; }

    public string? Status { get; init; }

    public string? Search { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 9;
}
