using Clerk.AspNet.Authorization.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Clerk.AspNet.Authorization.Extensions;

/// <summary>
/// Extension methods for registering Clerk Role Authorization services.
/// Provides convenient helper methods to add the authorization service to the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Clerk Role Authorization service to the dependency injection container.
    /// Registers IClerkRoleAuthorizationService with a scoped lifetime.
    ///
    /// This service is required by the OpaqueTokenValidationMiddleware to perform
    /// role-based authorization checks.
    ///
    /// Prerequisites:
    /// - Configuration must include "Clerk:SecretKey" setting
    /// - IConfiguration must be registered in the service collection
    /// - ILogger&lt;ClerkRoleAuthorizationService&gt; must be available (usually auto-registered)
    ///
    /// Example usage in Program.cs:
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddClerkRoleAuthorization();
    /// </summary>
    /// <param name="services">The service collection to register the authorization service to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddClerkRoleAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IClerkRoleAuthorizationService, ClerkRoleAuthorizationService>();
        return services;
    }
}
