namespace AirQuality.Domain;

/// <summary>Raises an alert when a pollutant goes above a threshold in a city.</summary>
public sealed class AlertRule
{
    // Required by EF Core.
    private AlertRule() { }

    public AlertRule(int cityId, Pollutant pollutant, double threshold)
    {
        if (threshold <= 0 || double.IsNaN(threshold) || double.IsInfinity(threshold))
            throw new ArgumentOutOfRangeException(nameof(threshold), threshold, "Threshold must be a positive number.");

        CityId = cityId;
        Pollutant = pollutant;
        Threshold = threshold;
    }

    public int Id { get; private set; }
    public int CityId { get; private set; }
    public Pollutant Pollutant { get; private set; }
    public double Threshold { get; private set; }

    /// <summary>A missing value never triggers an alert.</summary>
    public bool IsExceededBy(Measurement measurement) =>
        measurement.CityId == CityId
        && measurement.ValueOf(Pollutant) is { } value
        && value > Threshold;
}
