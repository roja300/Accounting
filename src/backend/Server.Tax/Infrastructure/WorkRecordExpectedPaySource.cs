using Microsoft.EntityFrameworkCore;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Infrastructure;

public sealed class WorkRecordExpectedPaySource(InvoiceContext context) : IExpectedPaySource
{
    public async Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(
        CancellationToken cancellationToken)
    {
        var workDates = await context.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.Status != WorkRecordStatuses.Deleted &&
                invoice.Currency == "GBP")
            .Select(invoice => invoice.CreatedAt)
            .ToListAsync(cancellationToken);
        return workDates
            .Select(date => TaxYearStart(DateOnly.FromDateTime(date.UtcDateTime)))
            .ToHashSet();
    }

    public async Task<decimal> GetExpectedPayAsync(
        DateTimeOffset startsAt,
        DateTimeOffset endsBefore,
        CancellationToken cancellationToken) =>
        await context.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.Status != WorkRecordStatuses.Deleted &&
                invoice.Currency == "GBP" &&
                invoice.CreatedAt >= startsAt &&
                invoice.CreatedAt < endsBefore)
            .SumAsync(invoice => (decimal?)invoice.Value, cancellationToken) ?? 0;

    private static int TaxYearStart(DateOnly date) =>
        date >= new DateOnly(date.Year, 4, 6) ? date.Year : date.Year - 1;
}
