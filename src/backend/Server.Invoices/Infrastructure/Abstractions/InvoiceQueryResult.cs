using Server.Invoices.Domain;

namespace Server.Invoices.Infrastructure.Abstractions;

public sealed record InvoiceQueryResult(
    IReadOnlyList<Invoice> Items,
    int TotalItems);
