using AirQuality.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace AirQuality.IntegrationTests;

/// <summary>
/// One real PostgreSQL 17 container per test class, with the ETL star schema and a small known dataset.
/// Set TEST_POSTGRES_IMAGE to use another image (default: postgres:17).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ReaderKey = "test-reader";
    public const string AdminKey = "test-admin";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder(
        Environment.GetEnvironmentVariable("TEST_POSTGRES_IMAGE") ?? "postgres:17").Build();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
        await using var conn = new NpgsqlConnection(_db.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "star-schema.sql"))
                          + await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "seed.sql"));
        await cmd.ExecuteNonQueryAsync();

        // Only migrate the disposable PostgreSQL container created by this fixture.
        var options = new DbContextOptionsBuilder<AirQualityDbContext>()
            .UseNpgsql(_db.GetConnectionString()).Options;
        await using var db = new AirQualityDbContext(options);
        await db.Database.MigrateAsync();
    }

    public new Task DisposeAsync() => _db.DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:AirQuality"] = _db.GetConnectionString(),
            [$"ApiKeys:{ReaderKey}"] = "reader",
            [$"ApiKeys:{AdminKey}"] = "admin",
        }));
    }

    public HttpClient ClientWithKey(string? key)
    {
        var client = CreateClient();
        if (key is not null) client.DefaultRequestHeaders.Add("X-Api-Key", key);
        return client;
    }
}
