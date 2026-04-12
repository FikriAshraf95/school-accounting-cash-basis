# School Accounting API Reference

## Overview

| Item | Detail |
|---|---|
| Base URL | `http://localhost:<port>/api/v1` |
| Auth | Bearer token (JWT) — `Authorization: Bearer <token>` |
| Format | JSON (`Content-Type: application/json`, `Accept: application/json`) |
| Interactive docs | `GET /scalar` — Scalar UI |
| OpenAPI spec | `GET /openapi/v1.json` |
| Rate limiting | Auth endpoints: 10 req/min per IP |
| CORS origins | `http://localhost:3000`, `http://127.0.0.1:3000` |

## Roles

| Role | Permissions |
|---|---|
| **Admin** | Full access |
| **Accountant** | Financial/accounting operations |
| **Staff** | General operations (students, classes) |
| **Viewer** | Read-only |

## Pagination

All list endpoints return:
```json
{
  "data": [ /* items */ ],
  "meta": {
    "total": 100,
    "page": 1,
    "perPage": 10,
    "lastPage": 7
  }
}
```

## Error Format
```json
{
  "type": "string",
  "title": "string",
  "status": 400,
  "detail": "string"
}
```

## HTTP Status Codes

| Code | Meaning |
|---|---|
| 200 | OK |
| 201 | Created |
| 204 | No Content (DELETE, logout, etc.) |
| 400 | Validation error |
| 401 | Missing/invalid token |
| 403 | Insufficient role |
| 404 | Not found |
| 429 | Rate limit exceeded |
| 500 | Server error |

---

## Frontend API Store

All calls go through `stores/api.ts` → `useAPI()` composable (`services/api.ts`). Bearer token is attached automatically from `stores/auth.ts`.

---

## 1. Authentication

No auth required. Rate limited to 10/min per IP.

### POST `/register`
**Body:**
```json
{
  "name": "string",
  "username": "string",
  "email": "string",
  "password": "string"
}
```
**Response 200:**
```json
{
  "token": "jwt-string",
  "user": {
    "id": 1,
    "name": "string",
    "username": "string",
    "email": "string",
    "role": "Admin",
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
}
```

### POST `/login`
**Body:**
```json
{
  "email": "string",
  "password": "string"
}
```
**Response 200:** Same as `/register`

### POST `/logout`
**Auth required.** Empty body. **Response 204.**

### GET `/user`
**Auth required.** Returns current authenticated user object.

---

## 2. Business Info

**Auth required for all. Admin/Accountant for write.**

### GET `/business-info`
**Response 200:**
```json
{
  "id": 1,
  "schoolName": "string",
  "registrationNumber": "string|null",
  "address": "string|null",
  "phone": "string|null",
  "email": "string|null",
  "financialYearStart": "date",
  "financialYearEnd": "date",
  "currency": "MYR",
  "timezone": "string",
  "bankName": "string|null",
  "bankAccountName": "string|null",
  "bankAccountNumber": "string|null",
  "taxRegistration": "string|null",
  "taxRate": 0.0,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### PUT `/business-info`
**Roles:** Admin, Accountant

**Body:** Same fields as response above (all optional except `schoolName`).

**Response 200:** Updated business info object.

---

## 3. Categories

**Auth required for all. Admin/Accountant for write.**

### GET `/categories`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `type` | string | `income` or `expense` |
| `isActive` | boolean | Filter active/inactive |

**Response 200 (paginated):**
```json
{
  "id": 1,
  "name": "string",
  "type": "income",
  "ledgerId": 1,
  "ledgerName": "string",
  "ledgerCode": "string",
  "description": "string|null",
  "requiresStudent": false,
  "isActive": true,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET `/categories/{id}`
Returns single category object.

### POST `/categories`
**Roles:** Admin, Accountant

**Body:**
```json
{
  "name": "string",
  "type": "income",
  "ledgerId": 1,
  "description": "string|null",
  "requiresStudent": false,
  "isActive": true
}
```
**Response 201:** Created category object.

### PUT `/categories/{id}`
**Roles:** Admin, Accountant

**Body:** Same as POST (type cannot be changed).

**Response 200:** Updated category object.

### DELETE `/categories/{id}`
**Roles:** Admin, Accountant. **Response 204.**

---

## 4. Classes

**Auth required for all. Admin/Staff for write. Admin only for delete.**

### GET `/classes`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `page` | int | Default: 1 |
| `perPage` | int | Default: 10 |
| `sortBy` | string | Field to sort by |
| `sortDesc` | boolean | Default: false |

**Response 200 (paginated):**
```json
{
  "id": 1,
  "name": "string",
  "code": "string",
  "gradeId": 1,
  "gradeName": "string",
  "section": "string|null",
  "description": "string|null",
  "capacity": 30,
  "feeAmount": 500.00,
  "isActive": true,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET `/classes/{id}`
Returns class object with enrolled students:
```json
{
  "id": 1,
  "name": "string",
  "code": "string",
  "gradeId": 1,
  "gradeName": "string",
  "section": "string|null",
  "description": "string|null",
  "capacity": 30,
  "feeAmount": 500.00,
  "isActive": true,
  "students": [
    {
      "id": 1,
      "studentId": "STU001",
      "name": "string",
      "email": "string|null",
      "phone": "string|null"
    }
  ],
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### POST `/classes`
**Roles:** Admin, Staff

**Body:**
```json
{
  "name": "string",
  "code": "string",
  "gradeId": 1,
  "section": "string|null",
  "description": "string|null",
  "capacity": 30,
  "feeAmount": 500.00,
  "isActive": true
}
```
**Response 201:** Created class object.

### PUT `/classes/{id}`
**Roles:** Admin, Staff. Same body as POST. **Response 200.**

### DELETE `/classes/{id}`
**Roles:** Admin only. **Response 204.**

### POST `/classes/{classId}/students`
**Roles:** Admin, Staff

**Body:**
```json
{ "studentId": 1 }
```
**Response 204.**

### DELETE `/classes/{classId}/students/{studentId}`
**Roles:** Admin, Staff. **Response 204.**

---

## 5. Grades

**Auth required for all. Admin/Staff for write. Admin only for delete.**

### GET `/grades`
**Query params:** `page`, `perPage`, `sortBy`, `sortDesc`

**Response 200 (paginated):**
```json
{
  "id": 1,
  "name": "string",
  "code": "string",
  "description": "string|null",
  "isActive": true,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET `/grades/{id}`
Returns single grade object.

### POST `/grades`
**Roles:** Admin, Staff

**Body:**
```json
{
  "name": "string",
  "code": "string",
  "description": "string|null",
  "isActive": true
}
```
**Response 201:** Created grade object.

### PUT `/grades/{id}`
**Roles:** Admin, Staff. Same body as POST. **Response 200.**

### DELETE `/grades/{id}`
**Roles:** Admin only. **Response 204.**

---

## 6. Ledgers

**Auth required for all. Admin/Accountant for write.**

### GET `/ledgers`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `page` | int | Default: 1 |
| `perPage` | int | Default: 10 |
| `sortBy` | string | Sort field |
| `sortDesc` | boolean | Default: false |
| `type` | string | `asset`\|`liability`\|`equity`\|`revenue`\|`expense` |
| `isActive` | boolean | Filter active/inactive |

**Response 200 (paginated):**
```json
{
  "id": 1,
  "code": "1001",
  "name": "string",
  "type": "asset",
  "category": "string|null",
  "balance": 10000.00,
  "isActive": true,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET `/ledgers/{id}`
Returns single ledger object.

### POST `/ledgers`
**Roles:** Admin, Accountant

**Body:**
```json
{
  "code": "1001",
  "name": "string",
  "type": "asset",
  "category": "string|null",
  "isActive": true
}
```
**Response 201:** Created ledger object.

### PUT `/ledgers/{id}`
**Roles:** Admin, Accountant

**Body:** `code`, `name`, `category`, `isActive` (type cannot be changed).

**Response 200:** Updated ledger object.

### DELETE `/ledgers/{id}`
**Roles:** Admin, Accountant. **Response 204.**

### GET `/ledgers/reports/trial-balance`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `year` | int | Optional — filter by fiscal year |

**Response 200:**
```json
{
  "items": [
    {
      "ledgerId": 1,
      "ledgerCode": "1001",
      "ledgerName": "string",
      "type": "asset",
      "debitAmount": 5000.00,
      "creditAmount": 0.00
    }
  ],
  "totalDebits": 50000.00,
  "totalCredits": 50000.00,
  "isBalanced": true
}
```

### GET `/ledgers/summary/{year}`
**Path param:** `year` (integer)

**Response 200:**
```json
{
  "year": 2025,
  "ledgers": [
    {
      "ledgerId": 1,
      "ledgerCode": "1001",
      "ledgerName": "string",
      "type": "asset",
      "openingBalance": 0.00,
      "totalDebits": 10000.00,
      "totalCredits": 5000.00,
      "netChange": 5000.00,
      "closingBalance": 5000.00
    }
  ],
  "totalAssets": 10000.00,
  "totalLiabilities": 2000.00,
  "totalEquity": 8000.00,
  "totalRevenue": 15000.00,
  "totalExpenses": 7000.00
}
```

### POST `/ledgers/year-end-close`
**Roles:** Admin, Accountant

**Body:**
```json
{ "year": 2025 }
```
**Response 200:**
```json
{
  "year": 2025,
  "netRevenue": 15000.00,
  "netExpense": 7000.00,
  "netProfit": 8000.00,
  "closingEntries": [
    {
      "ledgerCode": "4000",
      "ledgerName": "Revenue",
      "entryType": "debit",
      "amount": 15000.00
    }
  ],
  "closedAt": "datetime"
}
```

### POST `/ledgers/year-beginning-open`
**Roles:** Admin, Accountant

**Body:**
```json
{ "year": 2026 }
```
**Response 200:**
```json
{
  "year": 2026,
  "totalOpeningDebits": 10000.00,
  "totalOpeningCredits": 10000.00,
  "isBalanced": true,
  "openingEntries": [
    {
      "ledgerCode": "1001",
      "ledgerName": "Cash",
      "type": "asset",
      "entryType": "debit",
      "amount": 5000.00
    }
  ],
  "openedAt": "datetime"
}
```

---

## 7. Payers

**Auth required for all. Admin/Accountant/Staff for write. Admin/Accountant for delete.**

### GET `/payers`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `page` | int | Default: 1 |
| `perPage` | int | Default: 10 |
| `search` | string | Search by payer name |
| `type` | string | `donor`\|`sponsor`\|`vendor`\|`supplier`\|`general`\|`government` |
| `isActive` | boolean | Filter active/inactive |
| `sortBy` | string | Sort field |
| `sortDesc` | boolean | Default: false |

**Response 200 (paginated):**
```json
{
  "id": 1,
  "payerCode": "PAY001",
  "name": "string",
  "type": "donor",
  "category": "individual",
  "email": "string|null",
  "phone": "string|null",
  "address": "string|null",
  "balance": 0.00,
  "isRecurring": false,
  "notes": "string|null",
  "isActive": true,
  "deletedAt": null,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET `/payers/{id}`
Returns payer object with transaction history:
```json
{
  "...": "all payer fields",
  "transactionHistory": [
    {
      "id": 1,
      "transactionNumber": "TXN-001",
      "transactionDate": "datetime",
      "type": "income",
      "amount": 1000.00,
      "description": "string|null",
      "receiptNumber": "string|null",
      "isReversed": false,
      "createdAt": "datetime"
    }
  ]
}
```

### POST `/payers`
**Roles:** Admin, Accountant, Staff

**Body:**
```json
{
  "payerCode": "PAY001",
  "name": "string",
  "type": "donor",
  "category": "individual",
  "email": "string|null",
  "phone": "string|null",
  "address": "string|null",
  "isRecurring": false,
  "notes": "string|null",
  "isActive": true
}
```
**Type values:** `donor` | `sponsor` | `vendor` | `supplier` | `general` | `government`

**Category values:** `individual` | `corporate` | `government` | `ngo`

**Response 201:** Created payer object.

### PUT `/payers/{id}`
**Roles:** Admin, Accountant, Staff. Same body as POST. **Response 200.**

### DELETE `/payers/{id}`
**Roles:** Admin, Accountant. **Response 204.**

---

## 8. Students

**Auth required for all. Admin/Staff for write. Admin only for delete.**

### GET `/students`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `page` | int | Default: 1 |
| `perPage` | int | Default: 10 |
| `search` | string | Search by name |
| `classId` | int | Filter by class |
| `gradeId` | int | Filter by grade |
| `isActive` | boolean | Filter active/inactive |
| `sortBy` | string | Sort field |
| `sortDesc` | boolean | Default: false |

**Response 200 (paginated):**
```json
{
  "id": 1,
  "studentId": "STU001",
  "name": "string",
  "classId": 1,
  "className": "string|null",
  "gradeId": 1,
  "gradeName": "string|null",
  "email": "string|null",
  "phone": "string|null",
  "address": "string|null",
  "balance": 0.00,
  "isActive": true,
  "deletedAt": null,
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET `/students/{id}`
Returns student object with transaction history:
```json
{
  "...": "all student fields",
  "transactionHistory": [
    {
      "id": 1,
      "transactionNumber": "TXN-001",
      "transactionDate": "datetime",
      "type": "income",
      "amount": 500.00,
      "description": "string|null",
      "receiptNumber": "string|null",
      "isReversed": false,
      "createdAt": "datetime"
    }
  ]
}
```

### POST `/students`
**Roles:** Admin, Staff

**Body:**
```json
{
  "studentId": "STU001",
  "name": "string",
  "classId": 1,
  "gradeId": 1,
  "email": "string|null",
  "phone": "string|null",
  "address": "string|null",
  "isActive": true
}
```
**Response 201:** Created student object.

### PUT `/students/{id}`
**Roles:** Admin, Staff. Same body as POST. **Response 200.**

### DELETE `/students/{id}`
**Roles:** Admin only. **Response 204.**

### POST `/students/{id}/assign-class`
**Roles:** Admin, Staff

**Body:**
```json
{ "classId": 1 }
```
**Response 200:** Updated student object.

### GET `/students/report`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `gradeId` | int | Optional filter |
| `classId` | int | Optional filter |
| `dateFrom` | datetime | Optional date range start |
| `dateTo` | datetime | Optional date range end |

**Response 200 (array, no pagination):**
```json
[
  {
    "id": 1,
    "studentId": "STU001",
    "name": "string",
    "className": "string|null",
    "gradeName": "string|null",
    "balance": 500.00,
    "totalIncome": 1000.00,
    "totalExpense": 500.00,
    "transactionCount": 5
  }
]
```

### POST `/students/import`
**Roles:** Admin, Staff

**Content-Type:** `multipart/form-data`

**Body:** CSV file upload.

**CSV columns:** `StudentId`, `Name`, `ClassId` (optional), `GradeId` (optional), `Email` (optional), `Phone` (optional), `Address` (optional), `IsActive` (optional)

**Response 200:**
```json
{
  "imported": 45,
  "skipped": 2,
  "errors": ["Row 3: duplicate studentId"]
}
```

---

## 9. Transactions

**Auth required for all. Admin/Accountant/Staff for create/update. Admin/Accountant for delete/reverse.**

### GET `/transactions`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `page` | int | Default: 1 |
| `perPage` | int | Default: 10 |
| `type` | string | `income` or `expense` |
| `dateFrom` | datetime | Date range start |
| `dateTo` | datetime | Date range end |
| `transactableType` | string | `Student` or `Payer` |
| `transactableId` | int | Filter by student/payer ID |
| `sortBy` | string | Sort field |
| `sortDesc` | boolean | Default: false |

**Response 200 (paginated):**
```json
{
  "id": 1,
  "transactionNumber": "TXN-001",
  "transactionDate": "datetime",
  "type": "income",
  "transactableName": "string|null",
  "amount": 500.00,
  "paymentMethod": "cash|null",
  "description": "string|null",
  "isReversed": false,
  "createdAt": "datetime"
}
```

### GET `/transactions/{id}`
**Response 200 (full detail):**
```json
{
  "id": 1,
  "transactionNumber": "TXN-001",
  "transactionDate": "datetime",
  "type": "income",
  "transactableType": "Student",
  "studentId": 1,
  "studentName": "string|null",
  "payerId": null,
  "payerName": null,
  "amount": 500.00,
  "paymentMethod": "cash|null",
  "referenceNumber": "string|null",
  "description": "string|null",
  "receiptNumber": "string|null",
  "cashLedgerId": 1,
  "cashLedgerName": "string|null",
  "isReversed": false,
  "reversalTransactionId": null,
  "createdBy": 1,
  "createdByName": "string|null",
  "updatedBy": null,
  "updatedByName": null,
  "deletedAt": null,
  "createdAt": "datetime",
  "updatedAt": "datetime",
  "items": [
    {
      "id": 1,
      "categoryId": 1,
      "categoryName": "string|null",
      "amount": 500.00,
      "description": "string|null",
      "quantity": 1,
      "unitPrice": 500.00
    }
  ]
}
```

### POST `/transactions`
**Roles:** Admin, Accountant, Staff

**Body:**
```json
{
  "transactionDate": "datetime",
  "type": "income",
  "transactableType": "Student",
  "studentId": 1,
  "payerId": null,
  "paymentMethod": "cash",
  "referenceNumber": "string|null",
  "description": "string|null",
  "cashLedgerId": 1,
  "items": [
    {
      "categoryId": 1,
      "amount": 500.00,
      "description": "string|null",
      "quantity": 1,
      "unitPrice": 500.00
    }
  ]
}
```
**`type` values:** `income` | `expense`

**`transactableType` values:** `Student` | `Payer`

**Response 201:** Full transaction object.

### PUT `/transactions/{id}`
**Roles:** Admin, Accountant, Staff

**Body:** `transactionDate`, `paymentMethod`, `referenceNumber`, `description`, `items[]`

**Response 200:** Updated transaction object.

### DELETE `/transactions/{id}`
**Roles:** Admin, Accountant. **Response 204.**

### POST `/transactions/{id}/reverse`
**Roles:** Admin, Accountant

**Body:** Empty `{}`

**Response 200:** Full reversed transaction object (with `isReversed: true`).

---

## 10. Journal Entries

**Auth required. Read-only via API (entries created automatically on transactions).**

### GET `/journal-entries`
**Query params:**
| Param | Type | Description |
|---|---|---|
| `page` | int | Default: 1 |
| `perPage` | int | Default: 10 |
| `ledgerId` | int | Filter by ledger |
| `journalType` | string | Filter by journal type |
| `dateFrom` | datetime | Date range start |
| `dateTo` | datetime | Date range end |

**Response 200 (paginated):**
```json
{
  "id": 1,
  "transactionId": 1,
  "transactionNumber": "TXN-001",
  "ledgerId": 1,
  "ledgerCode": "1001",
  "ledgerName": "Cash",
  "entryType": "debit",
  "amount": 500.00,
  "entryDate": "datetime",
  "description": "string|null",
  "journalType": "string",
  "createdBy": 1,
  "createdByName": "string|null",
  "createdAt": "datetime"
}
```

---

## 11. Users

**Auth required. Admin role for all endpoints.**

### GET `/users`
**Query params:** `page`, `perPage`, `search` (username or email)

**Response 200 (paginated):**
```json
{
  "id": 1,
  "name": "string",
  "username": "string",
  "email": "string",
  "role": "Admin",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### GET `/users/{id}`
Returns single user object.

### POST `/users`
**Body:**
```json
{
  "name": "string",
  "username": "string",
  "email": "string",
  "password": "string",
  "role": "Admin"
}
```
**Role values:** `Admin` | `Accountant` | `Staff` | `Viewer`

**Response 201:** Created user object.

### PUT `/users/{id}`
**Body:** `name`, `username`, `email`

**Response 200:** Updated user object.

### PUT `/users/{id}/role`
**Body:**
```json
{ "role": "Accountant" }
```
**Response 200:** Updated user object.

### PUT `/users/{id}/password`
**Body:**
```json
{ "password": "newpassword" }
```
**Response 204.**

### DELETE `/users/{id}`
**Response 204.**

---

## Frontend Store Method Map

| `stores/api.ts` method | HTTP call |
|---|---|
| `register(data)` | POST `/register` |
| `login(data)` | POST `/login` |
| `logout()` | POST `/logout` |
| `getCurrentUser()` | GET `/user` |
| `getUsers(params)` | GET `/users` |
| `getUser(id)` | GET `/users/{id}` |
| `createUser(data)` | POST `/users` |
| `updateUser(id, data)` | PUT `/users/{id}` |
| `deleteUser(id)` | DELETE `/users/{id}` |
| `assignUserRole(id, data)` | PUT `/users/{id}/role` |
| `changeUserPassword(id, data)` | PUT `/users/{id}/password` |
| `getStudents(params)` | GET `/students` |
| `getStudent(id)` | GET `/students/{id}` |
| `createStudent(data)` | POST `/students` |
| `updateStudent(id, data)` | PUT `/students/{id}` |
| `deleteStudent(id)` | DELETE `/students/{id}` |
| `assignStudentToClass(id, data)` | POST `/students/{id}/assign-class` |
| `getStudentsReport(params)` | GET `/students/report` |
| `importStudents(formData)` | POST `/students/import` |
| `getGrades(params)` | GET `/grades` |
| `getGrade(id)` | GET `/grades/{id}` |
| `createGrade(data)` | POST `/grades` |
| `updateGrade(id, data)` | PUT `/grades/{id}` |
| `deleteGrade(id)` | DELETE `/grades/{id}` |
| `getClasses(params)` | GET `/classes` |
| `getClass(id)` | GET `/classes/{id}` |
| `createClass(data)` | POST `/classes` |
| `updateClass(id, data)` | PUT `/classes/{id}` |
| `deleteClass(id)` | DELETE `/classes/{id}` |
| `addStudentToClass(classId, data)` | POST `/classes/{classId}/students` |
| `removeStudentFromClass(classId, studentId)` | DELETE `/classes/{classId}/students/{studentId}` |
| `getTransactions(params)` | GET `/transactions` |
| `getTransaction(id)` | GET `/transactions/{id}` |
| `createTransaction(data)` | POST `/transactions` |
| `updateTransaction(id, data)` | PUT `/transactions/{id}` |
| `deleteTransaction(id)` | DELETE `/transactions/{id}` |
| `reverseTransaction(id)` | POST `/transactions/{id}/reverse` |
| `getCategories(params)` | GET `/categories` |
| `getCategory(id)` | GET `/categories/{id}` |
| `createCategory(data)` | POST `/categories` |
| `updateCategory(id, data)` | PUT `/categories/{id}` |
| `deleteCategory(id)` | DELETE `/categories/{id}` |
| `getLedgers(params)` | GET `/ledgers` |
| `getLedger(id)` | GET `/ledgers/{id}` |
| `createLedger(data)` | POST `/ledgers` |
| `updateLedger(id, data)` | PUT `/ledgers/{id}` |
| `deleteLedger(id)` | DELETE `/ledgers/{id}` |
| `getTrialBalance(params)` | GET `/ledgers/reports/trial-balance` |
| `getLedgerSummary(year)` | GET `/ledgers/summary/{year}` |
| `yearEndClose(data)` | POST `/ledgers/year-end-close` |
| `yearBeginningOpen(data)` | POST `/ledgers/year-beginning-open` |
| `getPayers(params)` | GET `/payers` |
| `getPayer(id)` | GET `/payers/{id}` |
| `createPayer(data)` | POST `/payers` |
| `updatePayer(id, data)` | PUT `/payers/{id}` |
| `deletePayer(id)` | DELETE `/payers/{id}` |
| `getJournalEntries(params)` | GET `/journal-entries` |
| `getBusinessInfo()` | GET `/business-info` |
| `updateBusinessInfo(data)` | PUT `/business-info` |
