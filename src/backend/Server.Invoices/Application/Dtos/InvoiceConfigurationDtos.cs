namespace Server.Invoices.Application.Dtos;

public sealed record InvoiceConfigurationDto(string Style);

public sealed record InvoiceConfigurationResponseDto(
    string Style,
    bool HasLogo,
    string LogoFileName,
    DateTimeOffset? UpdatedAt);

public sealed record InvoiceLogoDto(byte[] Data, string ContentType, string FileName);
