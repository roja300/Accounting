using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;

namespace Server.Invoices.Application.Abstractions;

public interface IExportedInvoiceService
{
    Task<GeneratedInvoicePageDto> GetPageAsync(
        GeneratedInvoiceListQueryDto query,
        CancellationToken cancellationToken);

    Task<GenerateInvoiceOptionsDto> GetOptionsAsync(CancellationToken cancellationToken);

    Task<GeneratedInvoiceDto> GenerateAsync(
        GenerateInvoiceDto request,
        CancellationToken cancellationToken);

    Task<GeneratedInvoiceDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<GeneratedInvoiceDto?> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken);

    Task<GeneratedInvoice?> GetDocumentSourceAsync(Guid id, CancellationToken cancellationToken);
}

public interface IInvoiceDocumentService
{
    byte[] Generate(GeneratedInvoice invoice);
}
