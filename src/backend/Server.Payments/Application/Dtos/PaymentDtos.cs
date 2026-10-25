namespace Server.Payments.Application.Dtos;

public sealed record PaymentDto(
    Guid Id,
    Guid ImportId,
    string ImportFileName,
    DateTimeOffset TransactionAt,
    string Description,
    string Counterparty,
    string Reference,
    decimal Amount,
    decimal? Balance,
    string Currency);

public sealed record PaymentPageDto(
    IReadOnlyList<PaymentDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record PaymentListQueryDto(
    DateOnly? DateAfter = null,
    DateOnly? DateBefore = null,
    string? Search = null,
    string? Direction = null,
    Guid? ImportId = null,
    int Page = 1,
    int PageSize = 25);

public sealed record PaymentImportDto(
    Guid Id,
    string FileName,
    int RowCount,
    DateTimeOffset ImportedAt);

public sealed record PaymentImportResultDto(
    PaymentImportDto? Import,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public sealed record DeletePaymentsDto(
    bool All = false,
    DateOnly? DateAfter = null,
    DateOnly? DateBefore = null,
    Guid? ImportId = null);
