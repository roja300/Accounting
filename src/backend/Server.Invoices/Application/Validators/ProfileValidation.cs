using System.Net.Mail;
using Server.Invoices.Application.Dtos;

namespace Server.Invoices.Application.Validators;

public static class ProfileValidation
{
    public static InvoiceValidationResult Validate(BusinessProfileDto profile)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, "businessType", profile.BusinessType, "Business type", 20);
        if (profile.BusinessType is not ("soleTrader" or "limitedCompany"))
        {
            errors["businessType"] = ["Business type must be soleTrader or limitedCompany."];
        }
        Required(errors, "legalName", profile.LegalName, "Legal name", 150);
        Optional(errors, "tradingName", profile.TradingName, "Trading name", 150);
        Required(errors, "email", profile.Email, "Email", 254);
        ValidateEmail(errors, profile.Email);
        Optional(errors, "phone", profile.Phone, "Phone", 30);
        Required(errors, "addressLine1", profile.AddressLine1, "Address line 1", 100);
        Optional(errors, "addressLine2", profile.AddressLine2, "Address line 2", 100);
        Required(errors, "city", profile.City, "Town or city", 100);
        Optional(errors, "county", profile.County, "County", 100);
        Required(errors, "postcode", profile.Postcode, "Postcode", 10);
        Optional(errors, "companyNumber", profile.CompanyNumber, "Company number", 8);
        Optional(errors, "vatNumber", profile.VatNumber, "VAT number", 12);

        if (profile.BusinessType == "limitedCompany" && string.IsNullOrWhiteSpace(profile.CompanyNumber))
        {
            errors["companyNumber"] = ["Company number is required for a limited company."];
        }

        return new InvoiceValidationResult(errors);
    }

    public static InvoiceValidationResult Validate(ClientDto client)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, "name", client.Name, "Customer name", 100);
        Optional(errors, "contactName", client.ContactName, "Contact name", 100);
        Required(errors, "email", client.Email, "Email", 254);
        ValidateEmail(errors, client.Email);
        Optional(errors, "phone", client.Phone, "Phone", 30);
        Required(errors, "addressLine1", client.AddressLine1, "Address line 1", 100);
        Optional(errors, "addressLine2", client.AddressLine2, "Address line 2", 100);
        Required(errors, "city", client.City, "Town or city", 100);
        Optional(errors, "county", client.County, "County", 100);
        Required(errors, "postcode", client.Postcode, "Postcode", 10);
        return new InvoiceValidationResult(errors);
    }

    private static void Required(
        IDictionary<string, string[]> errors,
        string key,
        string? value,
        string label,
        int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[key] = [$"{label} is required."];
        }
        else if (value.Trim().Length > maximumLength)
        {
            errors[key] = [$"{label} must be {maximumLength} characters or fewer."];
        }
    }

    private static void Optional(
        IDictionary<string, string[]> errors,
        string key,
        string? value,
        string label,
        int maximumLength)
    {
        if (value?.Trim().Length > maximumLength)
        {
            errors[key] = [$"{label} must be {maximumLength} characters or fewer."];
        }
    }

    private static void ValidateEmail(
        IDictionary<string, string[]> errors,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !MailAddress.TryCreate(value.Trim(), out _))
        {
            errors["email"] = ["Enter a valid email address."];
        }
    }
}
