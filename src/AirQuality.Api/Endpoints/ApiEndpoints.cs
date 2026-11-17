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
        read.MapGet("/cities", (AirQualityService s, CancellationToken ct) => s.GetCitiesAsync(ct));

        read.MapGet("/cities/{cityId:int}/aqi/latest", async (int cityId, AirQualityService s, CancellationToken ct) =>
            Results.Ok(await s.GetLatestAsync(cityId, ct)));

        read.MapGet("/cities/{cityId:int}/aqi", async (
            int cityId, DateTime from, DateTime to, Granularity? granularity,
            AirQualityService s, CancellationToken ct) =>
        {
            if (granularity == Granularity.Daily)
                return Results.Ok(await s.GetDailyAsync(cityId, DateOnly.FromDateTime(from), DateOnly.FromDateTime(to), ct));
            return Results.Ok(await s.GetHourlyAsync(cityId, from, to, ct));
        });

        read.MapGet("/alerts", (AlertService s, CancellationToken ct) => s.EvaluateAsync(ct));
        read.MapGet("/alert-rules", (AlertService s, CancellationToken ct) => s.ListRulesAsync(ct));

        var admin = api.MapGroup("/alert-rules").RequireAuthorization(Policies.CanManageRules);
        admin.MapPost("", async (CreateRuleRequest r, AlertService s, CancellationToken ct) =>
        {
            var rule = await s.CreateRuleAsync(r.CityId, r.Pollutant, r.Threshold, ct);
            return Results.Created($"/api/alert-rules/{rule.Id}", rule);
        });
        admin.MapDelete("/{id:int}", async (int id, AlertService s, CancellationToken ct) =>
        {
            await s.DeleteRuleAsync(id, ct);
            return Results.NoContent();
        });
    }
}
