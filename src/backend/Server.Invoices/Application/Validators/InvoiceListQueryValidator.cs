using Server.Invoices.Application.Dtos;

namespace Server.Invoices.Application.Validators;

public sealed class InvoiceListQueryValidator
{
    public const int MaximumPageSize = 50;

    public InvoiceValidationResult Validate(InvoiceListQueryDto query)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (query.Page < 1)
        {
            errors["page"] = ["Page must be at least 1."];
        }

        if (query.PageSize is < 1 or > MaximumPageSize)
        {
            errors["pageSize"] = [$"Page size must be between 1 and {MaximumPageSize}."];
        }

        if (query.DateAfter > query.DateBefore)
        {
            errors["dateRange"] = ["Date after must be on or before date before."];
        }

        ValidateOptionalText(
            errors,
            "client",
            query.Client,
            "Client",
            InvoiceValidator.NameMaximumLength);
        ValidateOptionalText(
            errors,
            "project",
            query.Project,
            "Project",
            InvoiceValidator.NameMaximumLength);
        ValidateOptionalText(
            errors,
            "search",
            query.Search,
            "Search",
            InvoiceValidator.DescriptionMaximumLength);

        return new InvoiceValidationResult(errors);
    }

    private static void ValidateOptionalText(
        IDictionary<string, string[]> errors,
        string key,
        string? value,
        string displayName,
        int maximumLength)
    {
        if (value?.Trim().Length > maximumLength)
        {
            errors[key] = [$"{displayName} must be {maximumLength} characters or fewer."];
        }
    }
}
