namespace Server.Invoices.Infrastructure.Abstractions;

public sealed record InvoiceQuery(
    DateTimeOffset? CreatedOnOrAfter,
    DateTimeOffset? CreatedOnOrBefore,
    string? NormalizedClient,
    string? NormalizedProject,
    string? DescriptionSearch,
    long Offset,
    int Limit);
