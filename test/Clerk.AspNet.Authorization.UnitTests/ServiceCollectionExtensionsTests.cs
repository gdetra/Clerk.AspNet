using Clerk.AspNet.Authorization.Authorization;
using Clerk.AspNet.Authorization.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Clerk.AspNet.Authorization.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddClerkRoleAuthorization_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Add required dependencies
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Clerk:SecretKey", "test_secret_key" }
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

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
