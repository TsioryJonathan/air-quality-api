using AirQuality.Application.Abstractions;
using AirQuality.Domain;

namespace AirQuality.Application;

public sealed class AlertService(IAirQualityReader reader, IAlertRuleStore rules)
{
    public Task<IReadOnlyList<AlertRule>> ListRulesAsync(CancellationToken ct) => rules.ListAsync(ct);

    public async Task<AlertRule> CreateRuleAsync(int cityId, Pollutant pollutant, double threshold, CancellationToken ct)
    {
        if (!Enum.IsDefined(pollutant))
            throw new ValidationException("Unknown pollutant.");
        if (!await reader.CityExistsAsync(cityId, ct))
            throw new NotFoundException($"City {cityId} not found.");

        AlertRule rule;
        try
        {
            rule = new AlertRule(cityId, pollutant, threshold);
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new ValidationException(e.Message);
        }

        await rules.AddAsync(rule, ct);
        return rule;
    }

    public async Task DeleteRuleAsync(int id, CancellationToken ct)
    {
        if (!await rules.DeleteAsync(id, ct))
            throw new NotFoundException($"Rule {id} not found.");
    }

    /// <summary>Compares the latest measurement of each city with every rule.</summary>
    public async Task<IReadOnlyList<TriggeredAlert>> EvaluateAsync(CancellationToken ct)
    {
        var allRules = await rules.ListAsync(ct);
        if (allRules.Count == 0) return [];

        var cities = (await reader.GetCitiesAsync(ct)).ToDictionary(c => c.Id, c => c.Name);
        var latest = await reader.GetLatestForAllCitiesAsync(ct);

        return
        [
            .. from rule in allRules
               from m in latest
               where rule.IsExceededBy(m)
               select new TriggeredAlert(
                   rule.Id, rule.CityId, cities.GetValueOrDefault(rule.CityId, "?"),
                   rule.Pollutant.ToString(), rule.Threshold, m.ValueOf(rule.Pollutant)!.Value, m.At),
        ];
    }
}
