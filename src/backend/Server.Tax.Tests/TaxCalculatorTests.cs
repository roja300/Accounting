using Server.Tax.Application.Dtos;
using Server.Tax.Application.Services;

namespace Server.Tax.Tests;

public sealed class TaxCalculatorTests
{
    [Fact]
    public void ShouldCalculateIncomeTaxAndClassFourNationalInsurance_WhenProfitCrossesHigherRateBand()
    {
        var result = TaxCalculator.Calculate(60_000, Configuration());

        Assert.Equal(11_432, result.IncomeTax);
        Assert.Equal(2_456.60m, result.NationalInsurance);
        Assert.Equal(13_888.60m, result.AmountToPay);
    }

    [Fact]
    public void ShouldApplyEmploymentTaxAndConfiguredReliefs_WhenCalculatingAmountToPay()
    {
        var configuration = Configuration() with
        {
            EmploymentIncome = 20_000,
            EmploymentTaxPaid = 2_000,
            PensionContributions = 5_000,
            AllowableExpenses = 4_000,
            AdditionalAllowances = 1_000
        };

        var result = TaxCalculator.Calculate(30_000, configuration);

        Assert.Equal(5_486, result.IncomeTax);
        Assert.Equal(805.80m, result.NationalInsurance);
        Assert.Equal(4_291.80m, result.AmountToPay);
    }

    [Fact]
    public void ShouldTaperPersonalAllowance_WhenAdjustedIncomeExceedsThreshold()
    {
        var result = TaxCalculator.Calculate(130_000, Configuration());

        Assert.Equal(44_703, result.IncomeTax);
        Assert.Equal(3_856.60m, result.NationalInsurance);
        Assert.Equal(48_559.60m, result.AmountToPay);
    }

    private static TaxYearConfigurationDto Configuration() => new(
        0, 0, 0, 0, 0, 12_570, 100_000, 37_700, 125_140,
        0.20m, 0.40m, 0.45m, 12_570, 50_270, 0.06m, 0.02m);
}
