# Clerk.AspNet.Authorization

[![NuGet](https://img.shields.io/nuget/v/Clerk.AspNet.Authorization.svg)](https://www.nuget.org/packages/Clerk.AspNet.Authorization)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Clerk.AspNet.Authorization.svg)](https://www.nuget.org/packages/Clerk.AspNet.Authorization)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A fluent, modern authorization library for ASP.NET Core Minimal APIs using [Clerk](https://clerk.com) as the identity provider.

## Installation

```bash
dotnet add package Clerk.AspNet.Authorization
```

Or via Package Manager Console:

```powershell
Install-Package Clerk.AspNet.Authorization
```

## Quick Start

### 1. Add Clerk Configuration

In your `appsettings.json`:

```json
{
  "Clerk": {
    "SecretKey": "your_clerk_secret_key"
  }
}
```

### 2. Register Services

In `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Clerk authorization services
builder.Services.AddClerkRoleAuthorization();

var app = builder.Build();

// Add opaque token validation middleware
app.UseRouting();
app.UseOpaqueTokenValidation();
```

### 3. Protect Your Endpoints

#### Simple Token Validation

```csharp
app.MapGet("/protected", () => "This requires authentication")
    .RequireOpaqueTokenAuthorization();
```

#### Single Role Required

```csharp
app.MapGet("/admin", () => "Admin only")
    .RequireRole()
    .Single("org:admin");
```

#### Any Role (OR logic)

```csharp
app.MapGet("/dashboard", () => "Managers and admins")
    .RequireRole()
    .OneOf("org:admin", "org:manager");
```

#### All Roles (AND logic)

```csharp
app.MapGet("/billing", () => "Requires multiple roles")
    .RequireRole()
    .AllOf("org:admin", "org:billing");
```

## Usage Examples

### Complete Minimal API Example

```csharp
using Clerk.AspNet.Authorization.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Clerk
builder.Services.AddClerkRoleAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseOpaqueTokenValidation();

// Public endpoint - no auth required
app.MapGet("/", () => "Hello World");

// Protected endpoint - any valid token
app.MapGet("/profile", (HttpContext context) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return new { UserId = userId, Message = "Your profile" };
})
.RequireOpaqueTokenAuthorization();

// Admin only
app.MapGet("/admin/dashboard", () => new { Message = "Admin Dashboard" })
    .RequireRole()
    .Single("org:admin");

// Manager or Admin
app.MapGet("/reports", () => new { Message = "Reports" })
    .RequireRole()
    .OneOf("org:admin", "org:manager");

// Requires both roles
app.MapGet("/sensitive", () => new { Message = "Sensitive Data" })
    .RequireRole()
    .AllOf("org:admin", "org:security");

app.Run();
```

### Custom API URL (for testing or self-hosted Clerk)

```json
{
  "Clerk": {
    "SecretKey": "your_secret_key",
    "ApiUrl": "https://your-clerk-instance.com"
  }
}
```

### Working with Claims

After authentication, user claims are available:

```csharp
app.MapGet("/me", (HttpContext context) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var role = context.User.FindFirst("role")?.Value;

    return new
    {
        UserId = userId,
        Role = role,
        IsAuthenticated = context.User.Identity?.IsAuthenticated ?? false
    };
})
.RequireOpaqueTokenAuthorization();
```

## Token Types Supported

### Opaque Tokens

Tokens starting with `oat_` are validated with Clerk's API:

```http
GET /protected HTTP/1.1
Authorization: Bearer oat_2a3b4c5d...
```

Features:
- ✅ Automatic validation with Clerk API
- ✅ Checks for revocation
- ✅ Checks for expiration
- ✅ Extracts user ID and roles

> **Note:** JWT token support is planned for a future release.

## Architecture

The library consists of two main components:

### 1. OpaqueTokenValidationMiddleware

- Intercepts requests and validates opaque tokens
- Sets up ClaimsPrincipal for authenticated users
- Integrated with ASP.NET Core's authorization system

### 2. ClerkRoleAuthorizationService

- Fetches user organization memberships from Clerk
- Validates role requirements
- Fully testable and mockable via `IClerkRoleAuthorizationService`

## Configuration Options

| Setting | Description | Required | Default |
|---------|-------------|----------|---------|
| `Clerk:SecretKey` | Your Clerk secret key | ✅ Yes | - |
| `Clerk:ApiUrl` | Custom Clerk API URL | ❌ No | Clerk production API |

## Performance

- ✅ **ConfigureAwait(false)** on all async operations (no context capture)
- ✅ **CancellationToken** support for all async methods
- ✅ **Efficient** - Minimal overhead on request pipeline

## Requirements

- .NET 10.0 or later
- ASP.NET Core Minimal APIs
- Clerk account and secret key

## Security Considerations

- Store your `Clerk:SecretKey` in secure configuration (Azure Key Vault, AWS Secrets Manager, etc.)
- Use HTTPS in production
- Enable CORS appropriately for your API
- The middleware checks for token revocation and expiration automatically

## Troubleshooting

### 401 Unauthorized with valid token

**Cause**: Clerk secret key not configured or incorrect

**Solution**: Verify `Clerk:SecretKey` in your configuration

### Role validation always fails

**Cause**: User doesn't have organization memberships in Clerk

**Solution**: Ensure users are added to organizations with appropriate roles in Clerk dashboard

### Custom API URL not working

**Cause**: Incorrect `Clerk:ApiUrl` format

**Solution**: Use full URL including protocol: `https://api.clerk.com`

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on:

- Conventional Commits format
- Development workflow
- Testing requirements
- Pull request process

This project uses [semantic-release](https://semantic-release.gitbook.io/) for automated versioning and publishing.

## Roadmap

- [ ] JWT token support
- [ ] Support for custom claim extraction
- [ ] Caching for organization memberships
- [ ] Multi-tenancy support
- [ ] Policy-based authorization integration
- [ ] OpenTelemetry instrumentation

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built on top of [Clerk](https://clerk.com) for authentication
- Uses [Clerk.BackendAPI](https://www.nuget.org/packages/Clerk.BackendAPI) SDK
- Inspired by ASP.NET Core's authorization patterns

## Support

- 📖 [Documentation](https://github.com/gdetra/Clerk.AspNet)
- 🐛 [Issue Tracker](https://github.com/gdetra/Clerk.AspNet/issues)
- 💬 [Discussions](https://github.com/gdetra/Clerk.AspNet/discussions)

---
