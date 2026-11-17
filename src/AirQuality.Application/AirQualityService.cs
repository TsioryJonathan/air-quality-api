using AirQuality.Application.Abstractions;
using AirQuality.Domain;

namespace AirQuality.Application;

public enum Granularity { Hourly, Daily }

public sealed class AirQualityService(IAirQualityReader reader)
{
    public const int MaxHourlyDays = 31;
    public const int MaxDailyDays = 366;

    public Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken ct) => reader.GetCitiesAsync(ct);

    public async Task<Measurement> GetLatestAsync(int cityId, CancellationToken ct)
    {
        await EnsureCityAsync(cityId, ct);
        return await reader.GetLatestAsync(cityId, ct)
            ?? throw new NotFoundException($"No measurement for city {cityId}.");
    }

    public async Task<IReadOnlyList<Measurement>> GetHourlyAsync(int cityId, DateTime from, DateTime to, CancellationToken ct)
    {
        CheckRange(from, to, MaxHourlyDays);
        await EnsureCityAsync(cityId, ct);
        return await reader.GetHourlyAsync(cityId, from, to, ct);
    }

    public async Task<IReadOnlyList<DailySummary>> GetDailyAsync(int cityId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        CheckRange(from.ToDateTime(TimeOnly.MinValue), to.ToDateTime(TimeOnly.MinValue), MaxDailyDays);
        await EnsureCityAsync(cityId, ct);
        return await reader.GetDailyAsync(cityId, from, to, ct);
    }

    private async Task EnsureCityAsync(int cityId, CancellationToken ct)
    {
        if (!await reader.CityExistsAsync(cityId, ct))
            throw new NotFoundException($"City {cityId} not found.");
    }

    private static void CheckRange(DateTime from, DateTime to, int maxDays)
    {
        if (from > to)
            throw new ValidationException("'from' must not be after 'to'.");
        if ((to - from).TotalDays > maxDays)
            throw new ValidationException($"The range must not exceed {maxDays} days.");
    }
}
