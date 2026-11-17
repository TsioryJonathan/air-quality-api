using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AirQuality.IntegrationTests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private HttpClient Reader() => factory.ClientWithKey(ApiFactory.ReaderKey);
    private HttpClient Admin() => factory.ClientWithKey(ApiFactory.AdminKey);

    [Fact]
    public async Task Health_is_public() =>
        Assert.Equal(HttpStatusCode.OK, (await factory.ClientWithKey(null).GetAsync("/health")).StatusCode);

    [Fact]
    public async Task Api_without_key_is_401() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.ClientWithKey(null).GetAsync("/api/cities")).StatusCode);

    [Fact]
    public async Task Api_with_wrong_key_is_401() =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.ClientWithKey("nope").GetAsync("/api/cities")).StatusCode);

    [Fact]
    public async Task Lists_cities_sorted_by_name()
    {
        var cities = await Reader().GetFromJsonAsync<JsonElement[]>("/api/cities", Json);
        Assert.Equal(["Antananarivo", "Toamasina"], cities!.Select(c => c.GetProperty("name").GetString()));
    }

    [Fact]
    public async Task Latest_returns_the_most_recent_hour()
    {
        var latest = await Reader().GetFromJsonAsync<JsonElement>("/api/cities/1/aqi/latest", Json);
        Assert.Equal(5, latest.GetProperty("aqi").GetInt32());
        Assert.StartsWith("2026-11-02T00:00:00", latest.GetProperty("at").GetString());
    }

    [Fact]
    public async Task Hourly_series_is_limited_to_the_range_and_ordered()
    {
        var rows = await Reader().GetFromJsonAsync<JsonElement[]>(
            "/api/cities/1/aqi?from=2026-11-01T01:00:00Z&to=2026-11-02T00:00:00Z", Json);
        Assert.Equal([4, 3, 5], rows!.Select(r => r.GetProperty("aqi").GetInt32()));
    }

    [Fact]
    public async Task Daily_series_is_aggregated_by_postgres()
    {
        var days = await Reader().GetFromJsonAsync<JsonElement[]>(
            "/api/cities/1/aqi?from=2026-11-01&to=2026-11-02&granularity=Daily", Json);
        Assert.Equal(2, days!.Length);
        Assert.Equal("2026-11-01", days[0].GetProperty("day").GetString());
        Assert.Equal(3.0, days[0].GetProperty("averageAqi").GetDouble());
        Assert.Equal(60, days[0].GetProperty("maxPm25").GetDouble());
        Assert.Equal(3, days[0].GetProperty("readings").GetInt32());
    }

    [Fact]
    public async Task Unknown_city_is_404() =>
        Assert.Equal(HttpStatusCode.NotFound, (await Reader().GetAsync("/api/cities/99/aqi/latest")).StatusCode);

    [Fact]
    public async Task Inverted_range_is_400() =>
        Assert.Equal(HttpStatusCode.BadRequest,
            (await Reader().GetAsync("/api/cities/1/aqi?from=2026-11-02&to=2026-11-01")).StatusCode);

    [Fact]
    public async Task Reader_cannot_create_rules() =>
        Assert.Equal(HttpStatusCode.Forbidden,
            (await Reader().PostAsJsonAsync("/api/alert-rules", new { cityId = 1, pollutant = "Pm25", threshold = 35 }, Json)).StatusCode);

    [Fact]
    public async Task Admin_creates_a_rule_then_it_triggers_an_alert_then_it_is_deleted()
    {
        var created = await Admin().PostAsJsonAsync("/api/alert-rules", new { cityId = 1, pollutant = "Pm25", threshold = 35 }, Json);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("id").GetInt32();

        // Latest Antananarivo reading has pm2.5 = 90 > 35.
        var alerts = await Reader().GetFromJsonAsync<JsonElement[]>("/api/alerts", Json);
        var alert = Assert.Single(alerts!, a => a.GetProperty("ruleId").GetInt32() == id);
        Assert.Equal("Antananarivo", alert.GetProperty("city").GetString());
        Assert.Equal(90, alert.GetProperty("value").GetDouble());

        Assert.Equal(HttpStatusCode.NoContent, (await Admin().DeleteAsync($"/api/alert-rules/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Admin().DeleteAsync($"/api/alert-rules/{id}")).StatusCode);
    }

    [Fact]
    public async Task Rule_with_negative_threshold_is_400() =>
        Assert.Equal(HttpStatusCode.BadRequest,
            (await Admin().PostAsJsonAsync("/api/alert-rules", new { cityId = 1, pollutant = "Pm25", threshold = -3 }, Json)).StatusCode);
}
