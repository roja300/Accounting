using Server.Api.Controllers;

namespace Server.Api.Tests;

public class WeatherForecastControllerTests
{
    private static readonly string[] ExpectedSummaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [Fact]
    public void Get_ReturnsFiveForecastsWithinTheExpectedRanges()
    {
        var firstExpectedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var controller = new WeatherForecastController();

        var forecasts = controller.Get().ToArray();

        Assert.Equal(5, forecasts.Length);
        Assert.Collection(
            forecasts,
            forecasts.Select((_, index) => new Action<WeatherForecast>(forecast =>
            {
                Assert.Equal(firstExpectedDate.AddDays(index), forecast.Date);
                Assert.InRange(forecast.TemperatureC, -20, 54);
                Assert.Contains(forecast.Summary, ExpectedSummaries);
            })).ToArray());
    }
}
