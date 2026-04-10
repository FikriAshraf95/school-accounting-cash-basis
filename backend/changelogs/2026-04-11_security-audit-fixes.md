# 2026-04-11_security-audit-fixes

**Completed:** Security Audit — Critical Issue Remediation

## Changes

### Security Fixes (Critical)

- **Removed hardcoded admin credentials from source control** (`appsettings.json`, `appsettings.Development.json`, `DbInitializer.cs`)
  - Removed `AdminSeedPassword` key from `appsettings.json` — password is no longer committed to git
  - Added `AdminSeedPassword: "DevAdmin@123!"` to `appsettings.Development.json` (development only)
  - `DbInitializer.SeedAsync`: removed `?? "ChangeMe123!"` fallback; now throws `InvalidOperationException` at startup if key is not configured
  - **Production deployment**: set `ADMINSEEDPASSWORD` environment variable before first run

- **Fixed token expiration bypass** (`Infrastructure/Auth/BearerTokenAuthHandler.cs`)
  - Tokens with `ExpiresAt = null` were previously allowed indefinitely
  - Changed condition from `HasValue && expired` to `!HasValue || expired`
  - Tokens without an expiry date are now rejected outright

- **Added rate limiting to authentication endpoints** (`Program.cs`, `Features/Auth/AuthController.cs`)
  - Registered built-in .NET `RateLimiter` with a named `"auth"` fixed-window policy
  - Policy: 10 requests per minute per IP, queue limit 0, returns HTTP 429
  - Applied `[EnableRateLimiting("auth")]` to `POST /api/v1/login` and `POST /api/v1/register`

- **Tightened CORS policy** (`Program.cs`, `appsettings.json`)
  - Replaced `AllowAnyMethod()` + `AllowAnyHeader()` with explicit lists:
    - Methods: `GET, POST, PUT, PATCH, DELETE`
    - Headers: `Authorization, Content-Type, Accept`
  - Allowed origins now read from `Cors:AllowedOrigins` config key (falls back to `http://localhost:3000`)
  - Added `"Cors": { "AllowedOrigins": [...] }` section to `appsettings.json` for easy per-environment override

- **Enabled HTTPS redirection** (`Program.cs`)
  - Added `app.UseHttpsRedirection()` to the middleware pipeline

- **Upgraded preview NuGet packages to stable releases** (`SchoolAccounting.Api.csproj`)
  - `FluentValidation` + `FluentValidation.DependencyInjectionExtensions`: `12.0.0-preview1` → `12.1.1`
  - `Microsoft.EntityFrameworkCore.SqlServer` + `.Design`: `10.0.0-preview.2.25163.2` → `10.0.0`

## Files Changed

| File | Change |
|---|---|
| `SchoolAccounting.Api/appsettings.json` | Removed `AdminSeedPassword`; added `Cors.AllowedOrigins` |
| `SchoolAccounting.Api/appsettings.Development.json` | Added `AdminSeedPassword` (dev value) |
| `Infrastructure/Persistence/DbInitializer.cs` | Removed hardcoded password fallback; throws if unconfigured |
| `Infrastructure/Auth/BearerTokenAuthHandler.cs` | Fixed null `ExpiresAt` bypass |
| `Program.cs` | Rate limiter, HTTPS redirect, restricted CORS |
| `Features/Auth/AuthController.cs` | `[EnableRateLimiting("auth")]` on Login + Register |
| `SchoolAccounting.Api.csproj` | Pinned FluentValidation and EF Core to stable versions |

## Notes

- Existing tokens in the database that have `ExpiresAt = null` will now be rejected on next use.
  Users will need to log in again to obtain a new token (all new tokens have a 30-day expiry via `AuthService`).
- The CORS change removes the wildcard header/method allowance; if the frontend sends non-standard
  headers in the future, `WithHeaders(...)` in `Program.cs` must be updated.
- No database migration required for any of these changes.
