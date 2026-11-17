namespace AirQuality.Domain;

/// <summary>One hourly reading for one city. Concentrations are in µg/m³.</summary>
public sealed record Measurement(
    int CityId,
    DateTime At,
    int? Aqi,
    double? Co,
    double? No,
    double? No2,
    double? O3,
    double? So2,
    double? Pm25,
    double? Pm10,
    double? Nh3)
{
    public double? ValueOf(Pollutant pollutant) => pollutant switch
    {
        Pollutant.Co => Co,
        Pollutant.No => No,
        Pollutant.No2 => No2,
        Pollutant.O3 => O3,
        Pollutant.So2 => So2,
        Pollutant.Pm25 => Pm25,
        Pollutant.Pm10 => Pm10,
        Pollutant.Nh3 => Nh3,
        _ => throw new ArgumentOutOfRangeException(nameof(pollutant), pollutant, null),
    };
}
