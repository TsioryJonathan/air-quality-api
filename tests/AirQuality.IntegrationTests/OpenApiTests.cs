using System.Net;

namespace AirQuality.IntegrationTests;

public class OpenApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Docs_are_public()
    {
        var client = factory.ClientWithKey(null);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/openapi/v1.yaml")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/scalar/v1")).StatusCode);
    }

    /// <summary>
    /// docs/openapi.yaml is generated from the code. If this fails, an endpoint changed:
    /// run the tests once with UPDATE_OPENAPI=1 to rewrite the file, then commit it with the change.
    /// </summary>
    [Fact]
    public async Task Committed_yaml_matches_the_generated_document()
    {
        var generated = await factory.ClientWithKey(null).GetStringAsync("/openapi/v1.yaml");

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AirQuality.slnx")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        var path = Path.Combine(dir.FullName, "docs", "openapi.yaml");

        if (Environment.GetEnvironmentVariable("UPDATE_OPENAPI") == "1")
        {
            await File.WriteAllTextAsync(path, generated);
            return;
        }

        Assert.Equal(Normalize(generated), Normalize(await File.ReadAllTextAsync(path)));
    }

    private static string Normalize(string s) => s.Replace("\r\n", "\n").TrimEnd();
}
