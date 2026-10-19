using Server.Invoices.Application.Dtos;

namespace Server.Invoices.Application.Abstractions;

public interface IInvoiceService
{
    Task<InvoicePageDto> GetPageAsync(
        InvoiceListQueryDto query,
        CancellationToken cancellationToken);

    Task<InvoiceResponseDto> CreateAsync(InvoiceDto invoice, CancellationToken cancellationToken);

    Task<InvoiceResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<InvoiceOptionsDto> GetOptionsAsync(CancellationToken cancellationToken);
}
