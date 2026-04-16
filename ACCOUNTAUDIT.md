# School Accounting System — Audit Report

**Date:** 2026-04-12
**Scope:** Full-stack audit (Backend: ASP.NET Core 10 / Frontend: Vue 3 + TypeScript)
**Accounting Method:** Cash Basis

---

## Executive Summary

The system is a functional cash basis school accounting application covering transaction recording, chart of accounts, student/payer management, and basic year-end procedures. However, it has significant gaps in financial reporting, fee management automation, and frontend-backend alignment that limit its usefulness as a complete school accounting system.

---

## 1. Frontend–Backend Disconnects (Critical)

### 1.1 P&L and Balance Sheet — Broken Report Pages
- Frontend routes and pages exist: `/accounting/reports/profit-loss`, `/accounting/reports/balance-sheet`
- **No corresponding backend API endpoints exist for either report**
- These pages are calling APIs that do not exist — they will render empty or error silently
- **Action Required:** Either implement backend endpoints or remove the frontend pages

### 1.2 Student Report Route Confusion
- The Reports hub links "Student Report" to `/students/reports` (enrollment/balance report)
- This is not a financial statement — the label is misleading to end users

### 1.3 Dead Dependency
- `vue-signature-pad` 3.0.2 is installed in the frontend but never used anywhere
- **Action Required:** Remove from package.json

---

## 2. Missing Core Accounting Features

### 2.1 Financial Statements (High Priority)
| Statement | Backend | Frontend | Status |
|---|---|---|---|
| Trial Balance | ✅ | ✅ | Working |
| Profit & Loss | ❌ | ✅ (broken) | API missing |
| Balance Sheet | ❌ | ✅ (broken) | API missing |
| Cash Flow Statement | ❌ | ❌ | Fully missing |
| Statement of Changes in Equity | ❌ | ❌ | Fully missing |

### 2.2 Manual Journal Entries
- `JournalEntry` records are auto-generated only — read-only from the API
- No ability to create manual adjusting entries, correction entries, or accrual entries
- **Standard accounting systems require manual journal entry capability**

### 2.3 Bank Reconciliation
- No bank statement import (CSV/OFX)
- No reconciliation workflow to match recorded transactions against bank records
- No unreconciled transaction tracking

### 2.4 Budget Management
- No budget creation, period allocation, or tracking
- No budget vs. actual variance report
- No overspending controls or warnings

### 2.5 Fixed Assets / Asset Register
- No asset register for school equipment, furniture, or vehicles
- No depreciation calculation (even basic straight-line method)
- No asset disposal or write-off tracking

---

## 3. School-Specific Feature Gaps

### 3.1 Fee Billing Automation (High Priority)
- `StudentClass.FeeAmount` is defined in the database and frontend form
- **No automated fee billing** — fees are never generated as transactions
- No fee schedule per grade, term, or semester
- No overdue/outstanding fee tracking
- No late fee or penalty calculation
- No scholarship, discount, or waiver management
- No bulk fee generation for an entire class

### 3.2 Accounts Receivable
- No invoice generation per student
- No AR aging report (0–30, 31–60, 61–90+ days overdue)
- No payment due dates on outstanding balances
- Student `Balance` field exists but has no billing cycle behind it

### 3.3 Accounts Payable
- No vendor bill/invoice management
- No AP aging report
- No payment scheduling for recurring expenses

---

## 4. Missing Frontend Pages (Backend API Exists, No UI)

| Feature | Backend Endpoint | Frontend Page |
|---|---|---|
| User Management | `GET/POST/PUT/DELETE /api/v1/users` | ❌ Missing |
| Grades Management | Full `GradesController` | ❌ Missing (only accessible via class form) |
| Journal Entries Viewer | `GET /api/v1/journal-entries` | ❌ Missing |
| Audit Log Viewer | `AuditLog` table fully populated | ❌ Missing |
| Own Profile / Password Change | `PUT /api/v1/users/{id}/password` | ❌ Missing |
| Active Sessions / Token Revoke | `PersonalAccessToken` tracked in DB | ❌ Missing |

None of these have sidebar navigation links.

---

## 5. Frontend-Only Weaknesses

### 5.1 Notification System — Non-Functional
- `notification.ts` Pinia store is empty
- Header bell icon is decorative — no real-time or async notifications wired up

### 5.2 Dark Mode — Declared, Not Implemented
- Header has a dark mode indicator but no theme switching logic exists

### 5.3 No Export or Print for Reports
- None of the report pages have PDF or Excel export
- A `/src/assets/pdf/` directory exists but is unused for report output

### 5.4 Quick Payment Asymmetry
- `/accounting/expenses/quick/:id` exists for expenses
- No equivalent quick-receipt flow for income

### 5.5 No Bulk Operations
- All list pages support single-row actions only
- No bulk delete, bulk activate/deactivate, or bulk fee assignment

### 5.6 Dashboard Has No Period Filter
- Monthly income/expense charts appear static
- No date range selector to change the visualization period

### 5.7 No Own Account Settings
- Users cannot change their own name, email, or password from the UI
- Only an Admin can do it — via the user management page that **does not exist in the frontend**

---

## 6. Backend-Only Weaknesses

### 6.1 Category Soft Delete Missing
- `Category` is the only major entity without `DeletedAt` (soft delete)
- Hard-deleting a category referenced by historical `TransactionItem` records would break reporting integrity

### 6.2 Recurring Transactions — Incomplete
- `Payer.IsRecurring` flag exists in both DB and frontend form
- No recurring transaction engine, scheduler, or automation behind it
- The flag is captured but never acted upon

### 6.3 Tax Calculation — Incomplete
- `BusinessInfo.TaxRate` (decimal) is stored
- Tax is never applied in transaction processing or line item calculation
- No tax reports or GST/SST tracking

### 6.4 Opening Balances for New Setup
- No mechanism to import historical balances when migrating from another system
- `yearBeginningOpen` only works from a prior year-end close — unusable for initial setup

### 6.5 Token Management
- No endpoint to list all active sessions for a user
- No revoke-all-sessions capability for compromised accounts
- No token refresh — expired tokens require full re-login

### 6.6 Self-Service Password Reset
- No email-based password reset flow
- Admin must manually reset all user passwords

### 6.7 Period Locking
- Only annual year-end close exists
- No monthly or quarterly period locking to prevent backdated entries
- No posting period controls

---

## 7. Missing Reports Summary

| Report | Backend | Frontend |
|---|---|---|
| Trial Balance | ✅ | ✅ |
| Profit & Loss | ❌ | ✅ (broken — no API) |
| Balance Sheet | ❌ | ✅ (broken — no API) |
| Cash Flow Statement | ❌ | ❌ |
| AR Aging | ❌ | ❌ |
| AP Aging | ❌ | ❌ |
| Student Fee Outstanding | ❌ | ❌ |
| Class Revenue Summary | ❌ | ❌ |
| Donor / Sponsor Contributions | ❌ | ❌ |
| Payment Method Breakdown | ❌ | ❌ |
| Year-over-Year Comparison | ❌ | ❌ |
| Audit Log Report | ✅ (DB only) | ❌ |
| Journal Entry Report | ✅ (API only) | ❌ |

---

## 8. Prioritized Action List

### High Priority
1. **Implement P&L and Balance Sheet backend endpoints** — or remove the broken frontend pages
2. **Build fee billing automation** — leverage existing `FeeAmount` on `StudentClass`
3. **Add User Management frontend page** — backend is fully built, no UI exists
4. **Add soft delete to `Category`** — prevents data integrity issues on deletion
5. **Add Audit Log and Journal Entry viewer pages** — data exists, no UI

### Medium Priority
6. **Manual journal entry creation** — required for adjustments and corrections
7. **Grades management frontend page**
8. **Own account profile/password change page**
9. **Cash Flow Statement** (backend + frontend)
10. **Bank reconciliation workflow**
11. **Complete tax calculation** in transactions
12. **Period locking** (monthly/quarterly)
13. **Dashboard period filter**

### Low Priority
14. **Budget management module**
15. **Fixed asset register**
16. **Recurring transaction engine** (wire up the existing `IsRecurring` flag)
17. **Report export to PDF/Excel**
18. **Remove `vue-signature-pad` dead dependency**
19. **Implement notification system**
20. **Self-service password reset via email**
21. **Token/session management UI**

---

## 9. Data Integrity Risks

| Risk | Severity | Description |
|---|---|---|
| Category hard delete | High | Can break historical transaction item records |
| No period locking | Medium | Allows backdated entries into closed periods |
| P&L/Balance Sheet pages broken | Medium | Users see empty/error reports without knowing APIs are missing |
| No AR tracking | Medium | Student balances exist but have no billing lifecycle |
| `FeeAmount` never billed | Medium | Class fee configuration is captured but never automated |

---

*This audit was generated from a full review of the backend (ASP.NET Core API, EF Core schema, migrations) and frontend (Vue 3, Pinia stores, router, pages, components, API service layer).*
