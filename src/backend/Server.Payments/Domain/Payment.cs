using Server.Shared.Common;

namespace Server.Payments.Domain;

public sealed class Payment : Entity
{
    public Guid ImportId { get; set; }

    public PaymentImport Import { get; set; } = null!;

    public DateTimeOffset TransactionAt { get; set; }

    public required string Description { get; set; }

    public required string Counterparty { get; set; }

    public required string Reference { get; set; }

    public decimal Amount { get; set; }

    public decimal? Balance { get; set; }

    public required string Currency { get; set; }
}
