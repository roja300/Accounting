using Server.Tax.Application.Dtos;

namespace Server.Tax.Application.Services;

public sealed record TaxCalculation(
    decimal IncomeTax,
    decimal NationalInsurance,
    decimal AmountToPay);

public static class TaxCalculator
{
    public static TaxCalculation Calculate(
        decimal earned,
        TaxYearConfigurationDto configuration)
    {
        var selfEmployedProfit = Math.Max(0, earned - configuration.AllowableExpenses);
        var adjustedIncome = Math.Max(
            0,
            selfEmployedProfit + configuration.EmploymentIncome - configuration.PensionContributions);
        var allowanceReduction = Math.Max(
            0,
            (adjustedIncome - configuration.PersonalAllowanceTaperThreshold) / 2);
        var personalAllowance = Math.Max(
            0,
            configuration.PersonalAllowance - allowanceReduction);
        var taxableIncome = Math.Max(
            0,
            adjustedIncome - personalAllowance - configuration.AdditionalAllowances);

        var incomeTax = TaxByBands(taxableIncome, configuration);
        var nationalInsurance = NationalInsurance(selfEmployedProfit, configuration);
        var amountToPay = Math.Max(
            0,
            incomeTax + nationalInsurance - configuration.EmploymentTaxPaid);

        return new TaxCalculation(
            Money(incomeTax),
            Money(nationalInsurance),
            Money(amountToPay));
    }

    private static decimal TaxByBands(
        decimal taxableIncome,
        TaxYearConfigurationDto configuration)
    {
        var basicBand = Math.Min(taxableIncome, configuration.BasicRateLimit);
        var higherBand = Math.Min(
            Math.Max(0, taxableIncome - configuration.BasicRateLimit),
            configuration.HigherRateLimit - configuration.BasicRateLimit);
        var additionalBand = Math.Max(0, taxableIncome - configuration.HigherRateLimit);
        return basicBand * configuration.BasicRate
            + higherBand * configuration.HigherRate
            + additionalBand * configuration.AdditionalRate;
    }

    private static decimal NationalInsurance(
        decimal profit,
        TaxYearConfigurationDto configuration)
    {
        var mainBand = Math.Min(
            Math.Max(0, profit - configuration.NationalInsuranceLowerLimit),
            configuration.NationalInsuranceUpperLimit - configuration.NationalInsuranceLowerLimit);
        var additionalBand = Math.Max(
            0,
            profit - configuration.NationalInsuranceUpperLimit);
        return mainBand * configuration.NationalInsuranceMainRate
            + additionalBand * configuration.NationalInsuranceAdditionalRate;
    }

    private static decimal Money(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
