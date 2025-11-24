namespace Clerk.AspNet.Authorization.Attributes;

/// <summary>
/// Custom authorization metadata attribute for role-based access control.
/// Indicates that an endpoint requires the user to have at least one of the specified roles.
///
/// Used in conjunction with .RequireRole().OneOf() fluent API.
/// The user must have at least one of the provided roles for authorization to succeed.
///
/// Example role formats:
/// - "org:admin" - Organization admin role
/// - "org:manager" - Organization manager role
/// - "org:member" - Organization member role
///
/// Usage:
/// app.MapGet("/moderator", handler).RequireRole().OneOf("org:admin", "org:manager");
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireAnyRoleAttribute : Attribute
{
    /// <summary>
    /// The array of roles where the user must have at least one.
    /// Example: ["org:admin", "org:manager"]
    /// </summary>
    public string[] Roles { get; }

    /// <summary>
    /// Initializes a new instance of the RequireAnyRoleAttribute.
    /// </summary>
    /// <param name="roles">Array of roles where user must have at least one.</param>
    public RequireAnyRoleAttribute(params string[] roles)
    {
        Roles = roles;
    }
}
