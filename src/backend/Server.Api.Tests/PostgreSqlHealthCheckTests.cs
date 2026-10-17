using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Server.Api.Health;

namespace Server.Api.Tests;

public class PostgreSqlHealthCheckTests
{
    [Fact]
    public async Task ShouldReturnUnhealthy_WhenConnectionIsNotConfigured()
    {
        var configuration = new ConfigurationBuilder().Build();
        var healthCheck = new PostgreSqlHealthCheck(configuration);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal("PostgreSQL is not configured.", result.Description);
    }
}
