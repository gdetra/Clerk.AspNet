namespace Clerk.AspNet.Authorization.Attributes;

/// <summary>
/// Custom authorization metadata attribute for role-based access control.
/// Indicates that an endpoint requires the user to have ALL of the specified roles.
///
/// Used in conjunction with .RequireRole().AllOf() fluent API.
/// The user must have all of the provided roles for authorization to succeed.
///
/// Example role formats:
/// - "org:admin" - Organization admin role
/// - "org:billing" - Organization billing role
/// - "org:manager" - Organization manager role
///
/// Usage:
/// app.MapGet("/billing", handler).RequireRole().AllOf("org:admin", "org:billing");
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireAllRolesAttribute : Attribute
{
    /// <summary>
    /// The array of roles that the user must ALL possess.
    /// Example: ["org:admin", "org:billing"]
    /// </summary>
    public string[] Roles { get; }

    /// <summary>
    /// Initializes a new instance of the RequireAllRolesAttribute.
    /// </summary>
    /// <param name="roles">Array of roles that user must ALL have.</param>
    public RequireAllRolesAttribute(params string[] roles)
    {
        Roles = roles;
    }
}
