using System.Security.Claims;
using Clerk.AspNet.Authorization.Attributes;
using Clerk.AspNet.Authorization.Authorization;
using Clerk.BackendAPI;
using Clerk.BackendAPI.Models.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Clerk.AspNet.Authorization.Middleware;

/// <summary>
/// Middleware to validate Clerk opaque tokens and establish user context.
/// Replaces JWT Bearer authentication for opaque token validation.
/// Supports custom role-based authorization via metadata attributes.
///
/// Features:
/// - Validates opaque tokens (oat_* prefix) via Clerk Backend API
/// - Extracts user ID from token and sets up claims principal
/// - Supports role-based authorization with fluent API (.Single, .OneOf, .AllOf)
/// - Handles both authenticated and public endpoints
/// - Provides detailed logging for debugging token validation
/// </summary>
public class OpaqueTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _clerkSecretKey;
    private readonly string? _clerkAuthority;
    private readonly string? _clerkApiUrl;
    private readonly ILogger<OpaqueTokenValidationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the OpaqueTokenValidationMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="configuration">Configuration containing Clerk:SecretKey, optional Clerk:ApiUrl, and optional Authentication:Jwt:Authority.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public OpaqueTokenValidationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<OpaqueTokenValidationMiddleware> logger)
    {
        _next = next;
        _clerkSecretKey = configuration["Clerk:SecretKey"];
        _clerkAuthority = configuration["Authentication:Jwt:Authority"];
        _clerkApiUrl = configuration["Clerk:ApiUrl"];
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to validate opaque tokens and handle authorization.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="authorizationService">Scoped service for role-based authorization checks (injected per request).</param>
    public async Task InvokeAsync(HttpContext context, IClerkRoleAuthorizationService authorizationService)
    {
        // Check if the current endpoint requires opaque token authorization
        var endpoint = context.GetEndpoint();
        var requiresAuth = endpoint?.Metadata.GetMetadata<RequireOpaqueTokenAuthorizationAttribute>() != null;
        var requiredRole = endpoint?.Metadata.GetMetadata<RequireRoleAttribute>();
        var requiredAnyRole = endpoint?.Metadata.GetMetadata<RequireAnyRoleAttribute>();
        var requiredAllRoles = endpoint?.Metadata.GetMetadata<RequireAllRolesAttribute>();

        // Extract Bearer token from Authorization header
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..];

            try
            {
                // Check if this is an opaque token (starts with oat_)
                if (token.StartsWith("oat_"))
                {
                    _logger.LogInformation($"[Token] Opaque token detected: {token[..Math.Min(50, token.Length)]}...");

                    // Validate opaque token with Clerk API
                    if (!string.IsNullOrEmpty(_clerkSecretKey))
                    {
                        var validationResult = await ValidateOpaqueTokenWithClerkAsync(token, context.RequestAborted).ConfigureAwait(false);

                        if (validationResult.IsValid)
                        {
                            // Set up user context for opaque token with user ID claim
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, validationResult.UserId ?? token)
                            };

                            // Check if role-based authorization is required
                            if (requiredRole != null || requiredAnyRole != null || requiredAllRoles != null)
                            {
                                AuthorizationResult authResult = null!;

                                // Delegate role authorization to service
                                if (requiredRole != null)
                                {
                                    authResult = await authorizationService.ValidateSingleRoleAsync(
                                        validationResult.UserId!, requiredRole.Role, context.RequestAborted).ConfigureAwait(false);
                                }
                                else if (requiredAnyRole != null)
                                {
                                    authResult = await authorizationService.ValidateAnyRoleAsync(
                                        validationResult.UserId!, requiredAnyRole.Roles, context.RequestAborted).ConfigureAwait(false);
                                }
                                else if (requiredAllRoles != null)
                                {
                                    authResult = await authorizationService.ValidateAllRolesAsync(
                                        validationResult.UserId!, requiredAllRoles.Roles, context.RequestAborted).ConfigureAwait(false);
                                }

                                if (!authResult.IsAuthorized)
                                {
                                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                    await context.Response.WriteAsync($"Forbidden: {authResult.ErrorMessage}", context.RequestAborted).ConfigureAwait(false);
                                    return;
                                }

                                if (!string.IsNullOrEmpty(authResult.AuthorizedRole))
                                {
                                    claims.Add(new Claim("role", authResult.AuthorizedRole));
                                }
                            }

                            var identity = new ClaimsIdentity(claims, "Bearer");
                            var principal = new ClaimsPrincipal(identity);
                            context.User = principal;

                            _logger.LogInformation($"[Token] ✓ Opaque token validated successfully");
                        }
                        else
                        {
                            _logger.LogWarning($"[Token] ✗ Opaque token validation failed at Clerk API");
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Unauthorized: Invalid token", context.RequestAborted).ConfigureAwait(false);
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[Token] ⚠️ Opaque token detected but Clerk:SecretKey not configured");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized: Token validation not configured", context.RequestAborted).ConfigureAwait(false);
                        return;
                    }
                }
                else
                {
                    // For non-opaque tokens (JWT), set minimal context
                    _logger.LogInformation($"[Token] Non-opaque token detected: {token[..Math.Min(50, token.Length)]}...");

                    // Create a basic identity for JWT tokens
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, "jwt-user")
                    };

                    var identity = new ClaimsIdentity(claims, "Bearer");
                    var principal = new ClaimsPrincipal(identity);
                    context.User = principal;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Token] ✗ Error validating token: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Token validation error", context.RequestAborted).ConfigureAwait(false);
                return;
            }
        }
        else if (requiresAuth)
        {
            // No token provided but endpoint requires opaque token authorization
            _logger.LogWarning($"[Token] ✗ No token provided for protected endpoint: {context.Request.Path}");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: No token provided", context.RequestAborted).ConfigureAwait(false);
            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates an opaque token using Clerk Backend API SDK.
    /// Calls POST /v1/oauth_applications/access_tokens/verify endpoint.
    /// </summary>
    /// <param name="token">The opaque token to validate.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Validation result containing token status and user ID if valid.</returns>
    private async Task<TokenValidationResult> ValidateOpaqueTokenWithClerkAsync(string token, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"[Clerk API] Validating opaque token with Clerk SDK");

            // Create Clerk API client with secret key and optional custom server URL
            var client = string.IsNullOrEmpty(_clerkApiUrl)
                ? new ClerkBackendApi(bearerAuth: _clerkSecretKey)
                : new ClerkBackendApi(serverUrl: _clerkApiUrl, bearerAuth: _clerkSecretKey);

            _logger.LogInformation($"[Clerk API] Using Clerk Backend API{(string.IsNullOrEmpty(_clerkApiUrl) ? "" : $" at {_clerkApiUrl}")}");

            // Check for cancellation before making expensive API call
            cancellationToken.ThrowIfCancellationRequested();

            // Create request body - use AccessToken property name
            var requestBody = new VerifyOAuthAccessTokenRequestBody
            {
                AccessToken = token
            };

            // Verify access token using SDK
            var response = await client.OauthAccessTokens.VerifyAsync(requestBody).ConfigureAwait(false);

            if (response?.Object != null)
            {
                _logger.LogInformation($"[Clerk API] ✓ Token validation succeeded");

                // Extract token metadata from response.Object
                try
                {
                    var tokenObj = response.Object;
                    var tokenId = tokenObj.Id ?? "unknown";
                    var subject = tokenObj.Subject ?? "unknown";
                    var revoked = tokenObj.Revoked is true;
                    var expired = tokenObj.Expired is true;

                    _logger.LogInformation($"  - Token ID: {tokenId}");
                    _logger.LogInformation($"  - Subject (User ID): {subject}");
                    _logger.LogInformation($"  - Revoked: {revoked}");
                    _logger.LogInformation($"  - Expired: {expired}");

                    // Token is valid if not revoked and not expired
                    var isValid = !revoked && !expired;
                    return new TokenValidationResult { IsValid = isValid, UserId = subject };
                }
                catch (Exception extractEx)
                {
                    _logger.LogWarning($"[Clerk API] ⚠️ Error extracting token metadata: {extractEx.Message}");
                    // If we got a response, token exists and is valid
                    return new TokenValidationResult { IsValid = true, UserId = "unknown" };
                }
            }
            else
            {
                _logger.LogWarning($"[Clerk API] ✗ Token validation failed: No token in response");
                return new TokenValidationResult { IsValid = false };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Clerk API] ✗ Error calling Clerk API: {ex.Message}");
            _logger.LogError($"[Clerk API] Stack trace: {ex.StackTrace}");
            return new TokenValidationResult { IsValid = false };
        }
    }
}

/// <summary>
/// Result of token validation containing validation status and user ID.
/// </summary>
internal class TokenValidationResult
{
    /// <summary>
    /// Indicates whether the token is valid (not revoked and not expired).
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The user ID (subject) extracted from the token.
    /// May be null or "unknown" if not available.
    /// </summary>
    public string? UserId { get; set; }
}
