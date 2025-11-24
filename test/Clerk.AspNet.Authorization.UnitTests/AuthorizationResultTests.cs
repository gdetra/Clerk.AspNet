using Clerk.AspNet.Authorization.Authorization;

namespace Clerk.AspNet.Authorization.UnitTests;

public class AuthorizationResultTests
{
    [Fact]
    public void AuthorizationResult_PropertiesCanBeSet()
    {
        // Arrange
        var isAuthorized = true;
        var userRoles = new List<string> { "org:admin", "org:billing" };
        var errorMessage = "User not authorized";
        var authorizedRole = "org:admin";

        // Act
        var result = new AuthorizationResult
        {
            IsAuthorized = isAuthorized,
            UserRoles = userRoles,
            ErrorMessage = errorMessage,
            AuthorizedRole = authorizedRole
        };

        // Assert
        Assert.Equal(isAuthorized, result.IsAuthorized);
        Assert.Equal(userRoles, result.UserRoles);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(authorizedRole, result.AuthorizedRole);
    }

    [Fact]
    public void AuthorizationResult_DefaultValues()
    {
        // Arrange & Act
        var result = new AuthorizationResult();

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equivalent(new List<string>(), result.UserRoles);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.AuthorizedRole);
    }

    [Fact]
    public void AuthorizationResult_CanCreateSuccessful()
    {
        // Arrange & Act
        var result = new AuthorizationResult
        {
            IsAuthorized = true,
            UserRoles = new List<string> { "org:admin" },
            AuthorizedRole = "org:admin"
        };

        // Assert
        Assert.True(result.IsAuthorized);
        Assert.NotNull(result.UserRoles);
        Assert.Equal("org:admin", result.AuthorizedRole);
    }

    [Fact]
    public void AuthorizationResult_CanCreateUnsuccessful()
    {
        // Arrange & Act
        var result = new AuthorizationResult
        {
            IsAuthorized = false,
            ErrorMessage = "Required role not found"
        };

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal("Required role not found", result.ErrorMessage);
    }
}
