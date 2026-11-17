using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AirQuality.Api.OpenApi;

/// <summary>Declares the X-Api-Key header in the OpenAPI document so API clients (and Scalar) can send it.</summary>
internal sealed class ApiKeySecurityTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "ApiKey";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken ct)
    {
        document.Info.Title = "Air Quality API";
        document.Info.Description = "Air quality measurements for six cities in Madagascar, with threshold-based alert rules.";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "X-Api-Key",
            Description = "API key. Its role (reader or admin) decides which routes are allowed.",
        };

        document.Security =
        [
            new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(SchemeName, document)] = [] },
        ];
        return Task.CompletedTask;
    }
}
