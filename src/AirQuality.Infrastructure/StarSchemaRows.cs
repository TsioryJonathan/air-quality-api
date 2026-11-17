namespace AirQuality.Infrastructure;

// Rows of the star schema written by the Airflow ETL (air-quality-etl).
// The API only reads these tables; their schema is owned by the ETL, not by EF migrations.

internal sealed class CityRow
{
    public int CityKey { get; set; }
    public string CityName { get; set; } = "";
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

internal sealed class FactRow
{
    public int FactId { get; set; }
    public int CityKey { get; set; }
    /// <summary>Hour encoded as YYYYMMDDHH (UTC).</summary>
    public int DateKey { get; set; }
    public int? Aqi { get; set; }
    public double? Co { get; set; }
    public double? No { get; set; }
    public double? No2 { get; set; }
    public double? O3 { get; set; }
    public double? So2 { get; set; }
    public double? Pm25 { get; set; }
    public double? Pm10 { get; set; }
    public double? Nh3 { get; set; }
}
