# School Accounting — Vue 3 Frontend (PHASE 1)

## Summary

A Vue 3 SPA consuming the School Accounting REST API. Handles student management, payer tracking, income/expense transactions, accounting reports, and year-end operations.

**Core principles:**
- KISS — reuse existing UI components; no new abstractions for one-off pages
- Use existing `useAPI()` composable and `stores/api.ts` for all HTTP calls
- Reuse template components: Header, SideBar, MainContent, Pagination, DatePicker
- One Detail.vue per module serves both create and edit (check `route.params.id`)
- No new UI atoms — everything is in `components/ui/` already

---

## Technology Stack

| Concern | Choice |
|---|---|
| Framework | Vue 3 + TypeScript |
| Build | Vite |
| Styling | Tailwind CSS + Radix Vue (`components/ui/`) |
| State | Pinia — auth + sidebar persist to localStorage |
| HTTP | Axios via `useAPI()` composable (`services/api.ts`) |
| Routing | Vue Router 4 (`router/routes.ts`) |
| Icons | Lucide + Iconify |
| Notifications | vue-sonner (toast) |
| Charts | Chart.js via vue-chartjs (Chart1.vue, Chart2.vue) |

---

## Existing Infrastructure (Already Built)

| Item | Location | Status |
|---|---|---|
| Auth pages | `pages/auth/Login.vue`, `Register.vue` | done |
| Layouts | `layouts/AuthLayout.vue`, `PublicLayout.vue` | done |
| Templates | Header, SideBar, MainContent, Pagination, DatePicker, YearEndClosing | done |
| Stores | `stores/auth.ts`, `stores/sidebar.ts`, `stores/api.ts` | done |
| API composable | `services/api.ts` — bearer auth, cancellation, retry, 401 redirect | done |
| API store methods | Students, Classes, Transactions, Categories, Ledgers | done |
| Router | All routes pre-defined in `router/routes.ts` | done |
| UI library | 46 component categories in `components/ui/` | done |

---

## Milestones

---

### Week 1 — Foundation & Wiring

**Goal:** The app is navigable end-to-end. Auth guard active. Dashboard renders. Sidebar links work.

**Deliverables:**
- Login and register flow fully functional against the live API.
- Auth guard enabled — unauthenticated users redirected to `/login`.
- Dashboard renders a stub with welcome card and key stat placeholders.
- Sidebar nav items wired to correct named routes.
- `.env.development` API base URL confirmed matching backend port.

#### Milestone 1 — Auth Wiring
- [x] Verify `Login.vue` / `Register.vue` POST to correct endpoints and store token
- [x] Enable `beforeEnter: requireAuth` on all protected routes in `routes.ts`
- [x] Enable `beforeEnter: redirectIfAuthenticated` on `/login` and `/register`
- [x] Confirm `logout()` in auth store clears token and navigates to `/login`
- [x] Verify 401 interceptor in `services/api.ts` clears auth and redirects to login

#### Milestone 2 — Shell & Navigation
- [x] Create `pages/dashboard/Index.vue` — welcome card + stat placeholders (students count, transactions count)
- [x] Wire sidebar nav items to their named routes (students, payer, classes, accounting sections)
- [x] Confirm `AuthLayout.vue` renders Header + Sidebar + MainContent for all protected routes
- [x] Confirm `VITE_API_BASE_URL` in `.env.development` matches backend port

---

### Week 2 — Reference Data Pages

**Goal:** Staff can manage business info, chart of accounts, categories, and classes from the UI.

**Deliverables:**
- Business info view/edit page (single-record form).
- Ledger (chart of accounts) list with create/edit/view.
- Category create/edit/view linked to a ledger.
- Classes list with create/edit/view.
- `stores/api.ts` extended with missing methods.

#### Milestone 3 — API Store Extensions
- [x] Add `getGrades()` to `stores/api.ts` → `GET /grades` (used as dropdown in Class and Student forms)
- [x] Add `getPayers(params)`, `getPayer(id)`, `createPayer(data)`, `updatePayer(id, data)`, `deletePayer(id)` to `stores/api.ts`
- [x] Add `getBusinessInfo()`, `updateBusinessInfo(data)` to `stores/api.ts`

#### Milestone 4 — Business Info & Ledgers
- [x] `pages/modules/accounting/main/View.vue` — display business name, address, contact; Edit button → `edit_main`
- [x] `pages/modules/accounting/main/Detail.vue` — edit form (name, address, phone, email) + save → `PUT /business-info`
- [x] `pages/modules/accounting/ledger/Index.vue` — paginated table (code, name, type, balance); link to create/view
- [x] `pages/modules/accounting/ledger/Detail.vue` — shared create/edit form (code, name, type select, normal balance); save → POST or PUT
- [x] `pages/modules/accounting/ledger/View.vue` — read-only ledger card (code, name, type, balance); Edit / Back buttons

#### Milestone 5 — Categories & Classes
- [x] `pages/modules/accounting/category/Detail.vue` — create/edit form (name, type select, ledger select); save
- [x] `pages/modules/accounting/category/View.vue` — read-only category detail; Edit / Back buttons
- [x] `pages/modules/classes/Index.vue` — table (name, grade, student count) with search and Pagination
- [x] `pages/modules/classes/Detail.vue` — create/edit form (name, grade select via `getGrades()`)
- [x] `pages/modules/classes/View.vue` — class card + enrolled students list; assign/remove student buttons

---

### Week 3 — People

**Goal:** Students and payers are fully manageable with search, pagination, and class assignment.

**Deliverables:**
- Students list: search by name, filter by grade/class, pagination.
- Student create/edit form. Class assignment on view page.
- CSV import button on students list.
- Payers list with search and pagination. Payer create/edit/view.

#### Milestone 6 — Students
- [ ] `pages/modules/students/Index.vue` — table (name, grade, class, email) with search input, grade/class filter selects, Pagination; CSV import button → `POST /students/import` (multipart) → toast with imported/skipped counts
- [ ] `pages/modules/students/Detail.vue` — create/edit form (name, email, phone, grade select, class select); save
- [ ] `pages/modules/students/View.vue` — student card + recent transactions list + assign-to-class button (uses `assignStudentToClass()`)

#### Milestone 7 — Payers
- [ ] `pages/modules/payer/Index.vue` — table (name, contact) with search + Pagination
- [ ] `pages/modules/payer/Detail.vue` — create/edit form (name, contact info); save
- [ ] `pages/modules/payer/View.vue` — payer card + transaction history list

#### Milestone 8 — Student Reports
- [ ] `pages/modules/student_reports/Index.vue` — filter bar (grade select, class select, DatePicker range); table of student transactions; uses `GET /students/report`

---

### Week 4 — Transactions

**Goal:** Accountants can record income and expenses with line items. Audit list visible.

**Deliverables:**
- Income (deposit) list with date filter and pagination.
- Income create/edit with dynamic line items (category, amount, description). Income view with reversal.
- Expense (payment) list, create/edit, view, and quick payment.
- All-transactions read-only list with type filter.

#### Milestone 9 — Income (Deposit)
- [ ] `pages/modules/accounting/deposit/Index.vue` — table (date, ref no, student/payer name, total) with DatePicker filter + Pagination
- [ ] `pages/modules/accounting/deposit/Detail.vue` — form: date, student or payer select, cash ledger select, dynamic line items (+ / − rows: category select + amount + description); save → `POST /transactions`
- [ ] `pages/modules/accounting/deposit/View.vue` — summary card + line items table; Reverse button → `POST /transactions/{id}/reverse` with AlertDialog confirmation

#### Milestone 10 — Expenses (Payment)
- [ ] `pages/modules/accounting/payment/Index.vue` — same pattern as deposit Index
- [ ] `pages/modules/accounting/payment/Detail.vue` — same pattern as deposit Detail (transaction type = expense)
- [ ] `pages/modules/accounting/payment/View.vue` — same pattern as deposit View
- [ ] `pages/modules/accounting/payment/QuickPayment.vue` — minimal form pre-filled with student data; single line item; fast save
- [ ] Add `reverseTransaction(id)` to `stores/api.ts` → `POST /transactions/{id}/reverse`

#### Milestone 11 — All Transactions
- [ ] `pages/modules/accounting/transactions/Index.vue` — combined list with type column (income/expense); filter by type + DatePicker range; read-only (no create button)

---

### Week 5 — Reports & Year-End

**Goal:** Finance staff can generate standard accounting reports and perform year-end close/open.

**Deliverables:**
- Reports index with links to each report type.
- Trial balance table with debit/credit totals.
- Profit & loss derived from income/expense ledger accounts.
- Balance sheet grouping asset/liability/equity accounts.
- Year-end close and open pages with confirmation dialogs.
- Dashboard fleshed out with live stats and charts.

#### Milestone 12 — API Store Extensions (Reports)
- [ ] Add `getTrialBalance(year?)` → `GET /ledgers/reports/trial-balance`
- [ ] Add `getLedgerSummary(year)` → `GET /ledgers/summary/{year}`
- [ ] Add `getJournalEntries(params)` → `GET /journal-entries`
- [ ] Add `yearEndClose(data)` → `POST /ledgers/year-end-close`
- [ ] Add `yearBeginningOpen(data)` → `POST /ledgers/year-beginning-open`

#### Milestone 13 — Reports Pages
- [ ] `pages/modules/accounting/reports/Index.vue` — grid of report cards (Trial Balance, Balance Sheet, Profit & Loss, Student Report)
- [ ] `pages/modules/accounting/reports/trial-balance/Index.vue` — year select; table of accounts with debit/credit columns and totals row; uses `getTrialBalance(year)`
- [ ] `pages/modules/accounting/reports/profit-loss/Index.vue` — year select; income vs expense ledger groups from `getLedgerSummary(year)`; net result row
- [ ] `pages/modules/accounting/reports/balance-sheet/Index.vue` — year select; asset / liability / equity groups from `getLedgerSummary(year)`

#### Milestone 14 — Year-End Pages
- [ ] `pages/modules/accounting/ledger/Close.vue` — year select + AlertDialog confirmation; calls `yearEndClose()`; reuse `YearEndClosing.vue` component where applicable; toast on success/error
- [ ] `pages/modules/accounting/ledger/Open.vue` — year select + AlertDialog confirmation; calls `yearBeginningOpen()`; toast on success/error

#### Milestone 15 — Dashboard & Polish
- [ ] Flesh out `pages/dashboard/Index.vue` — stat cards (total students, total payers, recent transactions, net balance); Chart1 or Chart2 for income vs expense trend
- [ ] Enable auth route guards globally in `router/routes.ts`
- [ ] Call `sidebar.setPageName('Title')` in `onMounted` on every page
- [ ] Add Skeleton loading states on all list pages (`components/ui/skeleton/`)
- [ ] Show error Alert (`components/ui/alert/`) on API failure instead of blank page

---

## Page Component Pattern

Every page follows the same minimal structure:

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { toast } from 'vue-sonner'
// import UI atoms from @/components/ui/...

const sidebar = useSidebarStore()
const route = useRoute()

onMounted(() => {
  sidebar.setPageName('Page Title')
  // fetch data
})
</script>

<template>
  <!-- Card, Table, Button, Dialog, Select, etc. from @/components/ui/ -->
  <!-- Pagination.vue, DatePicker.vue from @/components/templates/ -->
</template>
```

**Rules:**
- Always import atoms from `@/components/ui/` — never re-create existing components.
- Use `Pagination.vue` for every list page.
- Use `DatePicker.vue` for all date inputs.
- Toast via `vue-sonner` for success/error feedback on every write action.
- Use `AlertDialog` for destructive confirmations (delete, reverse transaction, year-end close).
- One `Detail.vue` per module for both create and edit — branch on `!!route.params.id`.
- Grades are never a standalone page — always a dropdown populated by `getGrades()`.

---

## Folder Structure (Pages — Target State)

```
src/pages/
├── dashboard/
│   └── Index.vue
├── auth/
│   ├── Login.vue               done
│   └── Register.vue            done
├── modules/
│   ├── students/
│   │   ├── Index.vue
│   │   ├── Detail.vue          # create + edit
│   │   └── View.vue
│   ├── student_reports/
│   │   └── Index.vue
│   ├── payer/
│   │   ├── Index.vue
│   │   ├── Detail.vue
│   │   └── View.vue
│   ├── classes/
│   │   ├── Index.vue
│   │   ├── Detail.vue
│   │   └── View.vue
│   └── accounting/
│       ├── main/
│       │   ├── View.vue
│       │   └── Detail.vue
│       ├── deposit/            # income/deposits
│       │   ├── Index.vue
│       │   ├── Detail.vue
│       │   └── View.vue
│       ├── payment/            # expenses/payments
│       │   ├── Index.vue
│       │   ├── Detail.vue
│       │   ├── View.vue
│       │   └── QuickPayment.vue
│       ├── transactions/
│       │   └── Index.vue
│       ├── ledger/
│       │   ├── Index.vue
│       │   ├── Detail.vue
│       │   ├── View.vue
│       │   ├── Close.vue
│       │   └── Open.vue
│       ├── category/
│       │   ├── Detail.vue
│       │   └── View.vue
│       └── reports/
│           ├── Index.vue
│           ├── balance-sheet/
│           │   └── Index.vue
│           ├── profit-loss/
│           │   └── Index.vue
│           └── trial-balance/
│               └── Index.vue
└── NotFound.vue                done
```

---

## Changelog

One file per week in [frontend/changelogs](frontend/changelogs). When a week's goals are fully achieved, create `YYYY-MM-DD_weekN.md`.

```
# YYYY-MM-DD_weekN
**Completed:** Week N — <week title>
## Changes
-
## Notes / blockers encountered
-
```
