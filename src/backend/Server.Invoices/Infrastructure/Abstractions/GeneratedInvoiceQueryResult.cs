using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Abstractions;

public sealed record GeneratedInvoiceQueryResult(
    IReadOnlyList<GeneratedInvoice> Items,
    int TotalItems);
