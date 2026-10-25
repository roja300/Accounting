using Server.Tax.Application.Dtos;
using Server.Tax.Application.Services;
using Server.Tax.Domain;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Tests;

public sealed class TaxYearServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ShouldReturnCurrentAndActiveTaxYearsAsCards()
    {
        var repository = new TaxYearRepository();
        var income = new TaxIncomeSource(new HashSet<int> { 2024 }, new TaxIncomeTotals(20_000, 15_000));
        var service = new TaxYearService(repository, income, new FixedTimeProvider(Now));

        var result = await service.GetTaxYearsAsync(CancellationToken.None);

        Assert.Equal([2026, 2024], result.Select(card => card.StartYear));
        Assert.All(result, card => Assert.Equal("draft", card.Status));
        Assert.Equal(new DateOnly(2024, 4, 6), result[1].StartsOn);
        Assert.Equal(new DateOnly(2025, 4, 5), result[1].EndsOn);
    }

    [Fact]
    public async Task ShouldPersistConfigurationAndReturnConfiguredCalculation_WhenTaxYearIsSaved()
    {
        var repository = new TaxYearRepository();
        var income = new TaxIncomeSource(new HashSet<int>(), new TaxIncomeTotals(50_000, 40_000));
        var service = new TaxYearService(repository, income, new FixedTimeProvider(Now));
        var configuration = Configuration() with { AllowableExpenses = 2_000 };

        var result = await service.ConfigureAsync(2026, configuration, CancellationToken.None);

        Assert.Equal("configured", result.Status);
        Assert.Equal(50_000, result.ExpectedPay);
        Assert.Equal(40_000, result.TotalEarned);
        Assert.NotNull(repository.Configuration);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ShouldRejectInvalidRateLimits_WhenConfigurationIsInvalid()
    {
        var service = new TaxYearService(
            new TaxYearRepository(),
            new TaxIncomeSource(new HashSet<int>(), new TaxIncomeTotals(0, 0)),
            new FixedTimeProvider(Now));

        var exception = await Assert.ThrowsAsync<TaxYearValidationException>(() =>
            service.ConfigureAsync(
                2026,
                Configuration() with { HigherRateLimit = 30_000 },
                CancellationToken.None));

        Assert.Equal("higherRateLimit", exception.Field);
    }

    private static TaxYearConfigurationDto Configuration() => new(
        0, 0, 0, 0, 0, 12_570, 100_000, 37_700, 125_140,
        0.20m, 0.40m, 0.45m, 12_570, 50_270, 0.06m, 0.02m);

    private sealed class TaxYearRepository : ITaxYearRepository
    {
        public TaxYearConfiguration? Configuration { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<IReadOnlyList<TaxYearConfiguration>> GetConfigurationsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TaxYearConfiguration>>(Configuration is null ? [] : [Configuration]);

        public Task<TaxYearConfiguration?> GetConfigurationAsync(int startYear, CancellationToken cancellationToken) =>
            Task.FromResult(Configuration?.StartYear == startYear ? Configuration : null);

        public Task AddConfigurationAsync(TaxYearConfiguration configuration, CancellationToken cancellationToken)
        {
            Configuration = configuration;
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }

    private sealed class TaxIncomeSource(
        IReadOnlySet<int> years,
        TaxIncomeTotals totals) : ITaxIncomeSource
    {
        public Task<IReadOnlySet<int>> GetActiveTaxYearsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(years);

        public Task<TaxIncomeTotals> GetTotalsAsync(
            DateTimeOffset startsAt,
            DateTimeOffset endsBefore,
            CancellationToken cancellationToken) => Task.FromResult(totals);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
