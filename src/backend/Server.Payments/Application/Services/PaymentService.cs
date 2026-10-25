using Server.Payments.Application.Abstractions;
using Server.Payments.Application.Dtos;
using Server.Payments.Application.Importing;
using Server.Payments.Domain;
using Server.Payments.Infrastructure.Abstractions;

namespace Server.Payments.Application.Services;

public sealed class PaymentService(
    IPaymentContext context,
    CsvPaymentParser parser,
    TimeProvider timeProvider) : IPaymentService
{
    public async Task<PaymentPageDto> GetPageAsync(
        PaymentListQueryDto query,
        CancellationToken cancellationToken)
    {
        var persistenceQuery = new PaymentQuery(
            StartOfDay(query.DateAfter),
            EndOfDay(query.DateBefore),
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.Direction,
            query.ImportId,
            (query.Page - 1) * query.PageSize,
            query.PageSize);
        var result = await context.QueryPaymentsAsync(persistenceQuery, cancellationToken);
        var totalPages = result.TotalItems == 0 ? 0 : (int)Math.Ceiling((double)result.TotalItems / query.PageSize);
        return new PaymentPageDto(result.Items.Select(Map).ToList(), query.Page, query.PageSize, result.TotalItems, totalPages);
    }

    public async Task<IReadOnlyList<PaymentImportDto>> GetImportsAsync(CancellationToken cancellationToken)
    {
        var imports = await context.GetImportsAsync(cancellationToken);
        return imports.Select(Map).ToList();
    }

    public async Task<PaymentImportResultDto> ImportAsync(
        Stream csv,
        string fileName,
        CancellationToken cancellationToken)
    {
        var parsed = await parser.ParseAsync(csv, cancellationToken);
        if (!parsed.IsValid) return new PaymentImportResultDto(null, parsed.Errors);

        var now = timeProvider.GetUtcNow();
        var paymentImport = new PaymentImport
        {
            Id = Guid.NewGuid(),
            FileName = Path.GetFileName(fileName),
            RowCount = parsed.Payments.Count,
            CreatedAt = now,
            UpdatedAt = now
        };
        foreach (var parsedPayment in parsed.Payments)
        {
            paymentImport.Payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                ImportId = paymentImport.Id,
                Import = paymentImport,
                TransactionAt = parsedPayment.TransactionAt.ToUniversalTime(),
                Description = parsedPayment.Description,
                Counterparty = parsedPayment.Counterparty,
                Reference = parsedPayment.Reference,
                Amount = parsedPayment.Amount,
                Balance = parsedPayment.Balance,
                Currency = parsedPayment.Currency,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await context.AddImportAsync(paymentImport, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return new PaymentImportResultDto(Map(paymentImport), parsed.Errors);
    }

    public Task<int> DeleteAsync(DeletePaymentsDto request, CancellationToken cancellationToken)
    {
        return context.DeletePaymentsAsync(new PaymentDeleteQuery(
            request.All,
            StartOfDay(request.DateAfter),
            EndOfDay(request.DateBefore),
            request.ImportId), cancellationToken);
    }

    private static PaymentDto Map(Payment payment) => new(
        payment.Id,
        payment.ImportId,
        payment.Import.FileName,
        payment.TransactionAt,
        payment.Description,
        payment.Counterparty,
        payment.Reference,
        payment.Amount,
        payment.Balance,
        payment.Currency);

    private static PaymentImportDto Map(PaymentImport paymentImport) => new(
        paymentImport.Id,
        paymentImport.FileName,
        paymentImport.RowCount,
        paymentImport.CreatedAt);

    private static DateTimeOffset? StartOfDay(DateOnly? date) => date is null
        ? null
        : new DateTimeOffset(date.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static DateTimeOffset? EndOfDay(DateOnly? date) => date is null
        ? null
        : new DateTimeOffset(date.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
}
