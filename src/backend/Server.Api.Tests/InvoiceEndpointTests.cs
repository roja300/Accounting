using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;

namespace Server.Api.Tests;

public class InvoiceEndpointTests
{
    [Fact]
    public async Task ShouldReturnFilteredInvoicePage_WhenQueryIsValid()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);

        var response = await client.GetAsync(
            "/invoices?dateAfter=2026-10-01&dateBefore=2026-10-31&client=Acme%20Ltd&project=Retainer&search=bookkeeping&page=2&pageSize=6",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var query = Assert.IsType<InvoiceListQueryDto>(service.ListQuery);
        Assert.Equal(new DateOnly(2026, 10, 1), query.DateAfter);
        Assert.Equal(new DateOnly(2026, 10, 31), query.DateBefore);
        Assert.Equal("Acme Ltd", query.Client);
        Assert.Equal("Retainer", query.Project);
        Assert.Equal("bookkeeping", query.Search);
        Assert.Equal(2, query.Page);
        Assert.Equal(6, query.PageSize);
        var page = await response.Content.ReadFromJsonAsync<InvoicePageDto>(
            CancellationToken.None);
        Assert.NotNull(page);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task ShouldReturnValidationProblemWithoutCallingService_WhenListQueryIsInvalid()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);

        var response = await client.GetAsync(
            "/invoices?dateAfter=2026-10-31&dateBefore=2026-10-01&page=0&pageSize=51",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.ListQuery);
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.Contains("dateRange", body, StringComparison.Ordinal);
        Assert.Contains("pageSize", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShouldCreateInvoice_WhenRequestIsValid()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);
        var request = CreateRequest();

        var response = await client.PostAsJsonAsync(
            "/invoices",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(request, service.CreatedInvoice);
        var created = await response.Content.ReadFromJsonAsync<InvoiceResponseDto>(
            CancellationToken.None);
        Assert.NotNull(created);
        Assert.Equal(StubInvoiceService.InvoiceId, created.Id);
        Assert.Equal(
            $"https://localhost/invoices/{StubInvoiceService.InvoiceId}",
            response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ShouldCreateInvoice_WhenCommentsAreOmitted()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);
        var request = CreateRequest() with { Comments = null };

        var response = await client.PostAsJsonAsync(
            "/invoices",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(request, service.CreatedInvoice);
        var created = await response.Content.ReadFromJsonAsync<InvoiceResponseDto>(
            CancellationToken.None);
        Assert.NotNull(created);
        Assert.Equal(string.Empty, created.Comments);
    }

    [Fact]
    public async Task ShouldReturnValidationProblem_WhenCustomerIsNotConfigured()
    {
        var service = new StubInvoiceService { RejectClient = true };
        using var client = CreateClient(service);

        var response = await client.PostAsJsonAsync(
            "/invoices",
            CreateRequest() with { Client = "Unknown Ltd" },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.Contains("Select a configured customer.", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShouldReturnValidationProblemWithoutCallingService_WhenRequestIsInvalid()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);
        var request = new InvoiceDto("", 0, "CAD", "", "", "");

        var response = await client.PostAsJsonAsync(
            "/invoices",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.CreatedInvoice);
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.Contains("description", body, StringComparison.Ordinal);
        Assert.Contains("currency", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShouldUpdateInvoice_WhenRequestIsValid()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);
        var request = CreateRequest() with { Description = "Year-end accounts" };

        var response = await client.PutAsJsonAsync(
            $"/invoices/{StubInvoiceService.InvoiceId}",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(StubInvoiceService.InvoiceId, service.UpdatedInvoiceId);
        Assert.Equal(request, service.UpdatedInvoice);
        var updated = await response.Content.ReadFromJsonAsync<InvoiceResponseDto>(
            CancellationToken.None);
        Assert.NotNull(updated);
        Assert.Equal("Year-end accounts", updated.Description);
    }

    [Fact]
    public async Task ShouldReturnValidationProblemWithoutUpdating_WhenUpdateRequestIsInvalid()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);
        var request = CreateRequest() with { Description = "" };

        var response = await client.PutAsJsonAsync(
            $"/invoices/{StubInvoiceService.InvoiceId}",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.UpdatedInvoice);
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenUpdatedInvoiceDoesNotExist()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);

        var response = await client.PutAsJsonAsync(
            $"/invoices/{Guid.NewGuid()}",
            CreateRequest(),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShouldDeleteInvoice_WhenInvoiceExists()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);

        var response = await client.DeleteAsync(
            $"/invoices/{StubInvoiceService.InvoiceId}",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(StubInvoiceService.InvoiceId, service.DeletedInvoiceId);
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenDeletedInvoiceDoesNotExist()
    {
        var service = new StubInvoiceService();
        using var client = CreateClient(service);

        var response = await client.DeleteAsync(
            $"/invoices/{Guid.NewGuid()}",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpClient CreateClient(StubInvoiceService service)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IInvoiceService>();
                    services.AddSingleton<IInvoiceService>(service);
                });
            });

        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static InvoiceDto CreateRequest()
    {
        return new InvoiceDto(
            "Monthly bookkeeping",
            125.50m,
            "GBP",
            "Completed October records.",
            "Acme Ltd",
            "Retainer");
    }
}
