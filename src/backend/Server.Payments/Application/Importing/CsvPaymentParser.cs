using System.Globalization;
using System.Text;

namespace Server.Payments.Application.Importing;

public sealed record ParsedPayment(
    DateTimeOffset TransactionAt,
    string Description,
    string Counterparty,
    string Reference,
    decimal Amount,
    decimal? Balance,
    string Currency);

public sealed record PaymentParseResult(
    IReadOnlyList<ParsedPayment> Payments,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public sealed class CsvPaymentParser
{
    private static readonly string[] DateHeaders = ["date", "transactiondate", "completeddate"];
    private static readonly string[] DateTimeHeaders = ["datetime", "timestamp", "transactiondatetime", "starteddate"];
    private static readonly string[] TimeHeaders = ["time", "transactiontime"];
    private static readonly string[] DescriptionHeaders = ["description", "transactiondescription", "narrative", "memo", "merchant", "notes", "type", "transactiontype"];
    private static readonly string[] CounterpartyHeaders = ["from", "payer", "paidby", "counterparty", "counterpartyname", "name", "merchant"];
    private static readonly string[] ReferenceHeaders = ["reference", "paymentreference", "transactionid", "id"];
    private static readonly string[] AmountHeaders = ["amount", "transactionamount", "amountgbp", "value"];
    private static readonly string[] PenceHeaders = ["amountpence"];
    private static readonly string[] CreditHeaders = ["credit", "creditamount", "moneyin", "paidin"];
    private static readonly string[] DebitHeaders = ["debit", "debitamount", "moneyout", "paidout"];
    private static readonly string[] BalanceHeaders = ["balance", "runningbalance"];
    private static readonly string[] CurrencyHeaders = ["currency", "currencycode"];
    private static readonly string[] DateFormats = ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "MM/dd/yyyy", "M/d/yyyy"];
    private static readonly string[] DateTimeFormats = ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "d/M/yyyy H:mm", "yyyy-MM-dd'T'HH:mm:ssK", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK"];

    public async Task<PaymentParseResult> ParseAsync(Stream csv, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(csv, Encoding.UTF8, true, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken);
        IReadOnlyList<IReadOnlyList<string>> rows;
        try
        {
            rows = ParseRows(text);
        }
        catch (FormatException exception)
        {
            return Invalid("file", exception.Message);
        }

        if (rows.Count < 2)
            return Invalid("file", "The CSV must contain a header and at least one payment row.");

        var headers = rows[0].Select(NormalizeHeader).ToList();
        var date = Find(headers, DateHeaders);
        var dateTime = Find(headers, DateTimeHeaders);
        var time = Find(headers, TimeHeaders);
        var description = Find(headers, DescriptionHeaders);
        var counterparty = Find(headers, CounterpartyHeaders);
        var reference = Find(headers, ReferenceHeaders);
        var amount = Find(headers, AmountHeaders);
        var pence = Find(headers, PenceHeaders);
        var credit = Find(headers, CreditHeaders);
        var debit = Find(headers, DebitHeaders);
        var balance = Find(headers, BalanceHeaders);
        var currency = Find(headers, CurrencyHeaders);

        var headerErrors = new List<string>();
        if (date < 0 && dateTime < 0) headerErrors.Add("a date or date/time column");
        if (description < 0) headerErrors.Add("a description column");
        if (amount < 0 && pence < 0 && credit < 0 && debit < 0) headerErrors.Add("an amount column, or credit/debit columns");
        if (headerErrors.Count > 0)
            return Invalid("headers", $"The CSV is missing {string.Join(", ", headerErrors)}.");

        var payments = new List<ParsedPayment>();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        for (var index = 1; index < rows.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rows[index].All(string.IsNullOrWhiteSpace)) continue;
            var rowNumber = index + 1;
            var rowErrors = new List<string>();
            var row = rows[index];
            if (row.Count != headers.Count)
            {
                errors[$"rows[{rowNumber}]"] = [$"Expected {headers.Count} columns but found {row.Count}."];
                continue;
            }

            var transactionAt = ParseDate(row, dateTime, date, time, rowErrors);
            var rowDescription = Value(row, description).Trim();
            if (rowDescription.Length == 0) rowErrors.Add("Description is required.");
            if (rowDescription.Length > 500) rowErrors.Add("Description must be 500 characters or fewer.");
            var rowCounterparty = Value(row, counterparty).Trim();
            if (rowCounterparty.Length > 200) rowErrors.Add("Counterparty must be 200 characters or fewer.");
            var rowReference = Value(row, reference).Trim();
            if (rowReference.Length > 200) rowErrors.Add("Reference must be 200 characters or fewer.");
            var rowAmount = ParseAmount(row, amount, pence, credit, debit, rowErrors);
            if (rowAmount == 0m) rowErrors.Add("Amount must not be zero.");
            var rowBalance = ParseOptionalMoney(Value(row, balance), "Balance", rowErrors);
            var rowCurrency = Value(row, currency).Trim().ToUpperInvariant();
            if (rowCurrency.Length == 0) rowCurrency = InferCurrency(rows[0], amount, pence);
            if (rowCurrency.Length != 3 || !rowCurrency.All(char.IsLetter)) rowErrors.Add("Currency must be a three-letter code.");

            if (rowErrors.Count > 0)
            {
                errors[$"rows[{rowNumber}]"] = [.. rowErrors];
                continue;
            }

            payments.Add(new ParsedPayment(transactionAt!.Value, rowDescription, rowCounterparty, rowReference, rowAmount!.Value, rowBalance, rowCurrency));
        }

        return errors.Count == 0
            ? new PaymentParseResult(payments, errors)
            : new PaymentParseResult([], errors);
    }

    private static DateTimeOffset? ParseDate(IReadOnlyList<string> row, int dateTime, int date, int time, ICollection<string> errors)
    {
        var dateTimeText = Value(row, dateTime).Trim();
        if (dateTimeText.Length > 0)
        {
            if (DateTimeOffset.TryParseExact(dateTimeText, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedDateTime) ||
                DateTimeOffset.TryParse(dateTimeText, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out parsedDateTime))
                return parsedDateTime;
            errors.Add("Date/time is not valid.");
            return null;
        }

        var dateText = Value(row, date).Trim();
        if (!DateOnly.TryParseExact(dateText, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            errors.Add("Date is not valid.");
            return null;
        }

        var parsedTime = TimeOnly.MinValue;
        var timeText = Value(row, time).Trim();
        if (timeText.Length > 0 && !TimeOnly.TryParse(timeText, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.None, out parsedTime))
        {
            errors.Add("Time is not valid.");
            return null;
        }
        return new DateTimeOffset(parsedDate.ToDateTime(parsedTime), TimeZoneInfo.Local.GetUtcOffset(parsedDate.ToDateTime(parsedTime)));
    }

    private static decimal? ParseAmount(IReadOnlyList<string> row, int amount, int pence, int credit, int debit, ICollection<string> errors)
    {
        if (pence >= 0)
        {
            var value = ParseRequiredMoney(Value(row, pence), "Amount", errors);
            return value / 100m;
        }
        if (amount >= 0) return ParseRequiredMoney(Value(row, amount), "Amount", errors);
        var creditValue = ParseOptionalMoney(Value(row, credit), "Credit", errors) ?? 0m;
        var debitValue = ParseOptionalMoney(Value(row, debit), "Debit", errors) ?? 0m;
        if (creditValue == 0m && debitValue == 0m) errors.Add("Credit or debit is required.");
        return creditValue - Math.Abs(debitValue);
    }

    private static decimal? ParseRequiredMoney(string value, string name, ICollection<string> errors)
    {
        var result = ParseOptionalMoney(value, name, errors);
        if (result is null && string.IsNullOrWhiteSpace(value)) errors.Add($"{name} is required.");
        return result;
    }

    private static decimal? ParseOptionalMoney(string value, string name, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().Replace("£", "", StringComparison.Ordinal).Replace("€", "", StringComparison.Ordinal).Replace("$", "", StringComparison.Ordinal).Replace(",", "", StringComparison.Ordinal);
        var negative = normalized.StartsWith('(') && normalized.EndsWith(')');
        normalized = normalized.Trim('(', ')');
        if (!decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed))
        {
            errors.Add($"{name} is not a valid monetary value.");
            return null;
        }
        return negative ? -parsed : parsed;
    }

    private static string InferCurrency(IReadOnlyList<string> originalHeaders, int amount, int pence)
    {
        var header = Value(originalHeaders, amount >= 0 ? amount : pence).ToUpperInvariant();
        if (header.Contains("EUR", StringComparison.Ordinal)) return "EUR";
        if (header.Contains("USD", StringComparison.Ordinal)) return "USD";
        return "GBP";
    }

    private static int Find(IReadOnlyList<string> headers, IEnumerable<string> aliases)
    {
        foreach (var alias in aliases)
        {
            for (var index = 0; index < headers.Count; index++)
            {
                if (headers[index] == alias) return index;
            }
        }
        return -1;
    }

    private static string Value(IReadOnlyList<string> row, int index) => index >= 0 && index < row.Count ? row[index] : string.Empty;

    private static string NormalizeHeader(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static PaymentParseResult Invalid(string key, string message) => new([], new Dictionary<string, string[]> { [key] = [message] });

    private static IReadOnlyList<IReadOnlyList<string>> ParseRows(string text)
    {
        var rows = new List<IReadOnlyList<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            if (character == '"')
            {
                if (quoted && index + 1 < text.Length && text[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else quoted = !quoted;
            }
            else if (character == ',' && !quoted)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if ((character == '\r' || character == '\n') && !quoted)
            {
                if (character == '\r' && index + 1 < text.Length && text[index + 1] == '\n') index++;
                row.Add(field.ToString());
                field.Clear();
                if (row.Any(value => value.Length > 0)) rows.Add(row);
                row = [];
            }
            else field.Append(character);
        }
        if (quoted) throw new FormatException("The CSV contains an unterminated quoted field.");
        row.Add(field.ToString());
        if (row.Any(value => value.Length > 0)) rows.Add(row);
        return rows;
    }
}
