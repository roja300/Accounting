using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;

namespace Server.Api.Tests;

internal sealed class StubInvoiceService : IInvoiceService
{
    public static readonly Guid InvoiceId =
        Guid.Parse("1f184969-9587-4103-b6ee-80a467669fa8");

    public InvoiceDto? CreatedInvoice { get; private set; }

    public Task<InvoiceResponseDto> CreateAsync(
        InvoiceDto invoice,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
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
            new DateTimeOffset(2026, 10, 18, 12, 30, 0, TimeSpan.Zero));
    }
}
