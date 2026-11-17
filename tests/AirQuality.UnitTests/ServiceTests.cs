using AirQuality.Application;
using AirQuality.Domain;

namespace AirQuality.UnitTests;

public class ServiceTests
{
    private readonly FakeReader _reader = new();
    private readonly FakeRuleStore _rules = new();

    [Fact]
    public async Task Hourly_rejects_inverted_range() =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            new AirQualityService(_reader).GetHourlyAsync(1, new(2026, 11, 2), new(2026, 11, 1), default));

    [Fact]
    public async Task Hourly_rejects_range_over_limit() =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            new AirQualityService(_reader).GetHourlyAsync(1, new(2026, 1, 1), new(2026, 6, 1), default));

    [Fact]
    public async Task Unknown_city_is_not_found() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new AirQualityService(_reader).GetLatestAsync(99, default));

    [Fact]
    public async Task Creating_rule_for_unknown_city_is_not_found() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new AlertService(_reader, _rules).CreateRuleAsync(99, Pollutant.Pm25, 35, default));

    [Fact]
    public async Task Creating_rule_with_bad_threshold_is_a_validation_error() =>
        await Assert.ThrowsAsync<ValidationException>(() =>
            new AlertService(_reader, _rules).CreateRuleAsync(1, Pollutant.Pm25, -1, default));

    [Fact]
    public async Task Evaluate_returns_only_exceeded_rules()
    {
        _reader.Latest.Add(new(1, new(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc), 4, null, null, null, null, null, 50, 20, null));
        _rules.Rules.Add(new AlertRule(1, Pollutant.Pm25, 35));   // 50 > 35: triggered
        _rules.Rules.Add(new AlertRule(1, Pollutant.Pm10, 45));   // 20 < 45: quiet

        var alerts = await new AlertService(_reader, _rules).EvaluateAsync(default);

        var alert = Assert.Single(alerts);
        Assert.Equal("Pm25", alert.Pollutant);
        Assert.Equal("Antananarivo", alert.City);
        Assert.Equal(50, alert.Value);
    }

    [Fact]
    public async Task Evaluate_without_rules_returns_nothing() =>
        Assert.Empty(await new AlertService(_reader, _rules).EvaluateAsync(default));
}
