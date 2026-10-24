using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Server.Invoices.Application.Abstractions;
using Server.Invoices.Application.Dtos;

namespace Server.Api.Tests;

public class ProfileEndpointTests
{
    [Fact]
    public async Task ShouldSaveBusinessProfile_WhenRequiredDetailsAreProvided()
    {
        var service = new StubProfileService();
        using var client = CreateClient(service);
        var request = CreateBusiness();

        var response = await client.PutAsJsonAsync(
            "/profile/business",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(request, service.SavedBusiness);
    }

    [Fact]
    public async Task ShouldRejectLimitedCompany_WhenCompanyNumberIsMissing()
    {
        var service = new StubProfileService();
        using var client = CreateClient(service);
        var request = CreateBusiness() with
        {
            BusinessType = "limitedCompany",
            CompanyNumber = null
        };

        var response = await client.PutAsJsonAsync(
            "/profile/business",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.SavedBusiness);
        Assert.Contains(
            "companyNumber",
            await response.Content.ReadAsStringAsync(CancellationToken.None),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShouldCreateCustomer_WhenInvoiceDetailsAreValid()
    {
        var service = new StubProfileService();
        using var client = CreateClient(service);
        var request = CreateCustomer();

        var response = await client.PostAsJsonAsync(
            "/profile/clients",
            request,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(request, service.CreatedClient);
    }

    [Fact]
    public async Task ShouldSaveInvoiceConfiguration_WhenStyleIsSupported()
    {
        var service = new StubProfileService();
        using var client = CreateClient(service);

        var response = await client.PutAsJsonAsync(
            "/profile/invoice-configuration",
            new InvoiceConfigurationDto("modern"),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("modern", service.SavedInvoiceConfiguration?.Style);
    }

    [Fact]
    public async Task ShouldRejectInvoiceConfiguration_WhenStyleIsUnsupported()
    {
        var service = new StubProfileService();
        using var client = CreateClient(service);

        var response = await client.PutAsJsonAsync(
            "/profile/invoice-configuration",
            new InvoiceConfigurationDto("decorative"),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(service.SavedInvoiceConfiguration);
    }

    [Fact]
    public async Task ShouldSaveInvoiceLogo_WhenPngIsUploaded()
    {
        var service = new StubProfileService();
        using var client = CreateClient(service);
        using var content = new MultipartFormDataContent();
        var logo = new ByteArrayContent([137, 80, 78, 71]);
        logo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(logo, "logo", "logo.png");

        var response = await client.PutAsync(
            "/profile/invoice-configuration/logo",
            content,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([137, 80, 78, 71], service.SavedLogo);
        Assert.Equal("image/png", service.SavedLogoContentType);
    }

    [Fact]
    public async Task ShouldReturnDemonstrationPdf_WhenInvoiceStyleIsSupported()
    {
        var service = new StubProfileService();
        using var client = CreateClient(service);

        var response = await client.GetAsync(
            "/profile/invoice-configuration/demo/modern",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        var data = await response.Content.ReadAsByteArrayAsync(CancellationToken.None);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(data, 0, 4));
    }

    private static HttpClient CreateClient(StubProfileService service)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IProfileService>();
                    services.AddSingleton<IProfileService>(service);
                });
            });

        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static BusinessProfileDto CreateBusiness() => new(
        "soleTrader",
        "Example Freelancer",
        null,
        "owner@example.test",
        null,
        "1 High Street",
        null,
        "London",
        null,
        "SW1A 1AA",
        null,
        null);

    private static ClientDto CreateCustomer() => new(
        "Acme Ltd",
        null,
        "accounts@acme.test",
        null,
        "2 Market Road",
        null,
        "Manchester",
        null,
        "M1 1AA");

    private sealed class StubProfileService : IProfileService
    {
        public BusinessProfileDto? SavedBusiness { get; private set; }

        public ClientDto? CreatedClient { get; private set; }

        public InvoiceConfigurationDto? SavedInvoiceConfiguration { get; private set; }

        public byte[]? SavedLogo { get; private set; }

        public string? SavedLogoContentType { get; private set; }

        public Task<BusinessProfileResponseDto?> GetBusinessAsync(
            CancellationToken cancellationToken) => Task.FromResult<BusinessProfileResponseDto?>(null);

        public Task<BusinessProfileResponseDto> SaveBusinessAsync(
            BusinessProfileDto profile,
            CancellationToken cancellationToken)
        {
            SavedBusiness = profile;
            return Task.FromResult(new BusinessProfileResponseDto(
                profile.BusinessType,
                profile.LegalName,
                profile.TradingName ?? string.Empty,
                profile.Email,
                profile.Phone ?? string.Empty,
                profile.AddressLine1,
                profile.AddressLine2 ?? string.Empty,
                profile.City,
                profile.County ?? string.Empty,
                profile.Postcode,
                profile.CompanyNumber ?? string.Empty,
                profile.VatNumber ?? string.Empty,
                DateTimeOffset.UtcNow));
        }

        public Task<IReadOnlyList<ClientResponseDto>> GetClientsAsync(
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ClientResponseDto>>([]);

        public Task<ClientResponseDto> CreateClientAsync(
            ClientDto client,
            CancellationToken cancellationToken)
        {
            CreatedClient = client;
            return Task.FromResult(ToResponse(client));
        }

        public Task<ClientResponseDto?> UpdateClientAsync(
            Guid id,
            ClientDto client,
            CancellationToken cancellationToken) => Task.FromResult<ClientResponseDto?>(ToResponse(client));

        public Task<ClientDeleteResult> DeleteClientAsync(
            Guid id,
            CancellationToken cancellationToken) => Task.FromResult(ClientDeleteResult.Deleted);

        public Task<InvoiceConfigurationResponseDto> GetInvoiceConfigurationAsync(
            CancellationToken cancellationToken) => Task.FromResult(ConfigurationResponse());

        public Task<InvoiceConfigurationResponseDto> SaveInvoiceConfigurationAsync(
            InvoiceConfigurationDto configuration,
            CancellationToken cancellationToken)
        {
            SavedInvoiceConfiguration = configuration;
            return Task.FromResult(ConfigurationResponse(configuration.Style));
        }

        public Task<InvoiceConfigurationResponseDto> SaveInvoiceLogoAsync(
            byte[] data,
            string contentType,
            string fileName,
            CancellationToken cancellationToken)
        {
            SavedLogo = data;
            SavedLogoContentType = contentType;
            return Task.FromResult(new InvoiceConfigurationResponseDto(
                "basic", true, fileName, DateTimeOffset.UtcNow));
        }

        public Task<InvoiceLogoDto?> GetInvoiceLogoAsync(CancellationToken cancellationToken) =>
            Task.FromResult<InvoiceLogoDto?>(null);

        public Task<InvoiceConfigurationResponseDto> DeleteInvoiceLogoAsync(
            CancellationToken cancellationToken) => Task.FromResult(ConfigurationResponse());

        private static InvoiceConfigurationResponseDto ConfigurationResponse(string style = "basic") =>
            new(style, false, string.Empty, DateTimeOffset.UtcNow);

        private static ClientResponseDto ToResponse(ClientDto client) => new(
            Guid.Parse("b31f5928-64b0-4dd7-aad1-3a3ca55158ed"),
            client.Name,
            client.ContactName ?? string.Empty,
            client.Email,
            client.Phone ?? string.Empty,
            client.AddressLine1,
            client.AddressLine2 ?? string.Empty,
            client.City,
            client.County ?? string.Empty,
            client.Postcode,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
    }
}
