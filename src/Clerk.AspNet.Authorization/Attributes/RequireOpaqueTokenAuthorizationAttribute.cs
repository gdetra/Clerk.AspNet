namespace Clerk.AspNet.Authorization.Attributes;

/// <summary>
/// Custom authorization metadata attribute for opaque token validation.
/// Indicates that an endpoint requires a valid opaque token via the custom middleware.
///
/// Usage:
/// Apply this attribute via the fluent API to require token authentication.
/// Typically applied automatically by .RequireRole() or .RequireOpaqueTokenAuthorization().
///
/// Example:
/// app.MapGet("/protected", handler).RequireOpaqueTokenAuthorization();
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireOpaqueTokenAuthorizationAttribute : Attribute
{
}
