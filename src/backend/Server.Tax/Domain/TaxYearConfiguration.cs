using Server.Shared.Common;

namespace Server.Tax.Domain;

public sealed class TaxYearConfiguration : Entity
{
    public int StartYear { get; set; }

    public decimal EmploymentIncome { get; set; }

    public decimal EmploymentTaxPaid { get; set; }

    public decimal PensionContributions { get; set; }

    public decimal AllowableExpenses { get; set; }

    public decimal AdditionalAllowances { get; set; }

    public decimal PersonalAllowance { get; set; }

    public decimal PersonalAllowanceTaperThreshold { get; set; }

    public decimal BasicRateLimit { get; set; }

    public decimal HigherRateLimit { get; set; }

    public decimal BasicRate { get; set; }

    public decimal HigherRate { get; set; }

    public decimal AdditionalRate { get; set; }

    public decimal NationalInsuranceLowerLimit { get; set; }

    public decimal NationalInsuranceUpperLimit { get; set; }

    public decimal NationalInsuranceMainRate { get; set; }

    public decimal NationalInsuranceAdditionalRate { get; set; }
}
