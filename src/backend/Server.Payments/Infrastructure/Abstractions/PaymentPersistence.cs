using Server.Payments.Domain;

namespace Server.Payments.Infrastructure.Abstractions;

public sealed record PaymentQuery(
    DateTimeOffset? TransactionAtOrAfter,
    DateTimeOffset? TransactionAtOrBefore,
    string? Search,
    string? Direction,
    Guid? ImportId,
    int Offset,
    int Limit);

public sealed record PaymentQueryResult(IReadOnlyList<Payment> Items, int TotalItems);

public sealed record PaymentDeleteQuery(
    bool All,
    DateTimeOffset? TransactionAtOrAfter,
    DateTimeOffset? TransactionAtOrBefore,
    Guid? ImportId);

public interface IPaymentContext
{
    Task<PaymentQueryResult> QueryPaymentsAsync(PaymentQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<PaymentImport>> GetImportsAsync(CancellationToken cancellationToken);

    Task AddImportAsync(PaymentImport paymentImport, CancellationToken cancellationToken);

    Task<int> DeletePaymentsAsync(PaymentDeleteQuery query, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
