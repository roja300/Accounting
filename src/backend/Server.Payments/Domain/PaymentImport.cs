using Server.Shared.Common;

namespace Server.Payments.Domain;

public sealed class PaymentImport : Entity
{
    public required string FileName { get; set; }

    public int RowCount { get; set; }

    public ICollection<Payment> Payments { get; set; } = [];
}
