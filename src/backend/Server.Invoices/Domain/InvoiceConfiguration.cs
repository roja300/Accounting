using Server.Shared.Common;

namespace Server.Invoices.Domain;

public sealed class InvoiceConfiguration : Entity
{
    public string Style { get; set; } = InvoiceStyles.Basic;

    public string LogoFileName { get; set; } = string.Empty;

    public string LogoContentType { get; set; } = string.Empty;

    public byte[]? LogoData { get; set; }
}

public static class InvoiceStyles
{
    public const string Basic = "basic";
    public const string Modern = "modern";
    public const string Minimal = "minimal";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(
        [Basic, Modern, Minimal],
        StringComparer.Ordinal);
}
