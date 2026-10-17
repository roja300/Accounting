namespace Server.Api;

public class WeatherForecast
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => (int)Math.Round(TemperatureC * 9d / 5d + 32, MidpointRounding.AwayFromZero);

    public string? Summary { get; set; }
}
