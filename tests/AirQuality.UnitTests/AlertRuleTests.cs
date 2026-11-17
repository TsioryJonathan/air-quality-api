using AirQuality.Domain;

namespace AirQuality.UnitTests;

public class AlertRuleTests
{
    private static Measurement Reading(int city, double? pm25) =>
        new(city, new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc), 3, null, null, null, null, null, pm25, null, null);

    [Fact]
    public void Exceeded_when_value_is_above_threshold() =>
        Assert.True(new AlertRule(1, Pollutant.Pm25, 35).IsExceededBy(Reading(1, 40)));

    [Fact]
    public void Not_exceeded_when_value_equals_threshold() =>
        Assert.False(new AlertRule(1, Pollutant.Pm25, 35).IsExceededBy(Reading(1, 35)));

    [Fact]
    public void Missing_value_never_triggers() =>
        Assert.False(new AlertRule(1, Pollutant.Pm25, 35).IsExceededBy(Reading(1, null)));

    [Fact]
    public void Other_city_never_triggers() =>
        Assert.False(new AlertRule(1, Pollutant.Pm25, 35).IsExceededBy(Reading(2, 99)));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Rejects_invalid_threshold(double threshold) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new AlertRule(1, Pollutant.Pm25, threshold));

    [Theory]
    [InlineData(1, AqiLevel.Good)]
    [InlineData(5, AqiLevel.VeryPoor)]
    public void Maps_aqi_to_level(int aqi, AqiLevel expected) => Assert.Equal(expected, AqiLevels.From(aqi));

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Rejects_aqi_out_of_range(int aqi) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => AqiLevels.From(aqi));
}
