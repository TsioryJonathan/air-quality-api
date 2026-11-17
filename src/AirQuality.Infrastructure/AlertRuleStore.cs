using AirQuality.Application.Abstractions;
using AirQuality.Domain;
using Microsoft.EntityFrameworkCore;

namespace AirQuality.Infrastructure;

internal sealed class AlertRuleStore(AirQualityDbContext db) : IAlertRuleStore
{
    public async Task<IReadOnlyList<AlertRule>> ListAsync(CancellationToken ct) =>
        await db.AlertRules.AsNoTracking().OrderBy(r => r.Id).ToListAsync(ct);

    public async Task AddAsync(AlertRule rule, CancellationToken ct)
    {
        db.AlertRules.Add(rule);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await db.AlertRules.Where(r => r.Id == id).ExecuteDeleteAsync(ct) > 0;
}
