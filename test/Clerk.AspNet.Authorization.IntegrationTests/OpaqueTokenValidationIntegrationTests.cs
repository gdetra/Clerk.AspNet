using System.Net;

namespace Clerk.AspNet.Authorization.IntegrationTests;

/// <summary>
/// Integration tests for opaque token validation middleware.
/// Tests the full request/response cycle with mocked Clerk API.
/// </summary>
public class OpaqueTokenValidationIntegrationTests : IAsyncLifetime
{
    private TestApiFactory _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _factory = new TestApiFactory();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task PublicEndpoint_WithoutToken_Returns200()
    {
        // Arrange
        _factory.ResetMocks();

        // Act
        var response = await _client.GetAsync("/public");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Public endpoint", content);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        _factory.ResetMocks();

        // Act
        var response = await _client.GetAsync("/protected");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidOpaqueToken_Returns200()
    {
        // Arrange
        _factory.ResetMocks();
        _factory.MockSuccessfulTokenValidation("user_123");

        var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request.Headers.Add("Authorization", "Bearer oat_valid_token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Protected endpoint", content);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidOpaqueToken_Returns401()
    {
        // Arrange
        _factory.ResetMocks();
        _factory.MockFailedTokenValidation();

        var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request.Headers.Add("Authorization", "Bearer oat_invalid_token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithRevokedToken_Returns401()
    {
        // Arrange
        _factory.ResetMocks();
        _factory.MockRevokedToken("user_123");

        var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request.Headers.Add("Authorization", "Bearer oat_revoked_token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_Returns401()
    {
        // Arrange
        _factory.ResetMocks();
        _factory.MockExpiredToken("user_123");

        var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request.Headers.Add("Authorization", "Bearer oat_expired_token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithJWTToken_Returns200()
    {
        // Arrange
        _factory.ResetMocks();
        var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        var request = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request.Headers.Add("Authorization", $"Bearer {jwtToken}");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        _factory.ResetMocks();

        // Act
        var response = await _client.GetAsync("/admin");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ManagerEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        _factory.ResetMocks();

        // Act
        var response = await _client.GetAsync("/manager");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task BillingEndpoint_WithoutToken_Returns401()
    {
        // Arrange
        _factory.ResetMocks();

        // Act
        var response = await _client.GetAsync("/billing");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_AlwaysReturns200()
    {
        // Arrange
        _factory.ResetMocks();

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MultipleRequests_WithDifferentTokens_WorkCorrectly()
    {
        // Arrange
        _factory.ResetMocks();
        _factory.MockSuccessfulTokenValidation("user_123");

        // Act - First request
        var request1 = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request1.Headers.Add("Authorization", "Bearer oat_token_1");
        var response1 = await _client.SendAsync(request1);

        // Reset and mock different token
        _factory.ResetMocks();
        _factory.MockSuccessfulTokenValidation("user_456");

        // Act - Second request with different token
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/protected");
        request2.Headers.Add("Authorization", "Bearer oat_token_2");
        var response2 = await _client.SendAsync(request2);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }
}
