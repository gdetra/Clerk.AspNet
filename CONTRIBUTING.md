# Contributing to Clerk.AspNet.Authorization

Thank you for your interest in contributing! This project uses automated versioning and releases.

## 🚀 Automated Versioning

This project uses **[semantic-release](https://semantic-release.gitbook.io/)** to automatically:
- Determine the next version number based on commit messages
- Generate release notes and CHANGELOG.md
- Create GitHub releases
- Publish to NuGet

## 📝 Commit Message Convention

We follow the **[Conventional Commits](https://www.conventionalcommits.org/)** specification. This is crucial for automatic versioning.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

| Type       | Description                                                    | Version Bump |
|------------|----------------------------------------------------------------|--------------|
| `feat`     | A new feature                                                  | **Minor**    |
| `fix`      | A bug fix                                                      | **Patch**    |
| `perf`     | Performance improvement                                        | **Patch**    |
| `refactor` | Code change that neither fixes a bug nor adds a feature        | **Patch**    |
| `docs`     | Documentation only changes                                     | **Patch***   |
| `style`    | Code style changes (formatting, missing semi-colons, etc.)     | None         |
| `test`     | Adding or correcting tests                                     | None         |
| `build`    | Changes to build system or dependencies                        | None         |
| `ci`       | Changes to CI configuration files and scripts                  | None         |
| `chore`    | Other changes that don't modify src or test files              | None         |
| `revert`   | Reverts a previous commit                                      | **Patch**    |

\* Only if scope is `README`

### Breaking Changes

To trigger a **Major** version bump, add `BREAKING CHANGE:` in the commit body or footer:

```
feat!: redesign authorization API

BREAKING CHANGE: The fluent API has been completely redesigned.
Old methods like `.RequireRole()` are no longer available.
```

Or use the `!` suffix:

```
feat!: remove legacy token validation
```

### Examples

#### Adding a feature (Minor version bump: 1.0.0 → 1.1.0)
```
feat(middleware): add support for custom claims extraction

This allows users to extract custom claims from tokens
and add them to the ClaimsPrincipal.
```

#### Fixing a bug (Patch version bump: 1.0.0 → 1.0.1)
```
fix(authorization): handle null user roles gracefully

Previously, if Clerk API returned null roles, the middleware
would throw NullReferenceException. Now returns empty list.

Fixes #123
```

#### Performance improvement (Patch version bump)
```
perf(service): cache organization memberships for 5 minutes

Reduces API calls to Clerk by caching user roles.
```

#### Breaking change (Major version bump: 1.0.0 → 2.0.0)
```
feat(api)!: replace .Single() with .RequireSingleRole()

BREAKING CHANGE: The fluent API method .Single() has been
renamed to .RequireSingleRole() for clarity. Update all
usages accordingly.
```

#### Documentation update (Patch version bump if scope is README)
```
docs(readme): add installation instructions

Added NuGet installation section and quick start guide.
```

#### Non-releasing commit
```
chore: update dependencies

Updated Clerk.BackendAPI to latest version.
```

## 🔄 Development Workflow

1. **Create a feature branch** from `main`:
   ```bash
   git checkout -b feat/my-new-feature
   ```

2. **Make your changes** and commit using conventional commits:
   ```bash
   git add .
   git commit -m "feat(middleware): add X feature"
   ```

3. **Push your branch** and create a Pull Request:
   ```bash
   git push origin feat/my-new-feature
   ```

4. **PR Review**: Your commits will be validated by commitlint in CI

5. **Merge to main**: Once approved, merge to main (squash or merge commits)

6. **Automatic Release**: On merge to main:
   - semantic-release analyzes all commits since last release
   - Determines next version number
   - Generates CHANGELOG.md
   - Creates GitHub release with release notes
   - Publishes to NuGet automatically

## 🛠️ Local Development

### Prerequisites
- .NET 10.0 SDK
- Node.js (for commitlint)

### Setup
```bash
# Install npm dependencies (for commitlint)
npm install

# Restore .NET dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

### Commit Validation (Optional)
You can validate your commits locally before pushing:

```bash
# Install commitlint CLI globally (optional)
npm install -g @commitlint/cli

# Validate last commit
npx commitlint --from HEAD~1 --to HEAD --verbose
```

## 📦 Release Process

Releases happen **automatically** when commits are merged to `main`:

1. Tests must pass
2. Commit messages are analyzed
3. Version is determined based on commit types
4. CHANGELOG.md is updated
5. Git tag is created (e.g., `v1.2.0`)
6. GitHub release is created with release notes
7. NuGet package is built and published

### Manual Release (Emergency Only)

If you need to trigger a release manually:

```bash
# Ensure you're on main with latest changes
git checkout main
git pull

# Run semantic-release locally (requires GITHUB_TOKEN and NUGET_API_KEY)
npm run release
```

## 🧪 Testing

All PRs must include tests for new features or bug fixes:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/Clerk.AspNet.Authorization.UnitTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📋 PR Checklist

Before submitting a PR, ensure:

- [ ] Code builds without errors (`dotnet build`)
- [ ] All tests pass (`dotnet test`)
- [ ] New features have tests
- [ ] Bug fixes have regression tests
- [ ] Commit messages follow Conventional Commits format
- [ ] Documentation is updated if needed
- [ ] No unnecessary dependencies added

## 🎯 Scope Guidelines

Use scopes to indicate what part of the codebase changed:

- `middleware` - Changes to OpaqueTokenValidationMiddleware
- `service` - Changes to ClerkRoleAuthorizationService
- `extensions` - Changes to extension methods
- `attributes` - Changes to authorization attributes
- `tests` - Changes to test code
- `docs` - Documentation changes
- `ci` - CI/CD configuration changes

## 💡 Tips

1. **Keep commits atomic**: One logical change per commit
2. **Write clear commit messages**: Others should understand what changed and why
3. **Use breaking changes sparingly**: They force major version bumps
4. **Reference issues**: Use `Fixes #123` or `Closes #456` in commit messages
5. **Test locally**: Run tests before pushing

## 🤝 Getting Help

- Open an issue for bugs or feature requests
- Check existing issues before creating new ones
- Ask questions in discussions

## 📜 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing! 🎉
