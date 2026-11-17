using AirQuality.Domain;

namespace AirQuality.Application.Abstractions;

public interface IAirQualityReader
{
    Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken ct);
    Task<bool> CityExistsAsync(int cityId, CancellationToken ct);
    Task<Measurement?> GetLatestAsync(int cityId, CancellationToken ct);
    Task<IReadOnlyList<Measurement>> GetLatestForAllCitiesAsync(CancellationToken ct);
    Task<IReadOnlyList<Measurement>> GetHourlyAsync(int cityId, DateTime from, DateTime to, CancellationToken ct);
    Task<IReadOnlyList<DailySummary>> GetDailyAsync(int cityId, DateOnly from, DateOnly to, CancellationToken ct);
}
