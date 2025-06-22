using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cellarium.Tests.Infrastructure;

/// <summary>
/// Test authentication scheme that bypasses real authentication for API testing.
/// Uses custom header format: Authorization: Test {userId}
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, 
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        
        if (authHeader == null || !authHeader.StartsWith($"{SchemeName} "))
        {
            return Task.FromResult(AuthenticateResult.Fail("No test authentication header found"));
        }

        var userId = authHeader.Substring($"{SchemeName} ".Length);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test user ID"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("scope", "cellarium")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Extensions for configuring test authentication
/// </summary>
public static class TestAuthenticationExtensions
{
    public static AuthenticationBuilder AddTestAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
            TestAuthenticationHandler.SchemeName, 
            options => { });
    }
}