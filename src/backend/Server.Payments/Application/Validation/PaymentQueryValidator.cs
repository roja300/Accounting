using Server.Payments.Application.Dtos;

namespace Server.Payments.Application.Validation;

public sealed class PaymentQueryValidator
{
    public const int MaximumPageSize = 100;

    public IReadOnlyDictionary<string, string[]> Validate(PaymentListQueryDto query)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (query.Page < 1)
            errors["page"] = ["Page must be at least 1."];
        if (query.PageSize is < 1 or > MaximumPageSize)
            errors["pageSize"] = [$"Page size must be between 1 and {MaximumPageSize}."];
        if (query.DateAfter > query.DateBefore)
            errors["dateRange"] = ["Date after must be on or before date before."];
        if (query.Search?.Trim().Length > 200)
            errors["search"] = ["Search must be 200 characters or fewer."];
        if (query.Direction is not null && query.Direction is not ("incoming" or "outgoing"))
            errors["direction"] = ["Direction must be incoming or outgoing."];
        return errors;
    }

    public IReadOnlyDictionary<string, string[]> Validate(DeletePaymentsDto request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.DateAfter > request.DateBefore)
            errors["dateRange"] = ["Date after must be on or before date before."];
        var selectorCount = (request.All ? 1 : 0) + (request.ImportId.HasValue ? 1 : 0) +
            (request.DateAfter.HasValue || request.DateBefore.HasValue ? 1 : 0);
        if (selectorCount == 0)
            errors["selection"] = ["Select all payments, an import, or a date range to delete."];
        if (selectorCount > 1)
            errors["selection"] = ["Use only one deletion method at a time."];
        return errors;
    }
}
