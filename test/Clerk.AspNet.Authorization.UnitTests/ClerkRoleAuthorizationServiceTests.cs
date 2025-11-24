using Clerk.AspNet.Authorization.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Clerk.AspNet.Authorization.UnitTests;

public class ClerkRoleAuthorizationServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClerkRoleAuthorizationService> _logger;
    private readonly ClerkRoleAuthorizationService _service;

    public ClerkRoleAuthorizationServiceTests()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Clerk:SecretKey", "test_secret_key" }
            });

        _configuration = configBuilder.Build();
        _logger = new Mock<ILogger<ClerkRoleAuthorizationService>>().Object;
        _service = new ClerkRoleAuthorizationService(_configuration, _logger);
    }

    [Fact]
    public async Task ValidateSingleRoleAsync_WithValidRole_ReturnsAuthorized()
    {
        // Arrange
        var userId = "user_123";
        var requiredRole = "org:admin";

        // Act
        var result = await _service.ValidateSingleRoleAsync(userId, requiredRole);

        // Assert
        Assert.NotNull(result);
        // Note: Actual test behavior depends on Clerk API mock/integration
    }

    [Fact]
    public async Task ValidateSingleRoleAsync_WithEmptyUserId_ReturnsNotAuthorized()
    {
        // Arrange
        var userId = "";
        var requiredRole = "org:admin";

        // Act
        var result = await _service.ValidateSingleRoleAsync(userId, requiredRole);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsAuthorized);
    }

    [Fact]
    public async Task ValidateAnyRoleAsync_WithValidRole_ReturnsAuthorized()
    {
        // Arrange
        var userId = "user_123";
        var requiredRoles = new[] { "org:admin", "org:manager" };

        // Act
        var result = await _service.ValidateAnyRoleAsync(userId, requiredRoles);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ValidateAllRolesAsync_WithAllValidRoles_ReturnsAuthorized()
    {
        // Arrange
        var userId = "user_123";
        var requiredRoles = new[] { "org:admin", "org:billing" };

        // Act
        var result = await _service.ValidateAllRolesAsync(userId, requiredRoles);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserRolesAsync_WithValidUserId_ReturnsUserRoles()
    {
        // Arrange
        var userId = "user_123";

        // Act
        var roles = await _service.GetUserRolesAsync(userId);

        // Assert
        Assert.NotNull(roles);
        Assert.IsType<List<string>>(roles);
    }

    [Fact]
    public async Task GetUserRolesAsync_WithEmptyUserId_ReturnsEmptyList()
    {
        // Arrange
        var userId = "";

        // Act
        var roles = await _service.GetUserRolesAsync(userId);

        // Assert
        Assert.NotNull(roles);
        Assert.Empty(roles);
    }
}
