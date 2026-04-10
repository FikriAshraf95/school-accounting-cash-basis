# Phase 2 — Accrual Cycle (Sales & Purchases)

## Context

Phase 1 implements a **cash-basis** accounting system: income and expense are recorded when money changes hands (`Transactions`). Phase 2 introduces an **accrual cycle** — documents are created when obligations arise (invoice issued, bill received), and cash movements are matched against those documents later.

This is additive. Phase 1 infrastructure (Transactions, JournalEntries, Ledgers, Payers, Students) remains unchanged.

---

## What Changes Conceptually

| Phase 1 (Cash) | Phase 2 (Accrual) |
|---|---|
| Record payment → journal entry immediately | Issue document → journal entry at creation; payment matched later |
| No document lifecycle | Document has status: `draft → sent → partially_paid → paid` |
| No outstanding balance tracking | AR aging / AP aging reports |
| No partial payment concept | Multiple payments against one document |

---

## Key Planning Questions (Resolve Before Starting)

1. **Partial payments?** A student paying 50% of an invoice now, 50% next month. Drives the `InvoicePayments` link-table design.
2. **Auto-create Transaction on payment?** When staff records a payment against an invoice, does the system auto-create a `Transaction` + journal entries, or does staff create the `Transaction` manually and then link it? — Auto-create is cleaner.
3. **Purchase Orders vs. Vendor Bills?** Full PO workflow (request → approve → receive goods → pay) or just vendor bills (receive bill → pay)? Full PO adds significant complexity.
4. **Tax handling?** `BusinessInfos.TaxRate` already exists. Do invoices carry tax line items, or is tax informational only?
5. **Reports scope?** AR aging, AP aging, cash-vs-accrual comparison — each needs its own query.

---

## New Vertical Slices

### `Features/Invoices/` — Sales / Accounts Receivable

**New tables:**

#### `Invoices`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| InvoiceNumber | nvarchar(50) | unique; generated e.g. `INV-YYYYMMDD-NNNN` |
| InvoiceDate | date | |
| DueDate | date | |
| Status | nvarchar(20) | `draft` / `sent` / `partially_paid` / `paid` / `overdue` / `cancelled` |
| CustomerType | nvarchar(50) | `Student` or `Payer` — polymorphic, same pattern as Transactions |
| CustomerId | int | polymorphic FK |
| Subtotal | decimal(15,2) | sum of line items before tax |
| TaxAmount | decimal(15,2) | |
| TotalAmount | decimal(15,2) | Subtotal + TaxAmount |
| PaidAmount | decimal(15,2) | updated when payments received |
| Notes | nvarchar(1000)? | |
| DeletedAt | datetime? | soft delete |
| CreatedAt / UpdatedAt | datetime | |

#### `InvoiceItems`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| InvoiceId | int FK → Invoices (cascade) | |
| CategoryId | int FK → Categories | |
| Description | nvarchar(500)? | |
| Quantity | int | default 1 |
| UnitPrice | decimal(15,2) | |
| Amount | decimal(15,2) | Quantity × UnitPrice |
| CreatedAt / UpdatedAt | datetime | |

#### `InvoicePayments`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| InvoiceId | int FK → Invoices | |
| TransactionId | int FK → Transactions | auto-created on payment |
| Amount | decimal(15,2) | amount applied to this invoice |
| PaymentDate | date | |
| CreatedAt | datetime | |

**Endpoints:**
```
GET    /api/v1/invoices                  paginated; ?status &customerId &customerType &dateFrom &dateTo
POST   /api/v1/invoices                  create invoice (status = draft)
GET    /api/v1/invoices/{id}             includes items + payment history
PUT    /api/v1/invoices/{id}             update — only if status = draft
DELETE /api/v1/invoices/{id}             soft delete — only if no payments
POST   /api/v1/invoices/{id}/send        change status draft → sent; triggers AR journal entry
POST   /api/v1/invoices/{id}/payments    record payment; auto-creates Transaction + journal entry
POST   /api/v1/invoices/{id}/cancel      cancel — reverses AR journal entry if already sent
```

**Double-entry flows:**

Invoice sent (`/send`):
- DEBIT `1100 Accounts Receivable`, CREDIT Revenue ledger (per category `LedgerId`)
- Invoice status → `sent`

Payment received (`/payments`):
- Auto-creates `Transaction` (income) for the payment amount
- DEBIT `1000 Cash / 1010 Cash in Bank`, CREDIT `1100 Accounts Receivable`
- Updates `Invoice.PaidAmount`; if fully paid → status = `paid`; else → `partially_paid`

Invoice cancelled:
- If status was `sent` or `partially_paid`: create reversing journal entries for the AR entry
- Status → `cancelled`

---

### `Features/Purchases/` — Purchases / Accounts Payable

**New tables:**

#### `PurchaseOrders`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| PONumber | nvarchar(50) | unique; generated e.g. `PO-YYYYMMDD-NNNN` |
| PODate | date | |
| DueDate | date? | |
| Status | nvarchar(20) | `draft` / `approved` / `partially_paid` / `paid` / `cancelled` |
| VendorId | int FK → Payers | Payer where Type = `vendor` / `supplier` |
| Subtotal | decimal(15,2) | |
| TaxAmount | decimal(15,2) | |
| TotalAmount | decimal(15,2) | |
| PaidAmount | decimal(15,2) | updated when payments made |
| Notes | nvarchar(1000)? | |
| DeletedAt | datetime? | soft delete |
| CreatedAt / UpdatedAt | datetime | |

#### `PurchaseItems`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| PurchaseOrderId | int FK → PurchaseOrders (cascade) | |
| CategoryId | int FK → Categories | |
| Description | nvarchar(500)? | |
| Quantity | int | default 1 |
| UnitPrice | decimal(15,2) | |
| Amount | decimal(15,2) | |
| CreatedAt / UpdatedAt | datetime | |

#### `PurchasePayments`
| Column | Type | Notes |
|---|---|---|
| Id | int PK | |
| PurchaseOrderId | int FK → PurchaseOrders | |
| TransactionId | int FK → Transactions | auto-created on payment |
| Amount | decimal(15,2) | |
| PaymentDate | date | |
| CreatedAt | datetime | |

**Endpoints:**
```
GET    /api/v1/purchases                 paginated; ?status &vendorId &dateFrom &dateTo
POST   /api/v1/purchases                 create purchase order (status = draft)
GET    /api/v1/purchases/{id}            includes items + payment history
PUT    /api/v1/purchases/{id}            update — only if status = draft
DELETE /api/v1/purchases/{id}            soft delete — only if no payments
POST   /api/v1/purchases/{id}/approve    change status draft → approved; triggers AP journal entry
POST   /api/v1/purchases/{id}/payments   record payment; auto-creates Transaction + journal entry
POST   /api/v1/purchases/{id}/cancel     cancel — reverses AP journal entry if already approved
```

**Double-entry flows:**

PO approved (`/approve`):
- DEBIT Expense ledger (per category `LedgerId`), CREDIT `2000 Accounts Payable`
- Status → `approved`

Payment made (`/payments`):
- Auto-creates `Transaction` (expense) for the payment amount
- DEBIT `2000 Accounts Payable`, CREDIT `1000 Cash / 1010 Cash in Bank`
- Updates `PurchaseOrder.PaidAmount`; if fully paid → status = `paid`; else → `partially_paid`

PO cancelled:
- If status was `approved` or `partially_paid`: create reversing journal entries
- Status → `cancelled`

---

## Chart of Accounts — Already Seeded

No new ledger accounts required. Phase 1 seed already includes:

| Code | Name | Used by |
|---|---|---|
| `1100` | Accounts Receivable | Invoice sent → AR entry |
| `2000` | Accounts Payable | PO approved → AP entry |

---

## RBAC

Extend the Phase 1 permission matrix:

| Feature / Action | Admin | Accountant | Staff | Viewer |
|---|---|---|---|---|
| **Invoices** GET | Y | Y | Y | Y |
| **Invoices** POST / PUT | Y | Y | Y | — |
| **Invoices** send / cancel | Y | Y | — | — |
| **Invoices** record payment | Y | Y | Y | — |
| **Invoices** DELETE | Y | Y | — | — |
| **Purchases** GET | Y | Y | Y | Y |
| **Purchases** POST / PUT | Y | Y | — | — |
| **Purchases** approve / cancel | Y | Y | — | — |
| **Purchases** record payment | Y | Y | — | — |
| **Purchases** DELETE | Y | Y | — | — |

---

## Reports (Future — `Features/Reports/`)

| Report | Description |
|---|---|
| AR Aging | Outstanding invoices grouped by days overdue (0–30, 31–60, 61–90, 90+) |
| AP Aging | Outstanding purchase orders grouped by days overdue |
| Revenue Accrual vs. Cash | Comparison of accrued revenue (invoices sent) vs. collected cash |

---

## Milestone Plan

### Milestone 9 — Sales / AR
- [ ] Create `Invoices`, `InvoiceItems`, `InvoicePayments` tables + EF migration
- [ ] `Features/Invoices/` — controller, service, DTOs, mappings
- [ ] Invoice lifecycle: create → send → payment → paid/partially_paid
- [ ] Auto-create `Transaction` + journal entries on `/send` and `/payments`
- [ ] Cancel flow with journal reversal
- [ ] Apply RBAC from permission matrix above

### Milestone 10 — Purchases / AP
- [ ] Create `PurchaseOrders`, `PurchaseItems`, `PurchasePayments` tables + EF migration
- [ ] `Features/Purchases/` — controller, service, DTOs, mappings
- [ ] PO lifecycle: create → approve → payment → paid/partially_paid
- [ ] Auto-create `Transaction` + journal entries on `/approve` and `/payments`
- [ ] Cancel flow with journal reversal
- [ ] Apply RBAC from permission matrix above

### Milestone 11 — AR/AP Reports
- [ ] AR aging report (`GET /api/v1/invoices/reports/aging`)
- [ ] AP aging report (`GET /api/v1/purchases/reports/aging`)
- [ ] Outstanding balances per customer/vendor
