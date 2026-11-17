using AirQuality.Api.Auth;
using AirQuality.Application;
using AirQuality.Domain;

namespace AirQuality.Api.Endpoints;

public static class ApiEndpoints
{
    public sealed record CreateRuleRequest(int CityId, Pollutant Pollutant, double Threshold);

    public static void MapApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        var read = api.MapGroup("").RequireAuthorization(Policies.CanRead);
        read.MapGet("/cities", (AirQualityService s, CancellationToken ct) => s.GetCitiesAsync(ct))
            .WithTags("Air quality").WithSummary("List cities");

        read.MapGet("/cities/{cityId:int}/aqi/latest", async (int cityId, AirQualityService s, CancellationToken ct) =>
            Results.Ok(await s.GetLatestAsync(cityId, ct)))
            .WithTags("Air quality").WithSummary("Latest measurement of a city");

        read.MapGet("/cities/{cityId:int}/aqi", async (
            int cityId, DateTime from, DateTime to, Granularity? granularity,
            AirQualityService s, CancellationToken ct) =>
        {
            if (granularity == Granularity.Daily)
                return Results.Ok(await s.GetDailyAsync(cityId, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to), ct));
            return Results.Ok(await s.GetHourlyAsync(cityId, from, to, ct));
        }).WithTags("Air quality").WithSummary("Hourly measurements, or a daily summary with granularity=Daily");

        read.MapGet("/alerts", (AlertService s, CancellationToken ct) => s.EvaluateAsync(ct))
            .WithTags("Alerts").WithSummary("Rules exceeded by each city's latest measurement");
        read.MapGet("/alert-rules", (AlertService s, CancellationToken ct) => s.ListRulesAsync(ct))
            .WithTags("Alerts").WithSummary("List alert rules");

        var admin = api.MapGroup("/alert-rules").RequireAuthorization(Policies.CanManageRules);
        admin.MapPost("", async (CreateRuleRequest r, AlertService s, CancellationToken ct) =>
        {
            var rule = await s.CreateRuleAsync(r.CityId, r.Pollutant, r.Threshold, ct);
            return Results.Created($"/api/alert-rules/{rule.Id}", rule);
        }).WithTags("Alerts").WithSummary("Create an alert rule (admin)");
        admin.MapDelete("/{id:int}", async (int id, AlertService s, CancellationToken ct) =>
        {
            await s.DeleteRuleAsync(id, ct);
            return Results.NoContent();
        }).WithTags("Alerts").WithSummary("Delete an alert rule (admin)");
    }
}
