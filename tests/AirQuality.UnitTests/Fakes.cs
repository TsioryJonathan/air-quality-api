using AirQuality.Application;
using AirQuality.Application.Abstractions;
using AirQuality.Domain;

namespace AirQuality.UnitTests;

internal sealed class FakeReader : IAirQualityReader
{
    public List<CityDto> Cities { get; } = [new(1, "Antananarivo", "MG", -18.88, 47.51)];
    public List<Measurement> Latest { get; } = [];

    public Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<CityDto>>(Cities);
    public Task<bool> CityExistsAsync(int cityId, CancellationToken ct) => Task.FromResult(Cities.Any(c => c.Id == cityId));
    public Task<Measurement?> GetLatestAsync(int cityId, CancellationToken ct) => Task.FromResult(Latest.FirstOrDefault(m => m.CityId == cityId));
    public Task<IReadOnlyList<Measurement>> GetLatestForAllCitiesAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<Measurement>>(Latest);
    public Task<IReadOnlyList<Measurement>> GetHourlyAsync(int cityId, DateTime from, DateTime to, CancellationToken ct) => Task.FromResult<IReadOnlyList<Measurement>>([]);
    public Task<IReadOnlyList<DailySummary>> GetDailyAsync(int cityId, DateOnly from, DateOnly to, CancellationToken ct) => Task.FromResult<IReadOnlyList<DailySummary>>([]);
}

internal sealed class FakeRuleStore : IAlertRuleStore
{
    public List<AlertRule> Rules { get; } = [];
    public Task<IReadOnlyList<AlertRule>> ListAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<AlertRule>>(Rules);
    public Task AddAsync(AlertRule rule, CancellationToken ct) { Rules.Add(rule); return Task.CompletedTask; }
    public Task<bool> DeleteAsync(int id, CancellationToken ct) => Task.FromResult(Rules.RemoveAll(r => r.Id == id) > 0);
}
