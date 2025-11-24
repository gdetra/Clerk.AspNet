namespace Clerk.AspNet.Authorization.Attributes;

/// <summary>
/// Custom authorization metadata attribute for role-based access control.
/// Indicates that an endpoint requires the user to have a specific role.
///
/// Used in conjunction with .RequireRole().Single() fluent API.
/// The user must have exactly the specified role for authorization to succeed.
///
/// Example role formats:
/// - "org:admin" - Organization admin role
/// - "org:manager" - Organization manager role
/// - "org:member" - Organization member role
///
/// Usage:
/// app.MapGet("/admin", handler).RequireRole().Single("org:admin");
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute
{
    /// <summary>
    /// The specific role required to access the endpoint.
    /// Example: "org:admin"
    /// </summary>
    public string Role { get; }

    /// <summary>
    /// Initializes a new instance of the RequireRoleAttribute.
    /// </summary>
    /// <param name="role">The role that is required (e.g., "org:admin").</param>
    public RequireRoleAttribute(string role)
    {
        Role = role;
    }
}
