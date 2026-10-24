using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Services;

namespace Server.Api.Tests;

internal sealed class StubInvoiceService : IInvoiceService
{
    public static readonly Guid InvoiceId =
        Guid.Parse("1f184969-9587-4103-b6ee-80a467669fa8");

    public InvoiceDto? CreatedInvoice { get; private set; }

    public InvoiceDto? UpdatedInvoice { get; private set; }

    public Guid? UpdatedInvoiceId { get; private set; }

    public Guid? StatusUpdatedInvoiceId { get; private set; }

    public string? UpdatedStatus { get; private set; }

    public InvoiceListQueryDto? ListQuery { get; private set; }

    public bool RejectClient { get; set; }

    public Task<InvoicePageDto> GetPageAsync(
        InvoiceListQueryDto query,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ListQuery = query;
        return Task.FromResult(new InvoicePageDto(
            [ToResponse(CreateInvoice())],
            query.Page,
            query.PageSize,
            1,
            1));
    }

    public Task<InvoiceResponseDto> CreateAsync(
        InvoiceDto invoice,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (RejectClient)
        {
            throw new ClientNotConfiguredException();
        }

        CreatedInvoice = invoice;
        return Task.FromResult(ToResponse(invoice));
    }

    public Task<InvoiceResponseDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        InvoiceResponseDto? invoice = id == InvoiceId ? ToResponse(CreateInvoice()) : null;
        return Task.FromResult(invoice);
    }

    public Task<InvoiceResponseDto?> UpdateAsync(
        Guid id,
        InvoiceDto invoice,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        UpdatedInvoiceId = id;
        UpdatedInvoice = invoice;
        InvoiceResponseDto? response = id == InvoiceId ? ToResponse(invoice) : null;
        return Task.FromResult(response);
    }

    public Task<InvoiceResponseDto?> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StatusUpdatedInvoiceId = id;
        UpdatedStatus = status;
        InvoiceResponseDto? response = id == InvoiceId ? ToResponse(CreateInvoice()) : null;
        return Task.FromResult(response);
    }

    public Task<InvoiceOptionsDto> GetOptionsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new InvoiceOptionsDto([], []));
    }

    private static InvoiceDto CreateInvoice()
    {
        return new InvoiceDto(
            "Monthly bookkeeping",
            125.50m,
            "GBP",
            "Completed October records.",
            "Acme Ltd",
            "Retainer");
    }

    private static InvoiceResponseDto ToResponse(InvoiceDto invoice)
    {
        return new InvoiceResponseDto(
            InvoiceId,
            invoice.Description,
            invoice.Value,
            invoice.Currency,
            invoice.Comments ?? string.Empty,
            invoice.Client,
            invoice.Project,
            new DateTimeOffset(2026, 10, 18, 12, 30, 0, TimeSpan.Zero),
            false);
    }
}
