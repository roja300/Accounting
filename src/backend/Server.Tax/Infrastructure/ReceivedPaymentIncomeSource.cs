using Microsoft.EntityFrameworkCore;
using Server.Payments.Infrastructure;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Infrastructure;

public sealed class ReceivedPaymentIncomeSource(PaymentContext context) : IReceivedPaymentSource
{
    public async Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(
        CancellationToken cancellationToken)
    {
        var receivedDates = await context.Payments
            .AsNoTracking()
            .Where(payment => payment.Amount > 0 && payment.Currency == "GBP")
            .Select(payment => payment.TransactionAt)
            .ToListAsync(cancellationToken);
        return receivedDates
            .Select(date => TaxYearStart(DateOnly.FromDateTime(date.UtcDateTime)))
            .ToHashSet();
    }

    public async Task<decimal> GetReceivedTotalAsync(
        DateTimeOffset startsAt,
        DateTimeOffset endsBefore,
        CancellationToken cancellationToken) =>
        await context.Payments
            .AsNoTracking()
            .Where(payment =>
                payment.Amount > 0 &&
                payment.Currency == "GBP" &&
                payment.TransactionAt >= startsAt &&
                payment.TransactionAt < endsBefore)
            .SumAsync(payment => (decimal?)payment.Amount, cancellationToken) ?? 0;

    private static int TaxYearStart(DateOnly date) =>
        date >= new DateOnly(date.Year, 4, 6) ? date.Year : date.Year - 1;
}
