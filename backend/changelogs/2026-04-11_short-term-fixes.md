# 2026-04-11_short-term-fixes

**Completed:** Short-Term Fixes from Security Audit

## Changes

### Fix #7 — Global Soft-Delete Query Filters (`AppDbContext.cs`)

- Added `HasQueryFilter(e => e.DeletedAt == null)` in `OnModelCreating` for all 6 soft-deletable entities: Transaction, Student, StudentGrade, StudentClass, Payer, Ledger
- Deleted records are now automatically excluded from all EF queries without relying on every service to remember to add `Where(x => x.DeletedAt == null)`
- Use `.IgnoreQueryFilters()` on a specific query if deleted records are ever intentionally needed
- EF Core emits advisory warnings for Ledger→Category and Ledger→JournalEntry required-end relationships — these are safe because existing business rules already prevent deletion when linked records exist

### Fix #8 — Transaction Number Race Condition (`TransactionService.cs`, `TransactionReversalService.cs`)

- Replaced the sequential number read-then-write pattern with a GUID-based suffix
- Old: `TXN-20260411-0001` (read last number from DB, then increment — race condition under concurrency)
- New: `TXN-20260411-A3F2B1C4` (8-char uppercase hex from `Guid.NewGuid()` — no DB read needed)
- Same change for reversal numbers: `REV-20260411-{8-char GUID}`
- Both generator methods are now `static` since they no longer need the DB context
- The `UNIQUE` index on `TransactionNumber` remains as the final safety net

### Fix #9 — Balance Update Algorithm Documentation (`TransactionService.cs`)

- Added a full debit/credit rule table and two worked accounting examples (income + expense) as comments in `UpdateLedgerBalanceAsync`
- No logic changes — documentation only

### Fix #10 — Test Coverage Expansion

- Added `SmokeTests/StudentServiceSmokeTests.cs` with 5 tests:
  - `CreateStudent_ShouldSucceed`
  - `CreateStudent_DuplicateStudentId_ShouldThrowConflict`
  - `GetStudents_ShouldReturnPagedResults`
  - `DeleteStudent_ShouldSoftDelete`
  - `GetStudent_AfterDelete_ShouldThrowNotFound`
- Added `SmokeTests/LedgerServiceSmokeTests.cs` with 5 tests:
  - `CreateLedger_ShouldSucceed`
  - `CreateLedger_InvalidType_ShouldThrowBusinessRule`
  - `CreateLedger_DuplicateCode_ShouldThrowConflict`
  - `DeleteLedger_WithNonZeroBalance_ShouldThrowConflict`
  - `GetLedgers_ShouldReturnPagedResults`
- **Total: 24 tests, all passing**
- Also upgraded `Microsoft.EntityFrameworkCore.InMemory` from `10.0.0-preview.2` → `10.0.0` (stable)

### Fix #11 — Missing `CreatedAt` Index (`AppDbContext.cs`, new migration)

- Added `entity.HasIndex(e => e.CreatedAt)` on the Transactions table
- Note: `{TransactionDate, Type}` composite index was already present — the audit report was incorrect in saying it was missing
- New migration: `AddCreatedAtIndexAndQueryFilters`

### Fix #12 — Constants File for Magic Strings (`Common/TransactionConstants.cs`)

- New file with named constants for all repeated string literals:
  - `TxnPrefix = "TXN-"`, `RevPrefix = "REV-"`, `RcpPrefix = "RCP-"`
  - `TypeIncome = "income"`, `TypeExpense = "expense"`
  - `EntryDebit = "debit"`, `EntryCredit = "credit"`
  - `JournalTransaction`, `JournalClosing`, `JournalOpening`
- Applied `TransactionConstants.EntryDebit/EntryCredit` in `UpdateLedgerBalanceAsync`
- Applied `TransactionConstants.TxnPrefix / RevPrefix` in number generators

## Files Changed

| File | Change |
|---|---|
| `Infrastructure/Persistence/AppDbContext.cs` | Global query filters + `CreatedAt` index |
| `Features/Transactions/TransactionService.cs` | GUID number generation + balance algorithm docs + constants |
| `Features/Transactions/TransactionReversalService.cs` | GUID reversal number generation + constants |
| `Common/TransactionConstants.cs` | New file — magic string constants |
| `SmokeTests/StudentServiceSmokeTests.cs` | New file — 5 tests |
| `SmokeTests/LedgerServiceSmokeTests.cs` | New file — 5 tests |
| `SchoolAccounting.Api.Tests.csproj` | EF InMemory `preview.2` → `10.0.0` stable |
| Migration: `AddCreatedAtIndexAndQueryFilters` | New EF migration |

## Notes

- No breaking changes to the API surface or database schema (the migration only adds an index)
- The global query filters make the many existing `Where(x => x.DeletedAt == null)` checks in service files redundant — they can be cleaned up incrementally but pose no correctness risk as-is
- Transaction numbers are no longer sequential-per-day; they are now random within each date prefix. If sequential numbers are required for business/regulatory reasons, a database sequence object would be the correct solution
