# Clerk.AspNet.Authorization

A comprehensive ASP.NET Core library for Clerk authentication and authorization integration. This library provides opaque token validation, role-based access control, and middleware integration for seamless Clerk integration.

## Features

- **Opaque Token Validation**: Validate Clerk session tokens and OAuth access tokens
- **Role-Based Authorization**: Simple, fluent API for role-based access control
- **Middleware Integration**: ASP.NET Core middleware for automatic token validation
- **Multiple Role Patterns**: Support for single role, any role, and all roles validation
- **Comprehensive Logging**: Detailed logging for debugging and monitoring
- **Async/Await Support**: Fully async implementation for modern ASP.NET Core

## Installation

```bash
dotnet add package Clerk.AspNet.Authorization
```

## Quick Start

### 1. Configure Services

In your `Program.cs`:

```csharp
using Clerk.AspNet.Authorization.Extensions;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add Clerk authentication services
builder.Services.AddClerkRoleAuthorization();

var app = builder.Build();

// Add Clerk token validation middleware
app.UseOpaqueTokenValidation();

app.MapGet("/protected", () => "This endpoint requires a valid token")
    .RequireOpaqueTokenAuthorization();

app.MapGet("/admin", () => "This endpoint requires admin role")
    .RequireRole()
    .Single("org:admin");

app.Run();
```

### 2. Configure Clerk Credentials

Add your Clerk secret key to `appsettings.json`:

```json
{
  "Clerk": {
    "SecretKey": "your_clerk_secret_key"
  }
}
```

## Usage Examples

### Token Validation Only

```csharp
app.MapGet("/api/profile", () => "Get user profile")
    .RequireOpaqueTokenAuthorization();
```

### Single Role Required

```csharp
app.MapGet("/admin/users", () => "Admin users list")
    .RequireRole()
    .Single("org:admin");
```

### Any Role Required (OR logic)

```csharp
app.MapGet("/manage", () => "Management dashboard")
    .RequireRole()
    .OneOf("org:admin", "org:manager", "org:supervisor");
```

### All Roles Required (AND logic)

```csharp
app.MapGet("/billing", () => "Billing information")
    .RequireRole()
    .AllOf("org:admin", "org:billing");
```

## API Reference

### ServiceCollectionExtensions

#### AddClerkRoleAuthorization()
Registers the Clerk role authorization service in the DI container.

```csharp
services.AddClerkRoleAuthorization();
```

### OpaqueTokenValidationExtensions

#### UseOpaqueTokenValidation()
Adds the token validation middleware to the application pipeline.

```csharp
app.UseOpaqueTokenValidation();
```

#### RequireOpaqueTokenAuthorization()
Marks an endpoint as requiring opaque token validation.

```csharp
app.MapGet("/endpoint", handler)
    .RequireOpaqueTokenAuthorization();
```

#### RequireRole()
Returns a fluent builder for role-based authorization.

```csharp
app.MapGet("/endpoint", handler)
    .RequireRole()
    .Single("role-name");
```

### ClerkRoleAuthorizationService

#### ValidateSingleRoleAsync(userId, role)
Validates that a user has a specific role.

#### ValidateAnyRoleAsync(userId, roles[])
Validates that a user has at least one of the specified roles.

#### ValidateAllRolesAsync(userId, roles[])
Validates that a user has all of the specified roles.

#### GetUserRolesAsync(userId)
Retrieves all roles for a user from Clerk.

## Configuration

### appsettings.json

```json
{
  "Clerk": {
    "SecretKey": "your_clerk_secret_key_here"
  }
}
```

**Note**: The `SecretKey` is required for the library to function. Get it from your [Clerk Dashboard](https://dashboard.clerk.com).

## HTTP Status Codes

- **200 OK**: Token valid and authorization passed
- **401 Unauthorized**: No token provided, invalid token, or token failed verification
- **403 Forbidden**: Token valid but user doesn't have required role(s)

## Error Handling

The middleware automatically returns appropriate HTTP status codes. For custom error handling, catch and log exceptions:

```csharp
try
{
    // Use the IClerkRoleAuthorizationService directly
    var result = await clerkService.ValidateSingleRoleAsync(userId, requiredRole);
    if (!result.IsAuthorized)
    {
        // Handle authorization failure
        return Results.Forbid();
    }
}
catch (Exception ex)
{
    // Log the exception
    logger.LogError(ex, "Authorization check failed");
    return Results.StatusCode(500);
}
```

## Testing

The library includes comprehensive unit tests using xUnit and Moq. Examples include:

- Service registration tests
- Role validation tests
- Token validation tests
- Attribute tests

Run tests with:

```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/yourusername/Clerk.AspNet).

## Requirements

- .NET 10.0 or higher
- ASP.NET Core 10.0 or higher
- Valid Clerk account and API credentials

## Related Resources

- [Clerk Documentation](https://clerk.com/docs)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
