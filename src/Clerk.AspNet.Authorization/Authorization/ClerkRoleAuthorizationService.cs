using Clerk.BackendAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Clerk.AspNet.Authorization.Authorization;

/// <summary>
/// Service for handling Clerk role authorization logic.
/// Separated from middleware to improve testability and maintainability.
/// Implements role validation using the Clerk Backend API to fetch user organization memberships.
/// </summary>
public interface IClerkRoleAuthorizationService
{
    /// <summary>
    /// Validates a user has a specific role.
    /// </summary>
    /// <param name="userId">The Clerk user ID to validate.</param>
    /// <param name="requiredRole">The specific role required (e.g., "org:admin").</param>
    /// <returns>Authorization result indicating whether the user has the required role.</returns>
    Task<AuthorizationResult> ValidateSingleRoleAsync(string userId, string requiredRole);

    /// <summary>
    /// Validates a user has at least one of the specified roles.
    /// </summary>
    /// <param name="userId">The Clerk user ID to validate.</param>
    /// <param name="requiredRoles">Array of roles where user must have at least one.</param>
    /// <returns>Authorization result indicating whether the user has any of the required roles.</returns>
    Task<AuthorizationResult> ValidateAnyRoleAsync(string userId, string[] requiredRoles);

    /// <summary>
    /// Validates a user has all the specified roles.
    /// </summary>
    /// <param name="userId">The Clerk user ID to validate.</param>
    /// <param name="requiredRoles">Array of roles that the user must ALL possess.</param>
    /// <returns>Authorization result indicating whether the user has all required roles.</returns>
    Task<AuthorizationResult> ValidateAllRolesAsync(string userId, string[] requiredRoles);

    /// <summary>
    /// Fetches user roles from Clerk organization memberships.
    /// </summary>
    /// <param name="userId">The Clerk user ID to fetch roles for.</param>
    /// <returns>List of role strings the user has across all organization memberships.</returns>
    Task<List<string>> GetUserRolesAsync(string userId);
}

/// <summary>
/// Result of role authorization validation.
/// Contains authorization status, user roles, error messages, and the matched role(s).
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// Whether authorization was successful.
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// User's roles fetched from Clerk organization memberships.
    /// </summary>
    public List<string> UserRoles { get; set; } = new();

    /// <summary>
    /// Error message if authorization failed.
    /// Provides details about why authorization was denied (e.g., missing roles).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The role(s) that satisfied the authorization requirement.
    /// For Single/OneOf: contains the matched role.
    /// For AllOf: contains comma-separated matched roles.
    /// </summary>
    public string? AuthorizedRole { get; set; }
}

/// <summary>
/// Implementation of Clerk role authorization service.
/// Integrates with Clerk Backend API to fetch user organization memberships and validate roles.
/// Requires Clerk:SecretKey configuration to function.
/// </summary>
public class ClerkRoleAuthorizationService : IClerkRoleAuthorizationService
{
    private readonly string? _clerkSecretKey;
    private readonly string? _clerkApiUrl;
    private readonly ILogger<ClerkRoleAuthorizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the ClerkRoleAuthorizationService.
    /// </summary>
    /// <param name="configuration">Configuration object containing Clerk:SecretKey and optional Clerk:ApiUrl settings.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public ClerkRoleAuthorizationService(IConfiguration configuration, ILogger<ClerkRoleAuthorizationService> logger)
    {
        _clerkSecretKey = configuration["Clerk:SecretKey"];
        _clerkApiUrl = configuration["Clerk:ApiUrl"];
        _logger = logger;
    }

    /// <summary>
    /// Validates user has a specific role using fluent .Single()
    /// User must have exactly the specified role.
    /// </summary>
    public async Task<AuthorizationResult> ValidateSingleRoleAsync(string userId, string requiredRole)
    {
        _logger.LogInformation($"[Role] Single() - Validating user {userId} has role '{requiredRole}'");

        var userRoles = await GetUserRolesAsync(userId);
        var result = new AuthorizationResult { UserRoles = userRoles };

        _logger.LogInformation($"[Role] User roles: {string.Join(", ", userRoles)}");

        if (!userRoles.Contains(requiredRole))
        {
            result.IsAuthorized = false;
            result.ErrorMessage = $"User does not have required role '{requiredRole}'";
            _logger.LogWarning($"[Role] ✗ {result.ErrorMessage}");
            return result;
        }

        result.IsAuthorized = true;
        result.AuthorizedRole = requiredRole;
        _logger.LogInformation($"[Role] ✓ User has required role: {requiredRole}");
        return result;
    }

    /// <summary>
    /// Validates user has at least one of the specified roles using fluent .OneOf()
    /// User must have at least one of the provided roles.
    /// </summary>
    public async Task<AuthorizationResult> ValidateAnyRoleAsync(string userId, string[] requiredRoles)
    {
        _logger.LogInformation($"[Role] OneOf() - Validating user {userId} has any of: {string.Join(", ", requiredRoles)}");

        var userRoles = await GetUserRolesAsync(userId);
        var result = new AuthorizationResult { UserRoles = userRoles };

        _logger.LogInformation($"[Role] User roles: {string.Join(", ", userRoles)}");

        var matchedRoles = userRoles.Intersect(requiredRoles).ToList();

        if (!matchedRoles.Any())
        {
            result.IsAuthorized = false;
            result.ErrorMessage = "User does not have any of the required roles";
            _logger.LogWarning($"[Role] ✗ {result.ErrorMessage}");
            return result;
        }

        result.IsAuthorized = true;
        result.AuthorizedRole = matchedRoles.First();
        _logger.LogInformation($"[Role] ✓ User has required role: {result.AuthorizedRole}");
        return result;
    }

    /// <summary>
    /// Validates user has all the specified roles using fluent .AllOf()
    /// User must have ALL the provided roles.
    /// </summary>
    public async Task<AuthorizationResult> ValidateAllRolesAsync(string userId, string[] requiredRoles)
    {
        _logger.LogInformation($"[Role] AllOf() - Validating user {userId} has all of: {string.Join(", ", requiredRoles)}");

        var userRoles = await GetUserRolesAsync(userId);
        var result = new AuthorizationResult { UserRoles = userRoles };

        _logger.LogInformation($"[Role] User roles: {string.Join(", ", userRoles)}");

        var missingRoles = requiredRoles.Where(r => !userRoles.Contains(r)).ToList();

        if (missingRoles.Any())
        {
            result.IsAuthorized = false;
            result.ErrorMessage = $"User missing required roles: {string.Join(", ", missingRoles)}";
            _logger.LogWarning($"[Role] ✗ {result.ErrorMessage}");
            return result;
        }

        result.IsAuthorized = true;
        result.AuthorizedRole = string.Join(",", requiredRoles);
        _logger.LogInformation($"[Role] ✓ User has all required roles");
        return result;
    }

    /// <summary>
    /// Fetches user roles from Clerk organization memberships.
    /// Calls the Clerk Backend API to retrieve all organization memberships for a user.
    /// </summary>
    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var roles = new List<string>();

        if (string.IsNullOrEmpty(userId) || userId == "unknown")
        {
            _logger.LogWarning($"[Role] Cannot fetch roles: User ID is empty or unknown");
            return roles;
        }

        try
        {
            _logger.LogInformation($"[Role] Fetching user roles from Clerk API for user: {userId}");

            if (string.IsNullOrEmpty(_clerkSecretKey))
            {
                _logger.LogError($"[Role] Clerk:SecretKey not configured");
                return roles;
            }

            // Create Clerk API client with secret key and optional custom server URL
            var client = string.IsNullOrEmpty(_clerkApiUrl)
                ? new ClerkBackendApi(bearerAuth: _clerkSecretKey)
                : new ClerkBackendApi(serverUrl: _clerkApiUrl, bearerAuth: _clerkSecretKey);

            var response = await client.Users.GetOrganizationMembershipsAsync(userId);
            var memberships = response?.OrganizationMemberships?.Data;

            if (memberships != null && memberships.Count > 0)
            {
                foreach (var membership in memberships)
                {
                    if (!string.IsNullOrEmpty(membership.Role))
                    {
                        roles.Add(membership.Role);
                        _logger.LogInformation($"[Role] Found role: {membership.Role}");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"[Role] No organization memberships found for user: {userId}");
            }

            _logger.LogInformation($"[Role] Retrieved {roles.Count} roles for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Role] Error fetching user roles: {ex.Message}");
        }

        return roles;
    }
}
