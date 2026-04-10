# School Accounting — .NET C# Rewrite (PHASE 1)

## Summary

A RESTful API for school financial management. Handles student payments, donor/vendor transactions, double-entry bookkeeping, and simple reporting. Rebuilt from Laravel (PHP) to .NET 10 minimal API with SQL Server (MSSQL).

**Core principles:**
- KISS — no unnecessary abstractions
- Vertical Slice Architecture — features, not layers
- No MediatR, no AutoMapper — direct DI and manual mapping
- Standard library first — minimal third-party packages

---

## Technology Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 |
| API style | Slim `[ApiController]` controllers — one per feature |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (MSSQL) |
| Auth | Bearer tokens — EF-backed `PersonalAccessTokens` table (no JWT library); RBAC via `ClaimTypes.Role` |
| Validation | FluentValidation or built-in DataAnnotations |
| Testing | xUnit + EF InMemory |

---

## Milestones

---

### Week 1 — Foundation & Auth

**Goal:** A running API that can authenticate users and reject unauthorised requests. Every subsequent milestone builds on this auth layer.

**Deliverables:**
- API starts, connects to SQL Server, and returns structured errors on failure.
- Any client can register, log in, receive a bearer token, and use it to call protected endpoints.
- Role stored on every user; new registrations default to `Viewer`; seed Admin account present on cold start.
- CORS configured for the front-end origin.

#### Milestone 1 — Project Bootstrap
- [x] Create solution + project (`dotnet new webapi`)
- [x] Configure EF Core with SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`)
- [x] Set up folder structure (Vertical Slice)
- [x] Add global error handling middleware
- [x] Add CORS policy matching original (`localhost:3000`)
- [x] Write `README.md` with setup instructions

#### Milestone 2 — Auth & RBAC
- [x] `POST /api/v1/register` — create user (role defaults to `Viewer`)
- [x] `POST /api/v1/login` — return bearer token
- [x] `POST /api/v1/logout` — revoke token
- [x] `GET  /api/v1/user` — current user
- [x] Token stored in `PersonalAccessTokens` table (simple, no JWT)
- [x] `Role` column on `Users`; `BearerTokenAuthHandler` issues `ClaimTypes.Role`
- [x] `[Authorize(Roles = ...)]` applied to all controllers per permission matrix
- [x] `Features/UserManagement/` — Admin-only CRUD + role assignment; last-Admin guard
- [x] `DbInitializer` seeds default `admin` account with `Role = Admin` on cold start

---

### Week 2 — Reference Data

**Goal:** All lookup / configuration data is manageable via the API. Transactions and students in later weeks depend on these records existing first.

**Deliverables:**
- Staff can create and maintain grades, classes, ledger accounts, categories, and business profile.
- Default chart of accounts (1000–3000 codes) seeded on first run so the system is usable immediately.
- Student-to-class assignment endpoint ready for Week 3.

#### Milestone 3 — Reference Data
- [x] **Grades** — CRUD (`/api/v1/grades`)
- [x] **Classes** — CRUD + assign/remove students (`/api/v1/classes`)
- [x] **Ledgers** (Chart of Accounts) — CRUD + seed default accounts (`/api/v1/ledgers`)
- [x] **Categories** — CRUD linked to ledger (`/api/v1/categories`)
- [x] **Business Info** — single-record GET/PUT (`/api/v1/business-info`)

---

### Week 3 — People & Transactions

**Goal:** The system can record financial activity. Income and expense transactions produce correct double-entry journal entries and update all balances atomically.

**Deliverables:**
- Students and payers are managed with soft delete and pagination.
- A transaction can be created with multiple line items; journal entries are auto-generated and validated (debit = credit) before saving.
- Ledger balances and student/payer balances are updated within the same DB transaction.
- Audit trail: every transaction records who created/updated it.

#### Milestone 4 — Students & Payers
- [ ] **Students** — CRUD, soft delete, pagination + filtering (`/api/v1/students`)
- [ ] **Payers** — CRUD, soft delete, pagination + filtering (`/api/v1/payers`)
- [ ] Student → Class assignment endpoint

#### Milestone 5 — Transactions
- [ ] Create transaction with line items (TransactionItems)
- [ ] Nullable `StudentId?` + `PayerId?` on Transaction — two explicit FK columns (one must be set, enforce via check constraint or service validation)
- [ ] `CashLedgerId` (required) — specifies which cash/bank ledger account is used
- [ ] Auto-create journal entries (double-entry); validate debit sum = credit sum before saving
- [ ] Save transaction + items + journal entries inside a single DB transaction
- [ ] Update ledger balances and student/payer balances
- [ ] Same-day edit / delete restriction
- [ ] `POST /api/v1/transactions/{id}/reverse` — reversal marks original `IsReversed = true`; does not soft-delete original
- [ ] Period lock: reject transactions dated in a closed year
- [ ] `CreatedBy` / `UpdatedBy` populated from authenticated user on all transaction writes

---

### Week 4 — Reports

**Goal:** Finance staff can view the full audit trail and generate standard accounting reports without leaving the API.

**Deliverables:**
- Journal entries list with filtering by ledger, type, and date range.
- Trial balance showing all ledger accounts with debit/credit totals.
- Ledger activity summary filterable by fiscal year.
- Student transaction report filterable by grade and class.

#### Milestone 6 — Reports
- [ ] Journal entries list (`/api/v1/journal-entries`)
- [ ] Trial balance (`/api/v1/ledgers/reports/trial-balance`)
- [ ] Ledger summary by year (`/api/v1/ledgers/summary/{year}`)
- [ ] Student transaction report by grade/class (`/api/v1/students/report`)

---

### Week 5 — Year-End, Import & Polish

**Goal:** The system supports the full fiscal-year lifecycle (open → close → re-open) and is production-ready with validation, seed data, and smoke tests.

**Deliverables:**
- Year-end close zeroes revenue/expense ledgers and posts closing entries to Retained Earnings; idempotency guard prevents double-close.
- Year-beginning open posts balanced opening entries for permanent accounts; prior-year-close guard enforced.
- Bulk student import via CSV; returns imported/skipped counts.
- All list endpoints use consistent pagination parameters.
- All write endpoints validated; seed data present on fresh install.
- Smoke tests cover auth, transaction create, and reversal.

#### Milestone 7 — Year-End & Import
- [ ] `ClosedYears` table + EF migration
- [ ] `POST /api/v1/ledgers/year-end-close` — closing entries; idempotency guard; inserts into `ClosedYears`
- [ ] `POST /api/v1/ledgers/year-beginning-open` — opening entries with correct debit/credit direction; balance check
- [ ] `POST /api/v1/students/import` — CSV upload (multipart/form-data); returns imported/skipped counts

#### Milestone 8 — Polish
- [ ] Consistent pagination query parameters (`page`, `perPage`, `sortBy`, `sortDesc`)
- [ ] Input validation on all endpoints
- [ ] Seed data (chart of accounts, sample students/classes)
- [ ] xUnit smoke tests for critical paths (auth, transaction create, reversal)

---

## Folder Structure (Vertical Slice)

```
SchoolAccounting.Api/
├── Program.cs
├── appsettings.json
│
├── Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs       # single EF Core DbContext, injected into every service
│   │   ├── DbInitializer.cs      # seeds default Admin on cold start
│   │   └── Migrations/
│   ├── Auth/
│   │   └── BearerTokenAuthHandler.cs  # EF token lookup → ClaimsPrincipal with role claim
│   └── Middleware/
│       └── ExceptionHandlingMiddleware.cs
│
├── Common/
│   ├── AppRole.cs                # const string role definitions: Admin, Accountant, Staff, Viewer
│   ├── PagedResult.cs            # shared pagination envelope
│   └── Exceptions/               # NotFoundException, ConflictException, etc.
│
└── Features/
    ├── Auth/
    │   ├── AuthController.cs
    │   ├── AuthService.cs
    │   └── AuthDto.cs
    ├── UserManagement/             # Admin-only — list/create/update users, assign roles
    │   ├── UserManagementController.cs
    │   ├── UserManagementService.cs
    │   └── UserManagementDto.cs
    ├── Students/
    │   ├── StudentsController.cs
    │   ├── StudentService.cs
    │   ├── StudentDto.cs         # Requests + Responses in one file
    │   └── StudentMappings.cs    # static ToResponse() / ToEntity() extension methods
    ├── Transactions/             # most complex feature — split services allowed
    │   ├── TransactionsController.cs
    │   ├── TransactionService.cs
    │   ├── TransactionReversalService.cs
    │   ├── YearEndCloseService.cs
    │   ├── TransactionRequests.cs
    │   ├── TransactionResponses.cs
    │   └── TransactionMappings.cs
    ├── Ledgers/
    ├── Categories/
    ├── Grades/
    ├── Classes/
    ├── Payers/
    ├── JournalEntries/
    └── BusinessInfo/
```

**Rules:**
- Each `Features/<Name>/` folder owns its controller, service(s), DTOs, and mappings.
- Controllers are the HTTP boundary only — 3–7 lines per action, no business logic.
- Services inject `AppDbContext` directly — no repository layer.
- Mappings are static extension methods — no AutoMapper.
- No service crosses feature boundaries. Reports that join multiple tables get their own feature folder.

---

## Changelog

One file per week in [backend/changelogs](backend/changelogs). When a week's goals are fully achieved, create a file `YYYY-MM-DD_weekN.md` (actual date, week just completed) and fill it in.

**File naming:** `2026-04-15_week1.md`, `2026-04-22_week2.md`, …
```
# YYYY-MM-DD_weekN
**Completed:** Week N — <week title>
## Changes
-
## Notes / blockers encountered
-
```

---

## Deployment

**Target:** Azure — App Service + Azure SQL Database + Key Vault

### Azure Resources

| Resource | SKU / Tier | Purpose |
|---|---|---|
| Resource Group | — | Container for all resources |
| Azure SQL Database | Basic or Standard S1 | SQL Server–compatible managed database |
| App Service Plan | B1 or higher | Compute host |
| App Service | .NET 10 stack | Runs the API |
| Azure Key Vault | Standard | Stores secrets; App Service reads via Key Vault references |

---

### Config Changes Before First Deploy

**1. Connection string — switch from Windows Auth to SQL Auth:**
```
Server=tcp:<your-server>.database.windows.net,1433;
Initial Catalog=SchoolAccounting;
User ID=<sqladmin>;
Password=<password>;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

**2. CORS — parameterize allowed origins:**

`appsettings.json` (add):
```json
"AllowedOrigins": [ "https://your-frontend.com" ]
```

Set via App Service → Configuration → Application Settings in production. Do not hardcode the production URL in source.

---

### Key Vault Secrets

| Key Vault secret name | App Service Application Setting | Description |
|---|---|---|
| `ConnectionStrings--DefaultConnection` | `ConnectionStrings:DefaultConnection` | Azure SQL connection string |
| `AdminSeedPassword` | `AdminSeedPassword` | Seeded admin password — rotate immediately after first login |

App Service reads Key Vault secrets via reference syntax — no SDK changes required in the app:
```
@Microsoft.KeyVault(SecretUri=https://<vault>.vault.azure.net/secrets/<name>/)
```

---

### EF Migrations

`DbInitializer` calls `db.Database.MigrateAsync()` on startup. EF applies all pending migrations automatically on each deploy — no separate migration step or script needed.

---

### GitHub Actions — CI/CD

Workflow file: `.github/workflows/deploy.yml`

```yaml
name: Build and Deploy

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release
      - run: dotnet publish -c Release -o ./publish

      - uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

**GitHub repo secrets required:**

| Secret | Where to get it |
|---|---|
| `AZURE_WEBAPP_NAME` | Your App Service name in Azure Portal |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | App Service → Overview → Download publish profile |

---

### First-Run Checklist

1. Create Resource Group
2. Create Azure SQL Database; note server name and SQL admin credentials
3. Store connection string in Key Vault (`ConnectionStrings--DefaultConnection`)
4. Create App Service (.NET 10); enable system-assigned managed identity
5. Grant Key Vault `Secret User` role to the App Service managed identity
6. Add Key Vault reference for `ConnectionStrings:DefaultConnection` in App Service Application Settings
7. Set `AllowedOrigins` Application Setting to the production front-end URL
8. Add `AZURE_WEBAPP_NAME` and `AZURE_WEBAPP_PUBLISH_PROFILE` secrets to the GitHub repo
9. Push to `main` — confirm the Actions workflow passes and the app starts
10. Log in as seeded `admin` / `admin@school.local`; **change the password immediately**
11. Create remaining user accounts and assign roles via `PUT /api/v1/users/{id}/role`
