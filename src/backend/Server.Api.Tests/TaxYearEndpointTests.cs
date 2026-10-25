using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Server.Tax.Application.Abstractions;
using Server.Tax.Application.Dtos;

namespace Server.Api.Tests;

public sealed class TaxYearEndpointTests
{
    [Fact]
    public async Task ShouldReturnTaxYearCards_WhenTaxYearsAreRequested()
    {
        var service = new StubTaxYearService();
        using var client = CreateClient(service);

        var response = await client.GetAsync("/tax-years", CancellationToken.None);
        var body = await response.Content.ReadFromJsonAsync<List<TaxYearDto>>(CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var card = Assert.Single(body!);
        Assert.Equal(2026, card.StartYear);
        Assert.Equal(52_000, card.ExpectedPay);
    }

    [Fact]
    public async Task ShouldConfigureRequestedTaxYear_WhenDetailsAreValid()
    {
        var service = new StubTaxYearService();
        using var client = CreateClient(service);
        var configuration = Configuration() with { EmploymentIncome = 18_000 };

        var response = await client.PutAsJsonAsync("/tax-years/2026", configuration, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2026, service.ConfiguredYear);
        Assert.Equal(18_000, service.Configuration?.EmploymentIncome);
    }

    private static HttpClient CreateClient(StubTaxYearService service)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITaxYearService>();
                services.AddSingleton<ITaxYearService>(service);
            });
        });
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static TaxYearConfigurationDto Configuration() => new(
        0, 0, 0, 0, 0, 12_570, 100_000, 37_700, 125_140,
        0.20m, 0.40m, 0.45m, 12_570, 50_270, 0.06m, 0.02m);

    private sealed class StubTaxYearService : ITaxYearService
    {
        public int? ConfiguredYear { get; private set; }
        public TaxYearConfigurationDto? Configuration { get; private set; }

        public Task<IReadOnlyList<TaxYearDto>> GetTaxYearsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TaxYearDto>>([Card("draft", Configuration())]);

        public Task<TaxYearDto> ConfigureAsync(
            int startYear,
            TaxYearConfigurationDto configuration,
            CancellationToken cancellationToken)
        {
            ConfiguredYear = startYear;
            Configuration = configuration;
            return Task.FromResult(Card("configured", configuration));
        }

        private static TaxYearDto Card(string status, TaxYearConfigurationDto configuration) => new(
            2026,
            new DateOnly(2026, 4, 6),
            new DateOnly(2027, 4, 5),
            status,
            52_000,
            40_000,
            5_486,
            1_645.80m,
            7_131.80m,
            configuration);
    }
}
