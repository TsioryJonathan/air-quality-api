namespace AirQuality.Domain;

/// <summary>OpenWeather air quality index: 1 (good) to 5 (very poor).</summary>
public enum AqiLevel
{
    Good = 1,
    Fair = 2,
    Moderate = 3,
    Poor = 4,
    VeryPoor = 5,
}

public static class AqiLevels
{
    public static AqiLevel From(int aqi) =>
        aqi is >= 1 and <= 5
            ? (AqiLevel)aqi
            : throw new ArgumentOutOfRangeException(nameof(aqi), aqi, "AQI must be between 1 and 5.");
}
