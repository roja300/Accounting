namespace Server.Invoices.Infrastructure.Abstractions;

public sealed record GeneratedInvoiceQuery(
    DateOnly? DateAfter,
    DateOnly? DateBefore,
    Guid? ClientId,
    string? Status,
    string? Search,
    int Offset,
    int Limit);
