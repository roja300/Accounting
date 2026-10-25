using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Infrastructure;

public sealed class TaxIncomeSource(
    IExpectedPaySource expectedPaySource,
    IReceivedPaymentSource receivedPaymentSource) : ITaxIncomeSource
{
    public async Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(
        CancellationToken cancellationToken)
    {
        var expectedYears = await expectedPaySource.GetActiveTaxYearsAsync(cancellationToken);
        var receivedYears = await receivedPaymentSource.GetActiveTaxYearsAsync(cancellationToken);
        return expectedYears.Concat(receivedYears).ToHashSet();
    }

    public async Task<TaxIncomeTotals> GetTotalsAsync(
        DateTimeOffset startsAt,
        DateTimeOffset endsBefore,
        CancellationToken cancellationToken)
    {
        var expectedPay = await expectedPaySource.GetExpectedPayAsync(
            startsAt,
            endsBefore,
            cancellationToken);
        var totalEarned = await receivedPaymentSource.GetReceivedTotalAsync(
            startsAt,
            endsBefore,
            cancellationToken);
        return new TaxIncomeTotals(expectedPay, totalEarned);
    }
}
