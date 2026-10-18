using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Validators;

namespace Server.Invoices.Tests;

public class InvoiceValidatorTests
{
    private readonly InvoiceValidator _validator = new();

    [Fact]
    public void ShouldAcceptInvoice_WhenAllFieldsAreValid()
    {
        var result = _validator.Validate(CreateValidInvoice());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ShouldReturnErrorsForEveryField_WhenInvoiceIsEmpty()
    {
        var invoice = new InvoiceDto(" ", 0, "", " ", "", " ");

        var result = _validator.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Equal(
            ["description", "value", "currency", "comments", "client", "project"],
            result.Errors.Keys);
    }

    [Fact]
    public void ShouldReturnErrors_WhenTextFieldsExceedTheirMaximumLengths()
    {
        var invoice = CreateValidInvoice() with
        {
            Description = new string('d', InvoiceValidator.DescriptionMaximumLength + 1),
            Comments = new string('c', InvoiceValidator.CommentsMaximumLength + 1),
            Client = new string('x', InvoiceValidator.NameMaximumLength + 1),
            Project = new string('y', InvoiceValidator.NameMaximumLength + 1)
        };

        var result = _validator.Validate(invoice);

        Assert.Equal(4, result.Errors.Count);
        Assert.Contains("description", result.Errors.Keys);
        Assert.Contains("comments", result.Errors.Keys);
        Assert.Contains("client", result.Errors.Keys);
        Assert.Contains("project", result.Errors.Keys);
    }

    [Theory]
    [InlineData(-1, "GBP")]
    [InlineData(10.001, "GBP")]
    [InlineData(10, "CAD")]
    public void ShouldReturnAnError_WhenMoneyDetailsAreInvalid(
        decimal value,
        string currency)
    {
        var invoice = CreateValidInvoice() with { Value = value, Currency = currency };

        var result = _validator.Validate(invoice);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    private static InvoiceDto CreateValidInvoice()
    {
        return new InvoiceDto(
            "Monthly bookkeeping",
            125.50m,
            "GBP",
            "Completed October records.",
            "Acme Ltd",
            "Retainer");
    }
}
