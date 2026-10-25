using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Server.Payments.Application.Abstractions;
using Server.Payments.Application.Dtos;

namespace Server.Api.Tests;

public sealed class PaymentEndpointTests
{
    [Fact]
    public async Task ShouldImportCsvOnServer_WhenFileIsValid()
    {
        var service = new StubPaymentService();
        using var client = CreateClient(service);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Date,Description,Amount\n24/10/2026,Salary,2000"), "file", "statement.csv");

        var response = await client.PostAsync("/payments/imports", content, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("statement.csv", service.ImportedFileName);
    }

    [Fact]
    public async Task ShouldReturnRowValidationErrors_WhenCsvContainsInvalidPayments()
    {
        var service = new StubPaymentService { RejectImport = true };
        using var client = CreateClient(service);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("invalid"), "file", "statement.csv");

        var response = await client.PostAsync("/payments/imports", content, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.Contains("rows[2]", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShouldRejectCsv_WhenFileNameExceedsPersistenceLimit()
    {
        var service = new StubPaymentService();
        using var client = CreateClient(service);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Date,Description,Amount\n24/10/2026,Salary,2000"), "file", $"{new string('a', 252)}.csv");

        var response = await client.PostAsync("/payments/imports", content, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.ImportedFileName);
    }

    [Fact]
    public async Task ShouldPassFiltersToService_WhenListingPayments()
    {
        var service = new StubPaymentService();
        using var client = CreateClient(service);
        var importId = Guid.Parse("7f598074-0ba6-4da2-b795-848e7f14195e");

        var response = await client.GetAsync($"/payments?dateAfter=2026-04-01&dateBefore=2027-04-05&search=salary&direction=incoming&importId={importId}&page=2&pageSize=10", CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(service.Query);
        Assert.Equal(importId, service.Query.ImportId);
        Assert.Equal("salary", service.Query.Search);
    }

    [Fact]
    public async Task ShouldDeletePaymentsByImport_WhenImportIsSelected()
    {
        var service = new StubPaymentService();
        using var client = CreateClient(service);
        var importId = Guid.Parse("7f598074-0ba6-4da2-b795-848e7f14195e");

        var response = await client.DeleteAsync($"/payments?importId={importId}", CancellationToken.None);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(importId, service.DeleteRequest?.ImportId);
    }

    [Fact]
    public async Task ShouldRejectDelete_WhenNoDeletionMethodIsSelected()
    {
        var service = new StubPaymentService();
        using var client = CreateClient(service);

        var response = await client.DeleteAsync("/payments", CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.DeleteRequest);
    }

    private static HttpClient CreateClient(StubPaymentService service)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPaymentService>();
                services.AddSingleton<IPaymentService>(service);
            });
        });
        return factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    }

    private sealed class StubPaymentService : IPaymentService
    {
        private static readonly Guid ImportId = Guid.Parse("8c3712ab-3d08-42e0-bb3e-521ad7bc2559");
        public bool RejectImport { get; init; }
        public string? ImportedFileName { get; private set; }
        public PaymentListQueryDto? Query { get; private set; }
        public DeletePaymentsDto? DeleteRequest { get; private set; }

        public Task<PaymentPageDto> GetPageAsync(PaymentListQueryDto query, CancellationToken cancellationToken)
        {
            Query = query;
            return Task.FromResult(new PaymentPageDto([], query.Page, query.PageSize, 0, 0));
        }

        public Task<IReadOnlyList<PaymentImportDto>> GetImportsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<PaymentImportDto>>([]);

        public Task<PaymentImportResultDto> ImportAsync(Stream csv, string fileName, CancellationToken cancellationToken)
        {
            ImportedFileName = fileName;
            return Task.FromResult(RejectImport
                ? new PaymentImportResultDto(null, new Dictionary<string, string[]> { ["rows[2]"] = ["Date is not valid."] })
                : new PaymentImportResultDto(new PaymentImportDto(ImportId, fileName, 1, DateTimeOffset.UtcNow), new Dictionary<string, string[]>()));
        }

        public Task<int> DeleteAsync(DeletePaymentsDto request, CancellationToken cancellationToken)
        {
            DeleteRequest = request;
            return Task.FromResult(1);
        }
    }
}
