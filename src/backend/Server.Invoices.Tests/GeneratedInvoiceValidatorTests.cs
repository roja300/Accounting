using Server.Invoices.Application.Dtos;
using Server.Invoices.Application.Validators;

namespace Server.Invoices.Tests;

public class GeneratedInvoiceValidatorTests
{
    [Fact]
    public void ShouldRejectGeneration_WhenDueDateIsBeforeIssueDate()
    {
        var validator = new GeneratedInvoiceValidator();

        var result = validator.Validate(new GenerateInvoiceDto(
            Guid.NewGuid(),
            null,
            "GBP",
            new DateOnly(2026, 10, 19),
            new DateOnly(2026, 10, 18),
            null));

        Assert.False(result.IsValid);
        Assert.Contains("dueDate", result.Errors.Keys);
    }
}
