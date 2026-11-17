using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AirQuality.Api.Auth;

public static class Roles
{
    public const string Reader = "reader";
    public const string Admin = "admin";
}

public static class Policies
{
    public const string CanRead = "CanRead";
    public const string CanManageRules = "CanManageRules";
}

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    /// <summary>Key (secret) to role. Loaded from configuration, never hard-coded.</summary>
    public Dictionary<string, string> Keys { get; set; } = [];
}

public sealed class ApiKeyHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var provided) || provided.Count != 1)
            return Task.FromResult(AuthenticateResult.NoResult());

        var providedBytes = Encoding.UTF8.GetBytes(provided.ToString());
        foreach (var (key, role) in Options.Keys)
        {
            // Constant-time comparison: the response time must not reveal a valid prefix.
            if (CryptographicOperations.FixedTimeEquals(providedBytes, Encoding.UTF8.GetBytes(key)))
            {
                var identity = new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, $"key:{role}"), new Claim(ClaimTypes.Role, role)], Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(
                    new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
            }
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
    }
}
