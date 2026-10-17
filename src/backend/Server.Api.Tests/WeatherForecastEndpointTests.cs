using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Server.Api.Tests;

public class WeatherForecastEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"))
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsForecasts()
    {
        var response = await _client.GetAsync("/WeatherForecast");

        response.EnsureSuccessStatusCode();
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();

        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Length);
        Assert.All(forecasts, forecast =>
        {
            Assert.InRange(forecast.TemperatureC, -20, 54);
            Assert.False(string.IsNullOrWhiteSpace(forecast.Summary));
        });
    }

    [Fact]
    public async Task OpenApiDocument_IsAvailableInDevelopment()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        response.EnsureSuccessStatusCode();
        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/WeatherForecast", document, StringComparison.Ordinal);
    }
}
