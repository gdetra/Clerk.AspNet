using Clerk.AspNet.Authorization.Authorization;
using Clerk.AspNet.Authorization.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Clerk.AspNet.Authorization.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddClerkRoleAuthorization_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddClerkRoleAuthorization();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IClerkRoleAuthorizationService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void AddClerkRoleAuthorization_RegistersAsScopedService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddClerkRoleAuthorization();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IClerkRoleAuthorizationService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

}
