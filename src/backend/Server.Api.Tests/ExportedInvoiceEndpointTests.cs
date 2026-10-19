using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;
using Server.Invoices.Domain;

namespace Server.Api.Tests;

public class ExportedInvoiceEndpointTests
{
    [Fact]
    public async Task ShouldGenerateInvoice_WhenRequestIsValid()
    {
        var service = new StubExportedInvoiceService();
        using var client = CreateClient(service);
        var request = new GenerateInvoiceDto(
            StubExportedInvoiceService.ClientId,
            null,
            "GBP",
            new DateOnly(2026, 10, 19),
            new DateOnly(2026, 11, 18),
            "Thank you for your business.");

        var response = await client.PostAsJsonAsync(
            "/exported-invoices",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(request, service.GenerateRequest);
        Assert.Equal(
            $"https://localhost/exported-invoices/{StubExportedInvoiceService.InvoiceId}",
            response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ShouldRejectGenerationWithoutCallingService_WhenDueDatePrecedesIssueDate()
    {
        var service = new StubExportedInvoiceService();
        using var client = CreateClient(service);

        var response = await client.PostAsJsonAsync(
            "/exported-invoices",
            new GenerateInvoiceDto(
                StubExportedInvoiceService.ClientId,
                null,
                "GBP",
                new DateOnly(2026, 10, 19),
                new DateOnly(2026, 10, 18),
                null),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.GenerateRequest);
    }

    [Fact]
    public async Task ShouldReturnServerGeneratedPdf_WhenInvoiceExists()
    {
        var service = new StubExportedInvoiceService();
        var documents = new StubInvoiceDocumentService();
        using var client = CreateClient(service, documents);

        var response = await client.GetAsync(
            $"/exported-invoices/{StubExportedInvoiceService.InvoiceId}/pdf",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(
            "INV-20261019-ABC123.pdf",
            response.Content.Headers.ContentDisposition?.FileName,
            StringComparison.Ordinal);
        Assert.True(documents.WasCalled);
    }

    [Fact]
    public async Task ShouldMarkInvoiceDeleted_WhenDeletionIsConfirmed()
    {
        var service = new StubExportedInvoiceService();
        using var client = CreateClient(service);

        var response = await client.DeleteAsync(
            $"/exported-invoices/{StubExportedInvoiceService.InvoiceId}",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(GeneratedInvoiceStatuses.Deleted, service.UpdatedStatus);
    }

    private static HttpClient CreateClient(
        StubExportedInvoiceService service,
        StubInvoiceDocumentService? documents = null)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IExportedInvoiceService>();
                    services.RemoveAll<IInvoiceDocumentService>();
                    services.AddSingleton<IExportedInvoiceService>(service);
                    services.AddSingleton<IInvoiceDocumentService>(
                        documents ?? new StubInvoiceDocumentService());
                });
            });

        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private sealed class StubInvoiceDocumentService : IInvoiceDocumentService
    {
        public bool WasCalled { get; private set; }

        public byte[] Generate(GeneratedInvoice invoice)
        {
            WasCalled = true;
            return "%PDF-test"u8.ToArray();
        }
    }

    private sealed class StubExportedInvoiceService : IExportedInvoiceService
    {
        public static readonly Guid InvoiceId =
            Guid.Parse("ed652b89-8923-4c68-920c-2e32bbb4150f");
        public static readonly Guid ClientId =
            Guid.Parse("b31f5928-64b0-4dd7-aad1-3a3ca55158ed");

        public GenerateInvoiceDto? GenerateRequest { get; private set; }

        public string? UpdatedStatus { get; private set; }

        public Task<GeneratedInvoicePageDto> GetPageAsync(
            GeneratedInvoiceListQueryDto query,
            CancellationToken cancellationToken) =>
            Task.FromResult(new GeneratedInvoicePageDto([], 1, 9, 0, 0));

        public Task<GenerateInvoiceOptionsDto> GetOptionsAsync(
            CancellationToken cancellationToken) =>
            Task.FromResult(new GenerateInvoiceOptionsDto([], []));

        public Task<GeneratedInvoiceDto> GenerateAsync(
            GenerateInvoiceDto request,
            CancellationToken cancellationToken)
        {
            GenerateRequest = request;
            return Task.FromResult(CreateDto());
        }

        public Task<GeneratedInvoiceDto?> GetAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult<GeneratedInvoiceDto?>(id == InvoiceId ? CreateDto() : null);

        public Task<GeneratedInvoiceDto?> UpdateStatusAsync(
            Guid id,
            string status,
            CancellationToken cancellationToken)
        {
            UpdatedStatus = status;
            return Task.FromResult<GeneratedInvoiceDto?>(id == InvoiceId ? CreateDto(status) : null);
        }

        public Task<GeneratedInvoice?> GetDocumentSourceAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            Task.FromResult<GeneratedInvoice?>(id == InvoiceId
                ? new GeneratedInvoice
                {
                    Id = InvoiceId,
                    InvoiceNumber = "INV-20261019-ABC123"
                }
                : null);

        private static GeneratedInvoiceDto CreateDto(
            string status = GeneratedInvoiceStatuses.Exported) =>
            new(
                InvoiceId,
                "INV-20261019-ABC123",
                ClientId,
                "Acme Ltd",
                new DateOnly(2026, 10, 19),
                new DateOnly(2026, 11, 18),
                "GBP",
                125m,
                "",
                status,
                [],
                new DateTimeOffset(2026, 10, 19, 12, 0, 0, TimeSpan.Zero));
    }
}
