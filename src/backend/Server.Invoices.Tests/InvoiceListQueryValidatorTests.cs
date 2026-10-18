using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Validators;

namespace Server.Invoices.Tests;

public class InvoiceListQueryValidatorTests
{
    private readonly InvoiceListQueryValidator _validator = new();

    [Fact]
    public void ShouldAcceptQuery_WhenFiltersAndPaginationAreValid()
    {
        var query = new InvoiceListQueryDto
        {
            DateAfter = new DateOnly(2026, 10, 1),
            DateBefore = new DateOnly(2026, 10, 31),
            Client = "Acme Ltd",
            Project = "Retainer",
            Search = "bookkeeping",
            Page = 2,
            PageSize = 9
        };

        var result = _validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ShouldRejectQuery_WhenDateRangeOrPaginationIsInvalid()
    {
        var query = new InvoiceListQueryDto
        {
            DateAfter = new DateOnly(2026, 10, 31),
            DateBefore = new DateOnly(2026, 10, 1),
            Page = 0,
            PageSize = InvoiceListQueryValidator.MaximumPageSize + 1
        };

        var result = _validator.Validate(query);

        Assert.Contains("dateRange", result.Errors.Keys);
        Assert.Contains("page", result.Errors.Keys);
        Assert.Contains("pageSize", result.Errors.Keys);
    }

    [Fact]
    public void ShouldRejectQuery_WhenTextFilterIsTooLong()
    {
        var query = new InvoiceListQueryDto
        {
            Client = new string('c', InvoiceValidator.NameMaximumLength + 1),
            Project = new string('p', InvoiceValidator.NameMaximumLength + 1),
            Search = new string('s', InvoiceValidator.DescriptionMaximumLength + 1)
        };

        var result = _validator.Validate(query);

        Assert.Contains("client", result.Errors.Keys);
        Assert.Contains("project", result.Errors.Keys);
        Assert.Contains("search", result.Errors.Keys);
    }
}
