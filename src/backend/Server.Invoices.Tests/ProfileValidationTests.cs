using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Validators;

namespace Server.Invoices.Tests;

public class ProfileValidationTests
{
    [Fact]
    public void ShouldRequireCompanyNumber_WhenBusinessIsLimitedCompany()
    {
        var profile = new BusinessProfileDto(
            "limitedCompany",
            "Example Ltd",
            null,
            "owner@example.test",
            null,
            "1 High Street",
            null,
            "London",
            null,
            "SW1A 1AA",
            null,
            null);

        var result = ProfileValidation.Validate(profile);

        Assert.False(result.IsValid);
        Assert.Contains("companyNumber", result.Errors.Keys);
    }

    [Fact]
    public void ShouldAcceptInvoiceReadyCustomer_WhenRequiredDetailsAreProvided()
    {
        var client = new ClientDto(
            "Acme Ltd",
            "Alex Smith",
            "accounts@acme.test",
            null,
            "2 Market Road",
            null,
            "Manchester",
            null,
            "M1 1AA");

        var result = ProfileValidation.Validate(client);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ShouldRejectCustomer_WhenEmailIsInvalid()
    {
        var client = new ClientDto(
            "Acme Ltd",
            null,
            "not-an-email",
            null,
            "2 Market Road",
            null,
            "Manchester",
            null,
            "M1 1AA");

        var result = ProfileValidation.Validate(client);

        Assert.False(result.IsValid);
        Assert.Contains("email", result.Errors.Keys);
    }
}
