namespace Server.Tax.Application.Dtos;

public sealed record TaxYearConfigurationDto(
    decimal EmploymentIncome,
    decimal EmploymentTaxPaid,
    decimal PensionContributions,
    decimal AllowableExpenses,
    decimal AdditionalAllowances,
    decimal PersonalAllowance,
    decimal PersonalAllowanceTaperThreshold,
    decimal BasicRateLimit,
    decimal HigherRateLimit,
    decimal BasicRate,
    decimal HigherRate,
    decimal AdditionalRate,
    decimal NationalInsuranceLowerLimit,
    decimal NationalInsuranceUpperLimit,
    decimal NationalInsuranceMainRate,
    decimal NationalInsuranceAdditionalRate);

public sealed record TaxYearDto(
    int StartYear,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Status,
    decimal ExpectedPay,
    decimal TotalEarned,
    decimal EstimatedIncomeTax,
    decimal EstimatedNationalInsurance,
    decimal AmountToPay,
    TaxYearConfigurationDto Configuration);
