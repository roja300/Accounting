using Microsoft.EntityFrameworkCore;
using Server.Invoices.Domain;
using Server.Invoices.Infrastructure;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Infrastructure;

public sealed class InvoiceTaxIncomeSource(InvoiceContext context) : ITaxIncomeSource
{
    public async Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(
        CancellationToken cancellationToken)
    {
        var workDates = await context.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.Status != WorkRecordStatuses.Deleted && invoice.Currency == "GBP")
            .Select(invoice => invoice.CreatedAt)
            .ToListAsync(cancellationToken);
        var paidDates = await context.GeneratedInvoices
            .AsNoTracking()
            .Where(invoice => invoice.Status == GeneratedInvoiceStatuses.PaymentReceived && invoice.Currency == "GBP")
            .Select(invoice => invoice.UpdatedAt)
            .ToListAsync(cancellationToken);

        return workDates.Concat(paidDates)
            .Select(date => TaxYearStart(DateOnly.FromDateTime(date.UtcDateTime)))
            .ToHashSet();
    }

    public async Task<TaxIncomeTotals> GetTotalsAsync(
        DateTimeOffset startsAt,
        DateTimeOffset endsBefore,
        CancellationToken cancellationToken)
    {
        var expected = await context.Invoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.Status != WorkRecordStatuses.Deleted &&
                invoice.Currency == "GBP" &&
                invoice.CreatedAt >= startsAt &&
                invoice.CreatedAt < endsBefore)
            .SumAsync(invoice => (decimal?)invoice.Value, cancellationToken) ?? 0;
        var earned = await context.GeneratedInvoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.Status == GeneratedInvoiceStatuses.PaymentReceived &&
                invoice.Currency == "GBP" &&
                invoice.UpdatedAt >= startsAt &&
                invoice.UpdatedAt < endsBefore)
            .SumAsync(invoice => (decimal?)invoice.Subtotal, cancellationToken) ?? 0;
        return new TaxIncomeTotals(expected, earned);
    }

    private static int TaxYearStart(DateOnly date) =>
        date >= new DateOnly(date.Year, 4, 6) ? date.Year : date.Year - 1;
}
