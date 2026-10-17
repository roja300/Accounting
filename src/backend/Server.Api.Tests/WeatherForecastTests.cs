namespace Server.Api.Tests;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(-40, -40)]
    [InlineData(0, 32)]
    [InlineData(20, 67)]
    public void TemperatureF_ConvertsFromCelsius(int temperatureC, int expectedTemperatureF)
    {
        var forecast = new WeatherForecast { TemperatureC = temperatureC };

        Assert.Equal(expectedTemperatureF, forecast.TemperatureF);
    }
}
