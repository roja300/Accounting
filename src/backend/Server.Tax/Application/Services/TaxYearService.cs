using Server.Tax.Application.Abstractions;
using Server.Tax.Application.Dtos;
using Server.Tax.Domain;
using Server.Tax.Infrastructure.Abstractions;

namespace Server.Tax.Application.Services;

public sealed class TaxYearService(
    ITaxYearRepository repository,
    ITaxIncomeSource incomeSource,
    TimeProvider timeProvider) : ITaxYearService
{
    public async Task<IReadOnlyList<TaxYearDto>> GetTaxYearsAsync(
        CancellationToken cancellationToken)
    {
        var configurations = await repository.GetConfigurationsAsync(cancellationToken);
        var configuredByYear = configurations.ToDictionary(configuration => configuration.StartYear);
        var years = new HashSet<int>(await incomeSource.GetActiveTaxYearsAsync(cancellationToken))
        {
            TaxYearStart(DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
        };
        years.UnionWith(configuredByYear.Keys);

        var cards = new List<TaxYearDto>();
        foreach (var year in years.OrderDescending())
        {
            configuredByYear.TryGetValue(year, out var configuration);
            cards.Add(await BuildAsync(year, configuration, cancellationToken));
        }

        return cards;
    }

    public async Task<TaxYearDto> ConfigureAsync(
        int startYear,
        TaxYearConfigurationDto request,
        CancellationToken cancellationToken)
    {
        Validate(startYear, request);
        var configuration = await repository.GetConfigurationAsync(startYear, cancellationToken);
        var now = timeProvider.GetUtcNow();
        if (configuration is null)
        {
            configuration = new TaxYearConfiguration
            {
                Id = Guid.NewGuid(),
                StartYear = startYear,
                CreatedAt = now
            };
            await repository.AddConfigurationAsync(configuration, cancellationToken);
        }

        Apply(configuration, request);
        configuration.UpdatedAt = now;
        await repository.SaveChangesAsync(cancellationToken);
        return await BuildAsync(startYear, configuration, cancellationToken);
    }

    private async Task<TaxYearDto> BuildAsync(
        int startYear,
        TaxYearConfiguration? entity,
        CancellationToken cancellationToken)
    {
        var startsOn = new DateOnly(startYear, 4, 6);
        var endsOn = new DateOnly(startYear + 1, 4, 5);
        var totals = await incomeSource.GetTotalsAsync(
            new DateTimeOffset(startsOn.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new DateTimeOffset(endsOn.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            cancellationToken);
        var configuration = entity is null ? Defaults() : Map(entity);
        var calculation = TaxCalculator.Calculate(totals.TotalEarned, configuration);

        return new TaxYearDto(
            startYear,
            startsOn,
            endsOn,
            entity is null ? "draft" : "configured",
            totals.ExpectedPay,
            totals.TotalEarned,
            calculation.IncomeTax,
            calculation.NationalInsurance,
            calculation.AmountToPay,
            configuration);
    }

    private static int TaxYearStart(DateOnly date) =>
        date >= new DateOnly(date.Year, 4, 6) ? date.Year : date.Year - 1;

    private static TaxYearConfigurationDto Defaults() => new(
        0,
        0,
        0,
        0,
        0,
        12_570,
        100_000,
        37_700,
        125_140,
        0.20m,
        0.40m,
        0.45m,
        12_570,
        50_270,
        0.06m,
        0.02m);

    private static TaxYearConfigurationDto Map(TaxYearConfiguration value) => new(
        value.EmploymentIncome,
        value.EmploymentTaxPaid,
        value.PensionContributions,
        value.AllowableExpenses,
        value.AdditionalAllowances,
        value.PersonalAllowance,
        value.PersonalAllowanceTaperThreshold,
        value.BasicRateLimit,
        value.HigherRateLimit,
        value.BasicRate,
        value.HigherRate,
        value.AdditionalRate,
        value.NationalInsuranceLowerLimit,
        value.NationalInsuranceUpperLimit,
        value.NationalInsuranceMainRate,
        value.NationalInsuranceAdditionalRate);

    private static void Apply(
        TaxYearConfiguration target,
        TaxYearConfigurationDto source)
    {
        target.EmploymentIncome = source.EmploymentIncome;
        target.EmploymentTaxPaid = source.EmploymentTaxPaid;
        target.PensionContributions = source.PensionContributions;
        target.AllowableExpenses = source.AllowableExpenses;
        target.AdditionalAllowances = source.AdditionalAllowances;
        target.PersonalAllowance = source.PersonalAllowance;
        target.PersonalAllowanceTaperThreshold = source.PersonalAllowanceTaperThreshold;
        target.BasicRateLimit = source.BasicRateLimit;
        target.HigherRateLimit = source.HigherRateLimit;
        target.BasicRate = source.BasicRate;
        target.HigherRate = source.HigherRate;
        target.AdditionalRate = source.AdditionalRate;
        target.NationalInsuranceLowerLimit = source.NationalInsuranceLowerLimit;
        target.NationalInsuranceUpperLimit = source.NationalInsuranceUpperLimit;
        target.NationalInsuranceMainRate = source.NationalInsuranceMainRate;
        target.NationalInsuranceAdditionalRate = source.NationalInsuranceAdditionalRate;
    }

    private static void Validate(int startYear, TaxYearConfigurationDto configuration)
    {
        if (startYear is < 2000 or > 9998)
            throw new TaxYearValidationException("startYear", "Enter a valid tax year.");

        var amounts = new[]
        {
            configuration.EmploymentIncome,
            configuration.EmploymentTaxPaid,
            configuration.PensionContributions,
            configuration.AllowableExpenses,
            configuration.AdditionalAllowances,
            configuration.PersonalAllowance,
            configuration.PersonalAllowanceTaperThreshold,
            configuration.BasicRateLimit,
            configuration.HigherRateLimit,
            configuration.NationalInsuranceLowerLimit,
            configuration.NationalInsuranceUpperLimit
        };
        var rates = new[]
        {
            configuration.BasicRate,
            configuration.HigherRate,
            configuration.AdditionalRate,
            configuration.NationalInsuranceMainRate,
            configuration.NationalInsuranceAdditionalRate
        };
        if (amounts.Any(amount => amount < 0))
            throw new TaxYearValidationException("configuration", "Amounts cannot be negative.");
        if (rates.Any(rate => rate is < 0 or > 1))
            throw new TaxYearValidationException("configuration", "Rates must be between 0 and 1.");
        if (configuration.HigherRateLimit < configuration.BasicRateLimit)
            throw new TaxYearValidationException("higherRateLimit", "The higher-rate limit must not be below the basic-rate limit.");
        if (configuration.NationalInsuranceUpperLimit < configuration.NationalInsuranceLowerLimit)
            throw new TaxYearValidationException("nationalInsuranceUpperLimit", "The upper National Insurance limit must not be below the lower limit.");
    }
}

public sealed class TaxYearValidationException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
