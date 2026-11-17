using AirQuality.Domain;

namespace AirQuality.Application.Abstractions;

public interface IAlertRuleStore
{
    Task<IReadOnlyList<AlertRule>> ListAsync(CancellationToken ct);
    Task AddAsync(AlertRule rule, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
