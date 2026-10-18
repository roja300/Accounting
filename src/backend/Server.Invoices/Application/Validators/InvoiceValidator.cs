using Server.Invoices.Application.Dtos;

namespace Server.Invoices.Application.Validators;

public sealed class InvoiceValidator
{
    public const int DescriptionMaximumLength = 200;
    public const int CommentsMaximumLength = 1_000;
    public const int NameMaximumLength = 100;
    public const decimal MaximumValue = 9_999_999_999.99m;

    private static readonly HashSet<string> SupportedCurrencies =
        new(["GBP", "EUR", "USD"], StringComparer.OrdinalIgnoreCase);

    public InvoiceValidationResult Validate(InvoiceDto invoice)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        ValidateRequiredText(
            errors,
            "description",
            invoice.Description,
            "Description",
            DescriptionMaximumLength);
        ValidateValue(errors, invoice.Value);
        ValidateCurrency(errors, invoice.Currency);
        ValidateRequiredText(
            errors,
            "comments",
            invoice.Comments,
            "Comments",
            CommentsMaximumLength);
        ValidateRequiredText(errors, "client", invoice.Client, "Client", NameMaximumLength);
        ValidateRequiredText(errors, "project", invoice.Project, "Project", NameMaximumLength);

        return new InvoiceValidationResult(errors);
    }

    private static void ValidateRequiredText(
        IDictionary<string, string[]> errors,
        string key,
        string? value,
        string displayName,
        int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[key] = [$"{displayName} is required."];
        }
        else if (value.Trim().Length > maximumLength)
        {
            errors[key] = [$"{displayName} must be {maximumLength} characters or fewer."];
        }
    }

    private static void ValidateValue(IDictionary<string, string[]> errors, decimal value)
    {
        if (value <= 0)
        {
            errors["value"] = ["Value must be greater than zero."];
        }
        else if (value > MaximumValue)
        {
            errors["value"] = [$"Value must not exceed {MaximumValue:0.00}."];
        }
        else if (decimal.Round(value, 2) != value)
        {
            errors["value"] = ["Value must have no more than two decimal places."];
        }
    }

    private static void ValidateCurrency(
        IDictionary<string, string[]> errors,
        string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            errors["currency"] = ["Currency is required."];
        }
        else if (!SupportedCurrencies.Contains(currency.Trim()))
        {
            errors["currency"] = ["Currency must be GBP, EUR, or USD."];
        }
    }
}
