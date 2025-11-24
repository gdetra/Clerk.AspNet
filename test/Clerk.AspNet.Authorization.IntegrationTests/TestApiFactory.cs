using Clerk.AspNet.Authorization.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;

namespace Clerk.AspNet.Authorization.IntegrationTests;

/// <summary>
/// Factory for creating a test web API with mocked Clerk Backend API.
/// Uses WebApplicationFactory to set up a minimal API for testing.
/// </summary>
public class TestApiFactory : WebApplicationFactory<object>
{
    private readonly WireMockServer _mockClerkServer;
    public string MockClerkBaseUrl => _mockClerkServer.Urls.First();

    public TestApiFactory()
    {
        _mockClerkServer = WireMockServer.Start();
    }

    /// <summary>
    /// Creates the web app builder and configures the application.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Add Clerk role authorization
            services.AddClerkRoleAuthorization();
        });

        builder.Configure(app =>
        {
            // Add the opaque token validation middleware
            app.UseOpaqueTokenValidation();

            // Map test endpoints
            MapTestEndpoints(app);
        });
    }

    /// <summary>
    /// Maps test endpoints for various authorization scenarios.
    /// </summary>
    private void MapTestEndpoints(IApplicationBuilder app)
    {
        var webApp = app as WebApplication ?? throw new InvalidOperationException("Application is not a WebApplication");

        // Public endpoint - no authorization required
        webApp.MapGet("/public", () => new { message = "Public endpoint" })
            .WithName("PublicEndpoint");

        // Protected endpoint - token validation required
        webApp.MapGet("/protected", () => new { message = "Protected endpoint" })
            .RequireOpaqueTokenAuthorization()
            .WithName("ProtectedEndpoint");

        // Admin endpoint - requires single role
        webApp.MapGet("/admin", () => new { message = "Admin endpoint" })
            .RequireRole()
            .Single("org:admin")
            .WithName("AdminEndpoint");

        // Manager endpoint - requires any role
        webApp.MapGet("/manager", () => new { message = "Manager endpoint" })
            .RequireRole()
            .OneOf("org:admin", "org:manager")
            .WithName("ManagerEndpoint");

        // Billing endpoint - requires all roles
        webApp.MapGet("/billing", () => new { message = "Billing endpoint" })
            .RequireRole()
            .AllOf("org:admin", "org:billing")
            .WithName("BillingEndpoint");

        // Health check endpoint
        webApp.MapGet("/health", () => new { status = "ok" })
            .WithName("HealthCheck");
    }

    /// <summary>
    /// Sets up a successful token validation response from mock Clerk API.
    /// </summary>
    public void MockSuccessfulTokenValidation(string userId = "user_123")
    {
        _mockClerkServer.Given(WireMock.RequestBuilders.Request
            .Create()
            .WithPath("/v1/oauth_applications/access_tokens/verify")
            .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response
            .Create()
            .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                {
                    "object": "oauth_access_token",
                    "id": "oat_test_token",
                    "subject": "{{userId}}",
                    "issued_at": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}},
                    "expires_at": {{(DateTimeOffset.UtcNow.AddHours(1)).ToUnixTimeSeconds()}},
                    "revoked": false,
                    "expired": false
                }
                """));
    }

    /// <summary>
    /// Sets up a failed token validation response from mock Clerk API.
    /// </summary>
    public void MockFailedTokenValidation()
    {
        _mockClerkServer.Given(WireMock.RequestBuilders.Request
            .Create()
            .WithPath("/v1/oauth_applications/access_tokens/verify")
            .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response
            .Create()
            .WithStatusCode(401)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                    "errors": [
                        {
                            "message": "Invalid token"
                        }
                    ]
                }
                """));
    }

    /// <summary>
    /// Sets up a revoked token response from mock Clerk API.
    /// </summary>
    public void MockRevokedToken(string userId = "user_123")
    {
        _mockClerkServer.Given(WireMock.RequestBuilders.Request
            .Create()
            .WithPath("/v1/oauth_applications/access_tokens/verify")
            .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response
            .Create()
            .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                {
                    "object": "oauth_access_token",
                    "id": "oat_revoked_token",
                    "subject": "{{userId}}",
                    "issued_at": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}},
                    "expires_at": {{(DateTimeOffset.UtcNow.AddHours(1)).ToUnixTimeSeconds()}},
                    "revoked": true,
                    "expired": false
                }
                """));
    }

    /// <summary>
    /// Sets up an expired token response from mock Clerk API.
    /// </summary>
    public void MockExpiredToken(string userId = "user_123")
    {
        _mockClerkServer.Given(WireMock.RequestBuilders.Request
            .Create()
            .WithPath("/v1/oauth_applications/access_tokens/verify")
            .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response
            .Create()
            .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                {
                    "object": "oauth_access_token",
                    "id": "oat_expired_token",
                    "subject": "{{userId}}",
                    "issued_at": {{(DateTimeOffset.UtcNow.AddHours(-2)).ToUnixTimeSeconds()}},
                    "expires_at": {{(DateTimeOffset.UtcNow.AddMinutes(-5)).ToUnixTimeSeconds()}},
                    "revoked": false,
                    "expired": true
                }
                """));
    }

    /// <summary>
    /// Sets up a user organization memberships response for role validation.
    /// </summary>
    public void MockUserOrganizationMemberships(string userId, params string[] roles)
    {
        var memberships = roles.Select((role, index) => $$"""
        {
            "id": "orgm_{{index}}",
            "object": "organization_membership",
            "role": "{{role}}",
            "organization_id": "org_{{index}}"
        }
        """).ToList();

        var membershipsJson = string.Join(",", memberships);

        _mockClerkServer.Given(WireMock.RequestBuilders.Request
            .Create()
            .WithPath($"/v1/users/{userId}/organization_memberships")
            .UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response
            .Create()
            .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody($$"""
                {
                    "object": "list",
                    "data": [{{membershipsJson}}],
                    "total_count": {{roles.Length}}
                }
                """));
    }

    /// <summary>
    /// Resets all mock responses.
    /// </summary>
    public void ResetMocks()
    {
        _mockClerkServer.Reset();
    }

    /// <summary>
    /// Destructor to clean up the mock server.
    /// </summary>
    ~TestApiFactory()
    {
        _mockClerkServer.Stop();
        _mockClerkServer.Dispose();
    }
}
