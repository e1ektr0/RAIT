namespace RAIT.Example.API.Models;

public record WeatherForecast
{
    public int Id { get; init; }
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string Summary { get; init; } = string.Empty;
}