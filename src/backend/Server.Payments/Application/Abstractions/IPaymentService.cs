using Server.Payments.Application.Dtos;

namespace Server.Payments.Application.Abstractions;

public interface IPaymentService
{
    Task<PaymentPageDto> GetPageAsync(PaymentListQueryDto query, CancellationToken cancellationToken);

    Task<IReadOnlyList<PaymentImportDto>> GetImportsAsync(CancellationToken cancellationToken);

    Task<PaymentImportResultDto> ImportAsync(Stream csv, string fileName, CancellationToken cancellationToken);

    Task<int> DeleteAsync(DeletePaymentsDto request, CancellationToken cancellationToken);
}
