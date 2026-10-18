namespace Server.Invoices.Application.Validators;

public sealed record InvoiceValidationResult(IReadOnlyDictionary<string, string[]> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
