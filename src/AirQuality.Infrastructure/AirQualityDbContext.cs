using AirQuality.Domain;
using Microsoft.EntityFrameworkCore;

namespace AirQuality.Infrastructure;

public sealed class AirQualityDbContext(DbContextOptions<AirQualityDbContext> options) : DbContext(options)
{
    internal DbSet<CityRow> Cities => Set<CityRow>();
    internal DbSet<FactRow> Facts => Set<FactRow>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Owned by the ETL: mapped for reading, never created or altered by migrations.
        b.Entity<CityRow>(e =>
        {
            e.ToTable("dim_city", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.CityKey);
            e.Property(x => x.CityKey).HasColumnName("city_key");
            e.Property(x => x.CityName).HasColumnName("city_name");
            e.Property(x => x.Country).HasColumnName("country");
            e.Property(x => x.Latitude).HasColumnName("latitude");
            e.Property(x => x.Longitude).HasColumnName("longitude");
        });

        b.Entity<FactRow>(e =>
        {
            e.ToTable("fact_aqi", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.FactId);
            e.Property(x => x.FactId).HasColumnName("fact_id");
            e.Property(x => x.CityKey).HasColumnName("city_key");
            e.Property(x => x.DateKey).HasColumnName("date_key");
            e.Property(x => x.Aqi).HasColumnName("aqi");
            e.Property(x => x.Co).HasColumnName("co");
            e.Property(x => x.No).HasColumnName("no");
            e.Property(x => x.No2).HasColumnName("no2");
            e.Property(x => x.O3).HasColumnName("o3");
            e.Property(x => x.So2).HasColumnName("so2");
            e.Property(x => x.Pm25).HasColumnName("pm2_5");
            e.Property(x => x.Pm10).HasColumnName("pm10");
            e.Property(x => x.Nh3).HasColumnName("nh3");
        });

        // Owned by this API: created by migrations.
        b.Entity<AlertRule>(e =>
        {
            e.ToTable("alert_rules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            e.Property(x => x.CityId).HasColumnName("city_id");
            e.Property(x => x.Pollutant).HasColumnName("pollutant").HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.Threshold).HasColumnName("threshold");
            e.ToTable(t => t.HasCheckConstraint("ck_alert_rules_threshold", "threshold > 0"));
        });
    }
}
