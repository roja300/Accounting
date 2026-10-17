namespace Server.Api.Tests;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(-40, -40)]
    [InlineData(0, 32)]
    [InlineData(20, 68)]
    public void ShouldConvertTemperatureFromCelsiusToFahrenheit(int temperatureC, int expectedTemperatureF)
    {
        var forecast = new WeatherForecast { TemperatureC = temperatureC };

        Assert.Equal(expectedTemperatureF, forecast.TemperatureF);
    }
}
