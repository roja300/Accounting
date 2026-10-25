using System.Text;
using Server.Payments.Application.Importing;

namespace Server.Payments.Tests;

public sealed class CsvPaymentParserTests
{
    private readonly CsvPaymentParser parser = new();

    [Fact]
    public async Task ShouldParseQuotedGenericBankRows_WhenRequiredColumnsArePresent()
    {
        const string csv = "Date,Time,Description,From,Reference,Amount,Balance,Currency\n24/10/2026,14:30,\"Invoice, October\",Acme Ltd,INV-1042,1250.50,8420.10,GBP";

        var result = await ParseAsync(csv);

        Assert.True(result.IsValid);
        var payment = Assert.Single(result.Payments);
        Assert.Equal("Invoice, October", payment.Description);
        Assert.Equal("Acme Ltd", payment.Counterparty);
        Assert.Equal("INV-1042", payment.Reference);
        Assert.Equal(1250.50m, payment.Amount);
        Assert.Equal(8420.10m, payment.Balance);
        Assert.Equal("GBP", payment.Currency);
    }

    [Fact]
    public async Task ShouldParseCreditAndDebitColumns_WhenBankExportsSeparateAmounts()
    {
        const string csv = "Transaction Date,Narrative,Money In,Money Out,Running Balance\n2026-10-23,Salary,2500.00,,9000.00\n2026-10-24,Office rent,,750.00,8250.00";

        var result = await ParseAsync(csv);

        Assert.True(result.IsValid);
        Assert.Equal(2500m, result.Payments[0].Amount);
        Assert.Equal(-750m, result.Payments[1].Amount);
    }

    [Fact]
    public async Task ShouldReturnEveryRowError_WhenRowsAreInvalid()
    {
        const string csv = "Date,Description,Amount\nnot-a-date,Salary,0\n24/10/2026,,hello";

        var result = await ParseAsync(csv);

        Assert.False(result.IsValid);
        Assert.Contains("rows[2]", result.Errors.Keys);
        Assert.Contains("rows[3]", result.Errors.Keys);
        Assert.Empty(result.Payments);
    }

    private async Task<PaymentParseResult> ParseAsync(string csv)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        return await parser.ParseAsync(stream, CancellationToken.None);
    }
}
