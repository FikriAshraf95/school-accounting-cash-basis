# Technical Specifications

Derived from the Laravel source at `school-accounting-api/`.

---

## Database

**Engine:** SQL Server (MSSQL)
**EF Core provider:** `Microsoft.EntityFrameworkCore.SqlServer`
**Migrations:** EF Core code-first migrations (`dotnet ef migrations add`)

Connection string (appsettings.json):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SchoolAccounting;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Registration in `Program.cs`:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Type mappings used:**

| C# / EF type | SQL Server column type |
|---|---|
| `int` (PK) | `INT IDENTITY(1,1)` |
| `string` (sized) | `NVARCHAR(n)` |
| `decimal(p,s)` | `DECIMAL(p, s)` |
| `bool` | `BIT` |
| `DateTime` | `DATETIME2` |
| `DateOnly` | `DATE` |
| `string?` / nullable | column allows `NULL` |

---

## Architectural Pattern

### Controller vs Service

| Layer | Responsibility |
|---|---|
| **Controller** | HTTP only — routing, verbs, `[Authorize]`, status codes. Calls one service method per action. Returns `ActionResult<T>`. |
| **Service** | All business logic, EF Core queries, domain rule enforcement. Throws typed exceptions. No HttpContext knowledge. |
| **Mappings** | Static extension methods (`entity.ToResponse()`, `request.ToEntity()`). No AutoMapper. |

Controllers never touch `DbContext`. Services never return `IActionResult`.

### Service Registration

All services registered as `Scoped` in `Program.cs`:

```csharp
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<TransactionReversalService>();
// ... one line per service
```

No interfaces unless a feature has concrete unit-test isolation needs.

### Naming Conventions

| Concern | Pattern | Example |
|---|---|---|
| Controller | `{Feature}Controller` | `StudentsController` |
| Service | `{Feature}Service` | `StudentService` |
| Sub-service | `{Feature}{Concern}Service` | `TransactionReversalService` |
| Request DTO | `{Action}{Feature}Request` | `CreateStudentRequest` |
| Response DTO | `{Feature}Response` | `StudentResponse` |
| List response DTO | `{Feature}SummaryResponse` | `StudentSummaryResponse` |
| Mappings class | `{Feature}Mappings` | `StudentMappings` |
| Exception | `{Entity}NotFoundException` | `StudentNotFoundException` |

### Exception → HTTP Status Code Mapping

Handled globally in `ExceptionHandlingMiddleware`:

| Exception class | HTTP status |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `BusinessRuleException` | 422 |
| `UnauthorizedException` | 401 |
| `ValidationException` (FluentValidation) | 400 |
| Unhandled `Exception` | 500 |

---

## RBAC

### Roles (`Common/AppRole.cs` — `const string` constants)

| Constant | Value | Access summary |
|---|---|---|
| `AppRole.Admin` | `"Admin"` | Full access including user management |
| `AppRole.Accountant` | `"Accountant"` | All financial ops; no user management |
| `AppRole.Staff` | `"Staff"` | Student/payer CRUD + create/edit transactions; no ledger or year-end |
| `AppRole.Viewer` | `"Viewer"` | Read-only everywhere; **default on registration** |

### How it works

1. `Role` stored as `nvarchar(20)` on the `Users` table.
2. `BearerTokenAuthHandler` (`Infrastructure/Auth/`) looks up the token hash in `PersonalAccessTokens`, loads the user, and emits a `ClaimTypes.Role` claim.
3. ASP.NET Core's built-in `[Authorize(Roles = "...")]` handles enforcement — no extra libraries.

### Authorization pattern

- All controllers: `[Authorize]` at class level.
- Actions needing a subset of roles: additional `[Authorize(Roles = AppRole.X)]` at action level.
- `UserManagementController`: `[Authorize(Roles = AppRole.Admin)]` at class level.
- `AuthController`: no class-level attribute; Register + Login are public.

### Permission matrix

| Feature / Action | Admin | Accountant | Staff | Viewer |
|---|---|---|---|---|
| **UserManagement** all | Y | — | — | — |
| **BusinessInfo** GET | Y | Y | Y | Y |
| **BusinessInfo** PUT | Y | Y | — | — |
| **Grades / Classes** GET | Y | Y | Y | Y |
| **Grades / Classes** POST / PUT | Y | — | Y | — |
| **Grades / Classes** DELETE | Y | — | — | — |
| **Classes** student assign/remove | Y | — | Y | — |
| **Students** GET + report | Y | Y | Y | Y |
| **Students** POST / PUT / import | Y | — | Y | — |
| **Students** DELETE (soft) | Y | — | — | — |
| **Payers** GET | Y | Y | Y | Y |
| **Payers** POST / PUT | Y | Y | Y | — |
| **Payers** DELETE (soft) | Y | Y | — | — |
| **Categories** GET | Y | Y | Y | Y |
| **Categories** POST / PUT / DELETE | Y | Y | — | — |
| **Ledgers** GET + reports | Y | Y | Y | Y |
| **Ledgers** POST / PUT / DELETE | Y | Y | — | — |
| **Ledgers** year-end close / open | Y | Y | — | — |
| **Transactions** GET | Y | Y | Y | Y |
| **Transactions** POST / PUT | Y | Y | Y | — |
| **Transactions** DELETE | Y | Y | — | — |
| **Transactions** reverse | Y | Y | — | — |
| **JournalEntries** GET | Y | Y | Y | Y |

### Bootstrap

`DbInitializer.SeedAsync` runs on startup after migrations. Creates one Admin user (`admin / admin@school.com`, password `ChangeMe123!`) if no Admin exists. Idempotent.

### Last-admin guard

`UserManagementService.AssignRoleAsync` checks: if changing the role of the only remaining Admin to a non-Admin role → throws `BusinessRuleException` (422).

---

## Database Schema

> All `bool` columns map to `BIT` in SQL Server. All `string` columns map to `NVARCHAR`. All monetary/amount columns use `DECIMAL(15,2)`. Timestamps use `DATETIME2`.


### `Users`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Name | nvarchar(255) | |
| Username | nvarchar(255) | unique |
| Email | nvarchar(255) | unique |
| PasswordHash | nvarchar(255) | bcrypt |
| Role | nvarchar(20) | `Admin` / `Accountant` / `Staff` / `Viewer`; default `Viewer` |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

### `PersonalAccessTokens`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| UserId | int FK → Users | |
| Token | nvarchar(64) | SHA-256 hash |
| Name | nvarchar(255) | device/client label |
| LastUsedAt | datetime? | |
| ExpiresAt | datetime? | |
| CreatedAt | datetime | |

### `BusinessInfos`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | single row |
| SchoolName | nvarchar(255) | |
| RegistrationNumber | nvarchar(100)? | |
| Address | nvarchar(500)? | |
| Phone | nvarchar(50)? | |
| Email | nvarchar(255)? | |
| FinancialYearStart | date | |
| FinancialYearEnd | date | |
| Currency | nvarchar(10) | default "MYR" |
| Timezone | nvarchar(100) | |
| BankName | nvarchar(255)? | |
| BankAccountName | nvarchar(255)? | |
| BankAccountNumber | nvarchar(100)? | |
| TaxRegistration | nvarchar(100)? | |
| TaxRate | decimal(5,2) | |

### `StudentGrades`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Name | nvarchar(100) | |
| Code | nvarchar(20) | unique |
| Description | nvarchar(500)? | |
| IsActive | bool | default true |
| DeletedAt | datetime? | soft delete |
| CreatedAt / UpdatedAt | datetime | |

### `StudentClasses`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Name | nvarchar(100) | |
| Code | nvarchar(20) | unique |
| GradeId | int FK → StudentGrades | |
| Section | nvarchar(50)? | |
| Description | nvarchar(500)? | |
| Capacity | int | |
| FeeAmount | decimal(15,2) | |
| IsActive | bool | |
| DeletedAt | datetime? | |
| CreatedAt / UpdatedAt | datetime | |

### `Students`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| StudentId | nvarchar(50) | unique student code |
| Name | nvarchar(255) | |
| ClassId | int? FK → StudentClasses | |
| GradeId | int? FK → StudentGrades | |
| Email | nvarchar(255)? | |
| Phone | nvarchar(50)? | |
| Address | nvarchar(500)? | |
| Balance | decimal(15,2) | running balance |
| IsActive | bool | |
| DeletedAt | datetime? | |
| CreatedAt / UpdatedAt | datetime | |

### `Payers`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| PayerCode | nvarchar(50) | unique |
| Name | nvarchar(255) | |
| Type | nvarchar(30) | donor / sponsor / vendor / supplier / general / government |
| Category | nvarchar(30) | individual / corporate / government / ngo |
| Email | nvarchar(255)? | |
| Phone | nvarchar(50)? | |
| Address | nvarchar(500)? | |
| Balance | decimal(15,2) | |
| IsRecurring | bool | |
| Notes | nvarchar(1000)? | |
| IsActive | bool | |
| DeletedAt | datetime? | |
| CreatedAt / UpdatedAt | datetime | |

### `Ledgers` (Chart of Accounts)
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Code | nvarchar(20) | unique account code |
| Name | nvarchar(255) | |
| Type | nvarchar(20) | asset / liability / equity / revenue / expense |
| Category | nvarchar(100)? | sub-classification |
| Balance | decimal(15,2) | |
| IsActive | bool | |
| DeletedAt | datetime? | |
| CreatedAt / UpdatedAt | datetime | |

### `Categories`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Name | nvarchar(100) | |
| Type | nvarchar(20) | income / expense |
| LedgerId | int FK → Ledgers | |
| Description | nvarchar(500)? | |
| RequiresStudent | bool | |
| IsActive | bool | |
| CreatedAt / UpdatedAt | datetime | |

### `Transactions`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| TransactionNumber | nvarchar(50) | unique |
| TransactionDate | date | |
| Type | nvarchar(20) | income / expense |
| TransactableType | nvarchar(50) | "Student" or "Payer" |
| TransactableId | int | polymorphic FK |
| Amount | decimal(15,2) | total amount |
| PaymentMethod | nvarchar(50)? | |
| ReferenceNumber | nvarchar(100)? | |
| Description | nvarchar(1000)? | |
| ReceiptNumber | nvarchar(100)? | internally generated for income transactions |
| CashLedgerId | int FK → Ledgers | asset-type ledger used as the cash/bank account; required |
| IsReversed | bool | default false; true when a reversal has been posted against this transaction |
| ReversalTransactionId | int? FK → Transactions (self) | FK to the reversing transaction |
| CreatedBy | int FK → Users | user who posted this transaction |
| UpdatedBy | int? FK → Users | user who last edited this transaction |
| DeletedAt | datetime? | soft delete |
| CreatedAt / UpdatedAt | datetime | |

**Indexes:** `(TransactionDate, Type)`, `(TransactableType, TransactableId)`, `(CashLedgerId)`

### `TransactionItems`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| TransactionId | int FK → Transactions (cascade) | |
| CategoryId | int FK → Categories | |
| Amount | decimal(15,2) | line total |
| Description | nvarchar(500)? | |
| Quantity | int | default 1 |
| UnitPrice | decimal(15,2) | |
| CreatedAt / UpdatedAt | datetime | |

### `JournalEntries`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| TransactionId | int? FK → Transactions | null for adjustment/closing |
| LedgerId | int FK → Ledgers | |
| EntryType | nvarchar(10) | debit / credit |
| Amount | decimal(15,2) | |
| EntryDate | date | |
| Description | nvarchar(500)? | |
| JournalType | nvarchar(20) | `transaction` / `reversal` / `adjustment` / `closing` / `opening` |
| CreatedBy | int FK → Users | user whose action triggered this posting |
| CreatedAt / UpdatedAt | datetime | |

**Indexes:** `(LedgerId, EntryDate)`, `(TransactionId)`

### `ClosedYears`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| Year | int | unique; e.g. 2024 |
| ClosedAt | datetime | |
| ClosedBy | int FK → Users | |

---

## API Endpoints

All routes prefixed `/api/v1/`. Protected routes require `Authorization: Bearer <token>`.

### Auth
```
POST   /register         body: { name, username, email, password }
POST   /login            body: { email, password }
POST   /logout           [auth]
GET    /user             [auth]
```

### Business Info
```
GET    /business-info    [auth]
PUT    /business-info    [auth]  body: BusinessInfo fields
```

### Student Grades
```
GET    /grades           [auth]  ?page &perPage &sortBy &sortDesc
POST   /grades           [auth]
GET    /grades/{id}      [auth]
PUT    /grades/{id}      [auth]
DELETE /grades/{id}      [auth]
```

### Student Classes
```
GET    /classes                              [auth]
POST   /classes                              [auth]
GET    /classes/{id}                         [auth]
PUT    /classes/{id}                         [auth]
DELETE /classes/{id}                         [auth]  fails if students exist
POST   /classes/{classId}/students           [auth]  body: { studentId }
DELETE /classes/{classId}/students/{studentId}  [auth]
```

### Students
```
GET    /students             [auth]  ?page &perPage &sortBy &sortDesc &search &classId &gradeId &isActive
POST   /students             [auth]
GET    /students/{id}        [auth]  includes transaction history
PUT    /students/{id}        [auth]
DELETE /students/{id}        [auth]  soft delete
GET    /students/report      [auth]  ?gradeId &classId &dateFrom &dateTo
POST   /students/import      [auth]  multipart/form-data; CSV file upload; returns count of imported/skipped rows
```

### Payers
```
GET    /payers           [auth]  ?page &perPage &sortBy &sortDesc &search &type &isActive
POST   /payers           [auth]
GET    /payers/{id}      [auth]  includes transaction history
PUT    /payers/{id}      [auth]
DELETE /payers/{id}      [auth]  soft delete; fails if has transactions
```

### Ledgers
```
GET    /ledgers                          [auth]  ?type &isActive
POST   /ledgers                          [auth]
GET    /ledgers/{id}                     [auth]
PUT    /ledgers/{id}                     [auth]
DELETE /ledgers/{id}                     [auth]  fails if has journal entries
GET    /ledgers/summary/{year}           [auth]
GET    /ledgers/reports/trial-balance    [auth]  ?year
POST   /ledgers/year-end-close          [auth]  body: { year }
POST   /ledgers/year-beginning-open     [auth]  body: { year }
```

### Categories
```
GET    /categories       [auth]  ?type &isActive
POST   /categories       [auth]
GET    /categories/{id}  [auth]
PUT    /categories/{id}  [auth]
DELETE /categories/{id}  [auth]  fails if used in transaction items
```

### Transactions
```
GET    /transactions             [auth]  ?page &perPage &sortBy &sortDesc &type &dateFrom &dateTo &transactableType &transactableId
POST   /transactions             [auth]
GET    /transactions/{id}        [auth]
PUT    /transactions/{id}        [auth]  same-day only
DELETE /transactions/{id}        [auth]  same-day only
POST   /transactions/{id}/reverse  [auth]
```

### Journal Entries
```
GET    /journal-entries  [auth]  ?page &perPage &ledgerId &journalType &dateFrom &dateTo
```

---

## Business Rules

### Transaction Creation
1. Validate `TransactableType` must be `"Student"` or `"Payer"`; validate the referenced entity exists and is active.
2. Validate `CashLedgerId` exists, is active, and is `Type = asset`.
3. Validate all `CategoryId`s in items exist, are active, and match transaction `Type`.
4. If any item's Category has `RequiresStudent = true`, `TransactableType` must be `"Student"`; otherwise throw `BusinessRuleException`.
5. `Amount` on Transaction = sum of item amounts.
6. Generate unique `TransactionNumber` (`TXN-YYYYMMDD-NNNN`). NNNN is a zero-padded daily sequence — use a DB unique constraint on `TransactionNumber` and retry on conflict to handle concurrent inserts.
7. Check period lock: if `TransactionDate` falls in a year present in `ClosedYears`, throw `BusinessRuleException`.
8. Build `JournalEntries` (double-entry):
   - **Income (multi-item)**: one DEBIT on `CashLedgerId` for the full transaction amount; one CREDIT per item on that item's Category `LedgerId`.
   - **Expense (multi-item)**: one DEBIT per item on that item's Category `LedgerId`; one CREDIT on `CashLedgerId` for the full transaction amount.
9. Validate sum(debit entry amounts) = sum(credit entry amounts) before persisting; throw `BusinessRuleException` on mismatch.
10. Save transaction, items, and journal entries inside a single DB transaction (`await db.Database.BeginTransactionAsync()`).
11. Update `Ledger.Balance` for each affected ledger.
12. Update `Student.Balance` or `Payer.Balance` (income: add amount; expense: subtract amount).

### Transaction Edit / Delete
- Only allowed on the **same calendar day** as `TransactionDate`.
- On delete: soft delete transaction, reverse journal entries, restore balances.

### Transaction Reversal
- Available any time (reversal transaction is always dated today, so it is not subject to the same-day edit rule).
- Cannot reverse an already-reversed transaction (`IsReversed = true`) — throw `BusinessRuleException`.
- Creates a new reversal transaction with the opposite `Type`, `ReferenceNumber = "REVERSAL OF {original.TransactionNumber}"`, and `TransactionDate = today`.
- Sets on the original: `IsReversed = true`, `ReversalTransactionId = newTransaction.Id`. Does **not** soft-delete the original — it remains visible in history.
- New `JournalEntries` for the reversal use `JournalType = 'reversal'`.
- Updates `Ledger.Balance` and `Student/Payer.Balance` for the reversal transaction (opposite sign to original).

### Year-End Close
1. Guard: if `ClosedYears` already contains `year`, throw `BusinessRuleException` (already closed).
2. Sum all `revenue` ledger balances → net revenue.
3. Sum all `expense` ledger balances → net expense.
4. Net profit = net revenue − net expense.
5. Create closing `JournalEntries` (`JournalType = 'closing'`, `TransactionId = null`):
   - DEBIT each revenue ledger (zeroing it), CREDIT `3000 Retained Earnings` for the same amount.
   - CREDIT each expense ledger (zeroing it), DEBIT `3000 Retained Earnings` for the same amount.
6. Set all revenue/expense `Ledger.Balance` to 0.
7. Insert row into `ClosedYears` (`Year`, `ClosedAt = now`, `ClosedBy = current user Id`).

### Year-Beginning Open
1. Guard: `ClosedYears` must contain year − 1 — cannot open a year without closing the prior year first.
2. For each `asset` ledger with non-zero balance: create `JournalType = 'opening'`, `EntryType = 'debit'`, `Amount = Ledger.Balance`.
3. For each `liability` and `equity` ledger with non-zero balance: create `JournalType = 'opening'`, `EntryType = 'credit'`, `Amount = Ledger.Balance`.
4. Validate total opening debit amounts = total opening credit amounts (assets = liabilities + equity); throw `BusinessRuleException` if unbalanced.
5. `Ledger.Balance` for permanent accounts is **not** reset — the opening entries are the journal record of the carry-forward, not a balance change.

### Period Lock
- Any `POST /transactions` or `PUT /transactions/{id}` is rejected when `TransactionDate` falls in a year listed in `ClosedYears`.
- Reversals are always dated today, so reversing a prior-year transaction is allowed as long as today's year is not closed.
- Year-end close and year-beginning open journal entries are system-generated and are exempt from the period lock.

### Ledger Balance Scope
- `revenue` and `expense` ledgers: `Ledger.Balance` tracks the **current open fiscal year** (reset to 0 on year-end close).
- `asset`, `liability`, and `equity` ledgers: `Ledger.Balance` is a **running cumulative** balance that persists across year-end.
- If `Ledger.Balance` becomes inconsistent it can be recomputed from `JournalEntries` (sum debits, subtract credits for debit-normal accounts; reversed for credit-normal accounts). Implement as a private `RecalculateBalance(ledgerId)` helper in `LedgerService` — no public endpoint needed in Phase 1.

### Balance Sign Convention
- `Student.Balance` and `Payer.Balance`: income transactions add, expense transactions subtract. Positive = net receipts on account.
- `Ledger.Balance` for debit-normal accounts (`asset`, `expense`): positive = net debit balance.
- `Ledger.Balance` for credit-normal accounts (`liability`, `equity`, `revenue`): positive = net credit balance.

### Payment Method Values
Valid values for `Transactions.PaymentMethod`:

| Value | Description |
|---|---|
| `cash` | Physical cash |
| `bank_transfer` | Bank or wire transfer |
| `check` | Cheque payment |
| `online` | Online payment gateway |
| `other` | Any other method |

Validated at the API boundary (service layer); stored as `nvarchar(50)` without a DB check constraint.

---

## Pagination Query Parameters

Standard across all list endpoints:

| Param | Default | Notes |
|---|---|---|
| page | 1 | |
| perPage | 15 | max 100 |
| sortBy | "createdAt" | column name (camelCase) |
| sortDesc | false | |
| paginate | true | set false for full list |
| search | — | keyword filter (alphanumeric + spaces only) |

Response envelope:
```json
{
  "data": [...],
  "meta": {
    "total": 100,
    "page": 1,
    "perPage": 10,
    "lastPage": 7
  }
}
```

---

## Seeded Chart of Accounts

| Code | Name | Type |
|---|---|---|
| 1000 | Cash on Hand | asset |
| 1010 | Cash in Bank | asset |
| 1100 | Accounts Receivable | asset |
| 1200 | Prepaid Expenses | asset |
| 2000 | Accounts Payable | liability |
| 2100 | Accrued Liabilities | liability |
| 3000 | Retained Earnings | equity |
| 3100 | Owner's Equity | equity |
| 4000 | Tuition Fee Income | revenue |
| 4010 | Miscellaneous Income | revenue |
| 4020 | Donations Received | revenue |
| 4030 | Government Grants | revenue |
| 5000 | Salaries Expense | expense |
| 5010 | Utilities Expense | expense |
| 5020 | Office Supplies | expense |
| 5030 | Maintenance & Repairs | expense |
| 5040 | School Supplies | expense |
| 5050 | Transportation Expense | expense |
| 5060 | Communication Expense | expense |
| 5070 | Miscellaneous Expense | expense |

---

## CORS

Allow origins: `http://localhost:3000`, `http://127.0.0.1:3000`
Allow methods: all
Allow credentials: true

---

## Error Response Format

```json
{
  "message": "Human-readable error",
  "errors": {              // optional, validation errors
    "fieldName": ["error detail"]
  }
}
```

HTTP status codes:
- `400` — validation error
- `401` — unauthenticated
- `403` — forbidden
- `404` — not found
- `409` — conflict (e.g. duplicate code, entity in use)
- `422` — business rule violation
- `500` — server error
