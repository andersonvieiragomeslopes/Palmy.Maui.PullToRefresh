namespace Example.Models;

public class Weather
{
    public DateTimeOffset Date { get; set; }
    public WeatherType Type { get; set; }
    public int? CurrentTemperature { get; set; }
    public int MinTemperature { get; set; }
    public int MaxTemperature { get; set; }
    public int Wind { get; set; }
}

public enum WeatherType
{
    Sunny,
    Cloudy,
    MostlyCloudyWithShowers,
    MostlyCloud,
    Rainy
}

public static class WeatherTypeExtension
{
    public static string? ToGlyph(this WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Sunny: return "sun";
            case WeatherType.Cloudy: return "cloud";
            case WeatherType.MostlyCloudyWithShowers: return "cloud-sun-rain";
            case WeatherType.MostlyCloud: return "cloud-sun";
            case WeatherType.Rainy: return "cloud-rain";
        }
        return null;
    }

    public static string? ToDescription(this WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Sunny: return "Sunny";
            case WeatherType.Cloudy: return "Cloudy";
            case WeatherType.MostlyCloudyWithShowers: return "Mostly cloudy with showers";
            case WeatherType.MostlyCloud: return "Mostly cloudy";
            case WeatherType.Rainy: return "Rainy";
        }
        return null;
    }
}
