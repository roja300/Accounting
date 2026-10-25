using Microsoft.EntityFrameworkCore;
using Server.Payments.Domain;
using Server.Payments.Infrastructure.Abstractions;

namespace Server.Payments.Infrastructure;

public sealed class PaymentContext(DbContextOptions<PaymentContext> options)
    : DbContext(options), IPaymentContext
{
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PaymentImport> PaymentImports => Set<PaymentImport>();

    public async Task<PaymentQueryResult> QueryPaymentsAsync(PaymentQuery query, CancellationToken cancellationToken)
    {
        var payments = Payments.AsNoTracking().Include(payment => payment.Import).AsQueryable();
        if (query.TransactionAtOrAfter is not null)
            payments = payments.Where(payment => payment.TransactionAt >= query.TransactionAtOrAfter);
        if (query.TransactionAtOrBefore is not null)
            payments = payments.Where(payment => payment.TransactionAt <= query.TransactionAtOrBefore);
        if (query.ImportId is not null)
            payments = payments.Where(payment => payment.ImportId == query.ImportId);
        if (query.Direction == "incoming")
            payments = payments.Where(payment => payment.Amount > 0);
        if (query.Direction == "outgoing")
            payments = payments.Where(payment => payment.Amount < 0);
        if (query.Search is not null)
        {
            var pattern = $"%{EscapeLikePattern(query.Search)}%";
            payments = payments.Where(payment =>
                EF.Functions.ILike(payment.Description, pattern, "\\") ||
                EF.Functions.ILike(payment.Counterparty, pattern, "\\") ||
                EF.Functions.ILike(payment.Reference, pattern, "\\"));
        }

        var totalItems = await payments.CountAsync(cancellationToken);
        var items = await payments
            .OrderByDescending(payment => payment.TransactionAt)
            .ThenByDescending(payment => payment.Id)
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);
        return new PaymentQueryResult(items, totalItems);
    }

    public async Task<IReadOnlyList<PaymentImport>> GetImportsAsync(CancellationToken cancellationToken)
    {
        return await PaymentImports.AsNoTracking().OrderByDescending(paymentImport => paymentImport.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddImportAsync(PaymentImport paymentImport, CancellationToken cancellationToken)
    {
        await PaymentImports.AddAsync(paymentImport, cancellationToken);
    }

    public async Task<int> DeletePaymentsAsync(PaymentDeleteQuery query, CancellationToken cancellationToken)
    {
        if (query.ImportId is not null)
            return await PaymentImports.Where(paymentImport => paymentImport.Id == query.ImportId).ExecuteDeleteAsync(cancellationToken);

        var payments = Payments.AsQueryable();
        if (!query.All)
        {
            if (query.TransactionAtOrAfter is not null)
                payments = payments.Where(payment => payment.TransactionAt >= query.TransactionAtOrAfter);
            if (query.TransactionAtOrBefore is not null)
                payments = payments.Where(payment => payment.TransactionAt <= query.TransactionAtOrBefore);
        }
        var deleted = await payments.ExecuteDeleteAsync(cancellationToken);
        await PaymentImports.Where(paymentImport => !paymentImport.Payments.Any()).ExecuteDeleteAsync(cancellationToken);
        return deleted;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentContext).Assembly);
    }

    private static string EscapeLikePattern(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal);
}
