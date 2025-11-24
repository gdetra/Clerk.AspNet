using Clerk.AspNet.Authorization.Attributes;
using Clerk.AspNet.Authorization.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Clerk.AspNet.Authorization.Extensions;

/// <summary>
/// Extension methods for opaque token validation middleware registration and endpoint configuration.
/// Provides fluent API for configuring Clerk role-based authorization on minimal API endpoints.
/// </summary>
public static class OpaqueTokenValidationExtensions
{
    /// <summary>
    /// Registers the opaque token validation middleware in the application pipeline.
    /// Must be called before mapping endpoints to enable token validation.
    ///
    /// Example:
    /// app.UseOpaqueTokenValidation();
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseOpaqueTokenValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OpaqueTokenValidationMiddleware>();
    }

    /// <summary>
    /// Marks a minimal API endpoint as requiring opaque token validation.
    /// This is an alternative to .RequireAuthorization() that uses custom Clerk token logic.
    /// Automatically validates that a Bearer token is provided and is a valid opaque token.
    ///
    /// Returns 401 Unauthorized if:
    /// - No Authorization header is provided
    /// - The token is invalid or expired
    /// - Clerk:SecretKey is not configured
    ///
    /// Example:
    /// app.MapGet("/protected", handler).RequireOpaqueTokenAuthorization();
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder RequireOpaqueTokenAuthorization(this RouteHandlerBuilder builder)
    {
        return builder.WithMetadata(new RequireOpaqueTokenAuthorizationAttribute());
    }

    /// <summary>
    /// Marks a minimal API endpoint as requiring role-based authorization.
    /// Returns a fluent builder for specifying role authorization semantics.
    /// Automatically includes token validation requirement.
    ///
    /// Supports three authorization modes:
    /// - .Single(role): User must have exactly this role
    /// - .OneOf(roles): User must have at least one of these roles
    /// - .AllOf(roles): User must have all of these roles
    ///
    /// Returns 403 Forbidden if role authorization fails.
    /// Returns 401 Unauthorized if token is invalid or missing.
    ///
    /// Example:
    /// app.MapGet("/admin", handler).RequireRole().Single("org:admin");
    /// app.MapGet("/moderator", handler).RequireRole().OneOf("org:admin", "org:manager");
    /// app.MapGet("/billing", handler).RequireRole().AllOf("org:admin", "org:billing");
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>A fluent builder for specifying role requirements.</returns>
    public static ClerkAuthorizationBuilder RequireRole(this RouteHandlerBuilder builder)
    {
        return new ClerkAuthorizationBuilder(builder);
    }
}

/// <summary>
/// Fluent builder for specifying role-based authorization requirements on endpoints.
/// Provides clear, semantic methods for defining authorization logic.
///
/// Enables three distinct authorization patterns:
/// 1. Single role requirement: .Single("org:admin")
/// 2. Any of roles: .OneOf("org:admin", "org:manager")
/// 3. All roles: .AllOf("org:admin", "org:billing")
///
/// All methods automatically include token validation via RequireOpaqueTokenAuthorization.
/// </summary>
public class ClerkAuthorizationBuilder
{
    private readonly RouteHandlerBuilder _routeBuilder;

    /// <summary>
    /// Initializes a new instance of the ClerkAuthorizationBuilder.
    /// </summary>
    /// <param name="routeBuilder">The route handler builder to decorate.</param>
    internal ClerkAuthorizationBuilder(RouteHandlerBuilder routeBuilder)
    {
        _routeBuilder = routeBuilder;
        // Always add base token authorization requirement
        _routeBuilder.WithMetadata(new RequireOpaqueTokenAuthorizationAttribute());
    }

    /// <summary>
    /// Require user to have exactly this specific role.
    /// User must have this exact role for authorization to succeed.
    ///
    /// Returns 403 Forbidden if user doesn't have this exact role.
    ///
    /// Example:
    /// .RequireRole().Single("org:admin")
    /// </summary>
    /// <param name="role">The exact role required (e.g., "org:admin").</param>
    /// <returns>The route handler builder for chaining.</returns>
    public RouteHandlerBuilder Single(string role)
    {
        return _routeBuilder.WithMetadata(new RequireRoleAttribute(role));
    }

    /// <summary>
    /// Require user to have at least one of the specified roles.
    /// User must have at least one of these roles for authorization to succeed.
    ///
    /// Returns 403 Forbidden if user doesn't have any of these roles.
    ///
    /// Example:
    /// .RequireRole().OneOf("org:admin", "org:manager")
    /// </summary>
    /// <param name="roles">Array of roles where user must have at least one.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public RouteHandlerBuilder OneOf(params string[] roles)
    {
        return _routeBuilder.WithMetadata(new RequireAnyRoleAttribute(roles));
    }

    /// <summary>
    /// Require user to have all of the specified roles.
    /// User must have every role in the array for authorization to succeed.
    ///
    /// Returns 403 Forbidden if user doesn't have all of these roles.
    ///
    /// Example:
    /// .RequireRole().AllOf("org:admin", "org:billing")
    /// </summary>
    /// <param name="roles">Array of roles that user must ALL possess.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public RouteHandlerBuilder AllOf(params string[] roles)
    {
        return _routeBuilder.WithMetadata(new RequireAllRolesAttribute(roles));
    }
}
