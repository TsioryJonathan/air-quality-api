using AirQuality.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirQuality.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // The connection string is read when the context is first resolved, so test hosts can override it.
        services.AddDbContext<AirQualityDbContext>((sp, o) => o.UseNpgsql(
            sp.GetRequiredService<IConfiguration>().GetConnectionString("AirQuality")
            ?? throw new InvalidOperationException("Missing connection string 'AirQuality'.")));
        services.AddScoped<IAirQualityReader, AirQualityReader>();
        services.AddScoped<IAlertRuleStore, AlertRuleStore>();
        return services;
    }
}
