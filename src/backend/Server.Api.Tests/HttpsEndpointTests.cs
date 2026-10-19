using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Server.Api.Tests;

public class HttpsEndpointTests
{
    [Fact]
    public async Task ShouldRedirectHttpRequestsToHttps()
    {
        using var client = CreateClient("http://accounting.test", allowAutoRedirect: false);

        var response = await client.GetAsync("/health/live", CancellationToken.None);

        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal(
            new Uri("https://accounting.test/health/live"),
            response.Headers.Location);
    }

    [Fact]
    public async Task ShouldIncludeHstsHeader_WhenRequestUsesHttpsInProduction()
    {
        using var client = CreateClient("https://accounting.test", allowAutoRedirect: true);

        var response = await client.GetAsync("/health/live", CancellationToken.None);

        response.EnsureSuccessStatusCode();
        Assert.Contains(
            "max-age=31536000",
            Assert.Single(response.Headers.GetValues("Strict-Transport-Security")),
            StringComparison.Ordinal);
    }

    private static HttpClient CreateClient(string baseAddress, bool allowAutoRedirect)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Production"));

        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = allowAutoRedirect,
            BaseAddress = new Uri(baseAddress)
        });
    }
}
