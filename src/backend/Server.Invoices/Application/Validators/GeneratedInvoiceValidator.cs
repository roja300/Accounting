using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;

namespace Server.Invoices.Application.Validators;

public sealed class GeneratedInvoiceValidator
{
    private static readonly HashSet<string> SupportedCurrencies =
        new(["GBP", "EUR", "USD"], StringComparer.OrdinalIgnoreCase);

    public InvoiceValidationResult Validate(GenerateInvoiceDto request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.ClientId == Guid.Empty)
            errors["clientId"] = ["Select a customer."];
        if (string.IsNullOrWhiteSpace(request.Currency) ||
            !SupportedCurrencies.Contains(request.Currency.Trim()))
            errors["currency"] = ["Currency must be GBP, EUR, or USD."];
        if (request.DueDate < request.IssueDate)
            errors["dueDate"] = ["Due date must be on or after the issue date."];
        if (request.Notes?.Trim().Length > 1_000)
            errors["notes"] = ["Notes must be 1000 characters or fewer."];
        return new InvoiceValidationResult(errors);
    }

    public InvoiceValidationResult Validate(GeneratedInvoiceListQueryDto query)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (query.Page < 1) errors["page"] = ["Page must be at least 1."];
        if (query.PageSize is < 1 or > 50)
            errors["pageSize"] = ["Page size must be between 1 and 50."];
        if (query.DateAfter > query.DateBefore)
            errors["dateRange"] = ["Date after must be on or before date before."];
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !GeneratedInvoiceStatuses.All.Contains(query.Status))
            errors["status"] = ["Status is not supported."];
        if (query.Search?.Trim().Length > 100)
            errors["search"] = ["Search must be 100 characters or fewer."];
        return new InvoiceValidationResult(errors);
    }
}
