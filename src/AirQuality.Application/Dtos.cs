namespace AirQuality.Application;

public sealed record CityDto(int Id, string Name, string? Country, double? Latitude, double? Longitude);

public sealed record DailySummary(DateOnly Day, double? AverageAqi, int? MaxAqi, double? MaxPm25, int Readings);

public sealed record TriggeredAlert(int RuleId, int CityId, string City, string Pollutant, double Threshold, double Value, DateTime At);
