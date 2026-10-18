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
