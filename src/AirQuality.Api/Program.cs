using System.Text.Json.Serialization;
using AirQuality.Api.Auth;
using AirQuality.Api.Endpoints;
using AirQuality.Application;
using AirQuality.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure();
builder.Services.AddScoped<AirQualityService>();
builder.Services.AddScoped<AlertService>();

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddAuthentication(ApiKeyOptions.Scheme)
    .AddScheme<ApiKeyOptions, ApiKeyHandler>(ApiKeyOptions.Scheme, _ => { });
builder.Services.AddOptions<ApiKeyOptions>(ApiKeyOptions.Scheme).Configure<IConfiguration>((o, config) =>
    o.Keys = config.GetSection("ApiKeys").Get<Dictionary<string, string>>() ?? []);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.CanRead, p => p.RequireRole(Roles.Reader, Roles.Admin))
    .AddPolicy(Policies.CanManageRules, p => p.RequireRole(Roles.Admin));

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler(handler => handler.Run(async ctx =>
{
    var error = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    ctx.Response.StatusCode = error switch
    {
        ValidationException => StatusCodes.Status400BadRequest,
        NotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError,
    };
    // Business messages are safe to show; anything else stays in the logs.
    var detail = error is ValidationException or NotFoundException ? error.Message : null;
    await Results.Problem(detail: detail, statusCode: ctx.Response.StatusCode).ExecuteAsync(ctx);
}));
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AirQualityDbContext>().Database.MigrateAsync();
}

app.MapOpenApi();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapApi();

app.Run();

// Lets the integration tests start the app with WebApplicationFactory<Program>.
public partial class Program;
