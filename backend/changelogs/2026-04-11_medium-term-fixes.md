# 2026-04-11_medium-term-fixes

**Completed:** Medium-Term Fixes from Security Audit (1–3 months)

## Changes

### Fix #13 — Audit Trail (`AuditLog`, `AuditInterceptor`, `AppDbContext`)

- New entity `Infrastructure/Persistence/AuditLog.cs`: tracks `EntityName`, `EntityId`, `Action` (Created/Modified/Deleted), `UserId`, `UserName`, `Timestamp`
- New `Infrastructure/Persistence/AuditInterceptor.cs`: extends `SaveChangesInterceptor`; fires on every `SaveChangesAsync`, captures changes on 4 audited entity types: **Transaction, Student, Ledger, Payer**
- `IHttpContextAccessor` used to read the current user's claims — no manual userId passing needed
- For newly created records (EntityState.Added), `EntityId` is `null` because auto-increment IDs are assigned by the DB after the save; for Modified/Deleted the actual ID is captured
- Added `DbSet<AuditLog> AuditLogs` + table config with two indexes (`{EntityName, EntityId}` and `Timestamp`) to `AppDbContext`
- Migration: `AddAuditLog`

### Fix #14 — Environment-Specific Config

- **No code changes required.** ASP.NET Core's configuration system already supports environment variable overrides using double-underscore (`__`) as a path separator:
  - To override the connection string in production: set env var `ConnectionStrings__DefaultConnection=<your-connection-string>`
  - To override the admin seed password: set env var `AdminSeedPassword=<your-password>`
- For local development, use `dotnet user-secrets`:
  ```bash
  dotnet user-secrets init   # once per project
  dotnet user-secrets set "AdminSeedPassword" "YourLocalPassword"
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;..."
  ```
- The `appsettings.json` connection string (`localhost/SchoolAccounting`) remains as a convenient local default
- For production/staging: inject env vars via the deployment environment (Docker Compose, systemd, Azure App Service, etc.)

### Fix #15 — Reference Data Caching (`CategoryService.cs`, `Program.cs`)

- Registered `builder.Services.AddMemoryCache()` in `Program.cs`
- `CategoryService` now accepts `IMemoryCache` via constructor injection
- `GetCategoriesAsync` caches the full mapped `List<CategoryResponse>` under key `"categories"` with a 5-minute TTL; filtering by type/isActive is applied in-memory after the cache hit
- `AsNoTracking()` used on the DB query to avoid caching EF-tracked entities
- Cache is invalidated (`.Remove(CacheKey)`) in `CreateCategoryAsync`, `UpdateCategoryAsync`, and `DeleteCategoryAsync`
- GradeService and ClassService were evaluated but not cached — their paginated/sorted queries are less suitable for a single-key cache, and the data size is small enough that direct DB queries are fast

### Fix #16 — API Versioning Strategy (Documentation)

- **Current state:** No API versioning. All routes follow the pattern `/api/{resource}`.
- **Decision:** No versioning needed at current scale. This is a single-tenant school system with a tightly coupled frontend; a breaking API change can be coordinated with a frontend deploy.
- **If versioning becomes necessary:** Use URL path prefix versioning (`/api/v1/...`). This is the most transparent and cache-friendly approach, supported natively by ASP.NET Core's routing. Avoid header-based versioning (harder to test and debug).
- **How to add it when needed:** Add `[Route("api/v1/[controller]")]` to controller base routes, or use the `Asp.Versioning` NuGet package for automated version negotiation.

## Files Changed

| File | Change |
|---|---|
| `Infrastructure/Persistence/AuditLog.cs` | New entity |
| `Infrastructure/Persistence/AuditInterceptor.cs` | New interceptor |
| `Infrastructure/Persistence/AppDbContext.cs` | Added `DbSet<AuditLog>` + table config |
| `Program.cs` | Added `AddHttpContextAccessor`, `AddSingleton<AuditInterceptor>`, `AddMemoryCache`, updated `AddDbContext` to inject interceptor |
| `Features/Categories/CategoryService.cs` | Added `IMemoryCache` caching for category list |
| Migration: `AddAuditLog` | New EF migration |

## Notes

- The audit interceptor is registered as a **Singleton** (correct — it holds no mutable state; `IHttpContextAccessor` is also Singleton)
- The EF advisory warnings about Ledger→Category, Ledger→JournalEntry, and Transaction→TransactionItem required-end relationships are unchanged and remain safe (existing business rules already prevent deletion when child records exist)
- `IMemoryCache` is process-local. If the app is ever scaled to multiple instances, the category cache will be per-instance. For a single-server school system this is fine; if multi-instance is needed, replace with `IDistributedCache` (Redis)
- Item #18 (Long Term: "Consider EF Core interceptor-based audit logging") is now superseded by #13 and marked done
