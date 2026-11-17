using System.Globalization;
using AirQuality.Application;
using AirQuality.Application.Abstractions;
using AirQuality.Domain;
using Microsoft.EntityFrameworkCore;

namespace AirQuality.Infrastructure;

internal sealed class AirQualityReader(AirQualityDbContext db) : IAirQualityReader
{
    public async Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken ct) =>
        await db.Cities.AsNoTracking()
            .OrderBy(c => c.CityName)
            .Select(c => new CityDto(c.CityKey, c.CityName, c.Country, c.Latitude, c.Longitude))
            .ToListAsync(ct);

    public Task<bool> CityExistsAsync(int cityId, CancellationToken ct) =>
        db.Cities.AnyAsync(c => c.CityKey == cityId, ct);

    public async Task<Measurement?> GetLatestAsync(int cityId, CancellationToken ct)
    {
        var row = await db.Facts.AsNoTracking()
            .Where(f => f.CityKey == cityId)
            .OrderByDescending(f => f.DateKey)
            .FirstOrDefaultAsync(ct);
        return row is null ? null : ToMeasurement(row);
    }

    public async Task<IReadOnlyList<Measurement>> GetLatestForAllCitiesAsync(CancellationToken ct)
    {
        // Latest date_key per city, then the matching rows.
        var latestKeys = db.Facts.GroupBy(f => f.CityKey).Select(g => new { CityKey = g.Key, DateKey = g.Max(f => f.DateKey) });
        var rows = await db.Facts.AsNoTracking()
            .Join(latestKeys, f => new { f.CityKey, f.DateKey }, k => new { k.CityKey, k.DateKey }, (f, _) => f)
            .ToListAsync(ct);
        return [.. rows.Select(ToMeasurement)];
    }

    public async Task<IReadOnlyList<Measurement>> GetHourlyAsync(int cityId, DateTime from, DateTime to, CancellationToken ct)
    {
        int fromKey = ToDateKey(from), toKey = ToDateKey(to);
        var rows = await db.Facts.AsNoTracking()
            .Where(f => f.CityKey == cityId && f.DateKey >= fromKey && f.DateKey <= toKey)
            .OrderBy(f => f.DateKey)
            .ToListAsync(ct);
        return [.. rows.Select(ToMeasurement)];
    }

    public async Task<IReadOnlyList<DailySummary>> GetDailyAsync(int cityId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        int fromKey = ToDateKey(from.ToDateTime(TimeOnly.MinValue));
        int toKey = ToDateKey(to.ToDateTime(new TimeOnly(23)));

        // date_key / 100 = YYYYMMDD: the day, computed by PostgreSQL.
        var rows = await db.Facts.AsNoTracking()
            .Where(f => f.CityKey == cityId && f.DateKey >= fromKey && f.DateKey <= toKey)
            .GroupBy(f => f.DateKey / 100)
            .Select(g => new
            {
                Day = g.Key,
                AverageAqi = g.Average(f => f.Aqi),
                MaxAqi = g.Max(f => f.Aqi),
                MaxPm25 = g.Max(f => f.Pm25),
                Readings = g.Count(),
            })
            .OrderBy(g => g.Day)
            .ToListAsync(ct);

        return [.. rows.Select(r => new DailySummary(
            DateOnly.ParseExact(r.Day.ToString(CultureInfo.InvariantCulture), "yyyyMMdd", CultureInfo.InvariantCulture),
            r.AverageAqi.HasValue ? Math.Round(r.AverageAqi.Value, 2) : null,
            r.MaxAqi, r.MaxPm25, r.Readings))];
    }

    private static int ToDateKey(DateTime utc) =>
        int.Parse(utc.ToString("yyyyMMddHH", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

    private static Measurement ToMeasurement(FactRow f) => new(
        f.CityKey,
        DateTime.ParseExact(f.DateKey.ToString(CultureInfo.InvariantCulture), "yyyyMMddHH",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
        f.Aqi, f.Co, f.No, f.No2, f.O3, f.So2, f.Pm25, f.Pm10, f.Nh3);
}
