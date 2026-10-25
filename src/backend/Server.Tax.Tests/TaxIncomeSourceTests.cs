using Server.Tax.Infrastructure;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Tests;

public sealed class TaxIncomeSourceTests
{
    private static readonly DateTimeOffset StartsAt = new(2026, 4, 6, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset EndsBefore = new(2027, 4, 6, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ShouldUseReceivedPaymentsForTotalEarned_WhenBuildingTaxIncomeTotals()
    {
        var source = new TaxIncomeSource(
            new ExpectedPaySource(52_000, new HashSet<int> { 2026 }),
            new ReceivedPaymentSource(40_000, new HashSet<int> { 2025, 2026 }));

        var totals = await source.GetTotalsAsync(StartsAt, EndsBefore, CancellationToken.None);
        var years = await source.GetActiveTaxYearsAsync(CancellationToken.None);

        Assert.Equal(52_000, totals.ExpectedPay);
        Assert.Equal(40_000, totals.TotalEarned);
        Assert.Equal([2025, 2026], years.Order());
    }

    private sealed class ExpectedPaySource(
        decimal total,
        IReadOnlySet<int> years) : IExpectedPaySource
    {
        public Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(years);

        public Task<decimal> GetExpectedPayAsync(
            DateTimeOffset startsAt,
            DateTimeOffset endsBefore,
            CancellationToken cancellationToken) => Task.FromResult(total);
    }

    private sealed class ReceivedPaymentSource(
        decimal total,
        IReadOnlySet<int> years) : IReceivedPaymentSource
    {
        public Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(years);

        public Task<decimal> GetReceivedTotalAsync(
            DateTimeOffset startsAt,
            DateTimeOffset endsBefore,
            CancellationToken cancellationToken) => Task.FromResult(total);
    }
}
