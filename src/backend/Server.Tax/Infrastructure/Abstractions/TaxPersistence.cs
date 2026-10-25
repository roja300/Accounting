using Server.Tax.Domain;

namespace Server.Tax.Infrastructure.Abstractions;

public sealed record TaxIncomeTotals(decimal ExpectedPay, decimal TotalEarned);

public interface ITaxIncomeSource
{
    Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(CancellationToken cancellationToken);

    Task<TaxIncomeTotals> GetTotalsAsync(
        DateTimeOffset startsAt,
        DateTimeOffset endsBefore,
        CancellationToken cancellationToken);
}

public interface IExpectedPaySource
{
    Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(CancellationToken cancellationToken);

    Task<decimal> GetExpectedPayAsync(
        DateTimeOffset startsAt,
        DateTimeOffset endsBefore,
        CancellationToken cancellationToken);
}

public interface IReceivedPaymentSource
{
    Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(CancellationToken cancellationToken);

    Task<decimal> GetReceivedTotalAsync(
        DateTimeOffset startsAt,
        DateTimeOffset endsBefore,
        CancellationToken cancellationToken);
}

public interface ITaxYearRepository
{
    Task<IReadOnlyList<TaxYearConfiguration>> GetConfigurationsAsync(CancellationToken cancellationToken);

    Task<TaxYearConfiguration?> GetConfigurationAsync(int startYear, CancellationToken cancellationToken);

    Task AddConfigurationAsync(TaxYearConfiguration configuration, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
