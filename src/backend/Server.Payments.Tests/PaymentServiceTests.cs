using System.Text;
using Server.Payments.Application.Dtos;
using Server.Payments.Application.Importing;
using Server.Payments.Application.Services;
using Server.Payments.Domain;
using Server.Payments.Infrastructure.Abstractions;

namespace Server.Payments.Tests;

public sealed class PaymentServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ShouldSaveWholeImport_WhenEveryRowIsValid()
    {
        var context = new MockPaymentContext();
        var service = CreateService(context);
        const string csv = "Date,Description,From,Reference,Amount,Balance\n24/10/2026,Salary,Example Ltd,PAY-1,2000,5000\n25/10/2026,Bonus,Example Ltd,PAY-2,500,5500";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = await service.ImportAsync(stream, "statement.csv", CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.NotNull(context.AddedImport);
        Assert.Equal(2, context.AddedImport.RowCount);
        Assert.Equal(2, context.AddedImport.Payments.Count);
        Assert.Equal(1, context.SaveChangesCalls);
    }

    [Fact]
    public async Task ShouldNotSaveAnyRows_WhenOneRowIsInvalid()
    {
        var context = new MockPaymentContext();
        var service = CreateService(context);
        const string csv = "Date,Description,Amount\n24/10/2026,Salary,2000\ninvalid,Broken,10";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var result = await service.ImportAsync(stream, "statement.csv", CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Null(context.AddedImport);
        Assert.Equal(0, context.SaveChangesCalls);
    }

    [Fact]
    public async Task ShouldPassFiltersAndPaginationToPersistence_WhenListingPayments()
    {
        var context = new MockPaymentContext();
        var service = CreateService(context);

        await service.GetPageAsync(new PaymentListQueryDto(
            new DateOnly(2026, 4, 1),
            new DateOnly(2027, 4, 5),
            "Acme",
            "incoming",
            Guid.Parse("7f598074-0ba6-4da2-b795-848e7f14195e"),
            3,
            25), CancellationToken.None);

        Assert.NotNull(context.CapturedQuery);
        Assert.Equal(50, context.CapturedQuery.Offset);
        Assert.Equal("Acme", context.CapturedQuery.Search);
        Assert.Equal("incoming", context.CapturedQuery.Direction);
    }

    private static PaymentService CreateService(MockPaymentContext context) => new(context, new CsvPaymentParser(), new FixedTimeProvider(Now));

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class MockPaymentContext : IPaymentContext
    {
        public PaymentImport? AddedImport { get; private set; }
        public PaymentQuery? CapturedQuery { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<PaymentQueryResult> QueryPaymentsAsync(PaymentQuery query, CancellationToken cancellationToken)
        {
            CapturedQuery = query;
            return Task.FromResult(new PaymentQueryResult([], 0));
        }

        public Task<IReadOnlyList<PaymentImport>> GetImportsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<PaymentImport>>([]);

        public Task AddImportAsync(PaymentImport paymentImport, CancellationToken cancellationToken)
        {
            AddedImport = paymentImport;
            return Task.CompletedTask;
        }

        public Task<int> DeletePaymentsAsync(PaymentDeleteQuery query, CancellationToken cancellationToken) => Task.FromResult(0);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }
}
