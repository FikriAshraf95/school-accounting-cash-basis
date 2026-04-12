# Frontend Code Audit Report

**Date:** 2026-04-12
**Scope:** `frontend/src/` — all pages, stores, services, router
**Reference:** `FEPROJECT.md`, `API.md`

---

## Summary

Overall quality is solid. Consistent patterns, error handling, loading states, and toast notifications are present throughout all pages. Five confirmed bugs found, one missing feature gap, and several minor code quality issues.

---

## BUGS (Confirmed)

### BUG 1 — `importStudents` breaks multipart upload

**File:** `frontend/src/stores/api.ts` lines 105–109
**Severity:** High — CSV import silently fails

```typescript
return await http.post('/students/import', data, undefined, {
  headers: { 'Content-Type': 'multipart/form-data' },  // ← WRONG
});
```

Manually setting `Content-Type: multipart/form-data` without the `boundary` parameter causes the server to reject the body — the boundary is what separates form fields in a multipart body. Axios automatically sets the correct header (including boundary) when it detects a `FormData` body. The entire `headers` config block should be removed.

**Fix:** Remove the config object argument:
```typescript
return await http.post('/students/import', data);
```

---

### BUG 2 — Classes search field is decorative only

**File:** `frontend/src/pages/modules/classes/Index.vue` lines 60, 77–80
**Severity:** High — broken UI feature

`searchQuery` is declared and bound to the `<Input>` in the template, but `fetchClasses()` never reads it — there is no `params.search = searchQuery.value` assignment. The search box does nothing when typed into.

**Fix:** Add to `fetchClasses()`:
```typescript
if (searchQuery.value) params.search = searchQuery.value
```
And wire the input to trigger a fetch (either `@keyup.enter` or a `watch`).

---

### BUG 3 — Ledger search field is decorative only

**File:** `frontend/src/pages/modules/accounting/ledger/Index.vue` lines 65, 90–98
**Severity:** High — broken UI feature

Same issue as BUG 2. `typeFilter` IS wired correctly (line 95) with a working `watch()`, but `searchQuery` is never passed to `getLedgers(params)`.

**Fix:** Add to `fetchLedgers()`:
```typescript
if (searchQuery.value) params.search = searchQuery.value
```
And add `searchQuery` to the existing `watch(typeFilter, ...)` or add a separate watcher.

---

### BUG 4 — Auth token logged to browser console

**File:** `frontend/src/stores/auth.ts` line 69
**Severity:** High — security / information disclosure

```typescript
console.log('response: ', response);  // logs full auth response including JWT token
```

This debug statement logs the full login response — including the JWT token — to the browser console in production. Any person with DevTools access (or a browser extension) can read the token.

**Fix:** Remove line 69.

---

### BUG 5 — `edit_main` route has spurious `:id` param

**File:** `frontend/src/router/routes.ts` line 173
**Severity:** Medium — edit business info page unreachable

```typescript
{ name: "edit_main", path: "main/edit/:id", ... }
```

`/business-info` is a singleton endpoint — there is no record ID. Any navigation to `{ name: 'edit_main' }` without an `:id` param will either fail to resolve or leave `:id` as a literal in the URL. The path should not have `:id`.

**Fix:**
```typescript
{ name: "edit_main", path: "main/edit", ... }
```

---

## MISSING FUNCTIONALITY

### M1 — No category list/index page or route

There are routes for `create_category`, `edit_category`, and `view_category`, but **no index route** for categories. Users have no way to browse, search, or navigate to categories from the UI. The `FEPROJECT.md` folder structure also omits `category/Index.vue`. This is a gap in the spec that should be resolved — either add the page or add a navigation path from another page (e.g., ledger view).

---

## CODE QUALITY

### Q1 — `isAuthenticated` getter uses unsafe OR logic

**File:** `frontend/src/stores/auth.ts` line 27

```typescript
isAuthenticated: (state) => !!state.token || !!state.user,
```

OR logic means a stale `user` object in `localStorage` can make the app think the user is authenticated even after the token is cleared (or vice versa). The 401 interceptor clears auth correctly, but the `||` here can mask edge cases.

**Recommendation:** Use `&&` or rely solely on `!!state.token` as the source of truth. `checkAuth()` already validates against the server.

---

### Q2 — Unused `watch` import in Classes Index

**File:** `frontend/src/pages/modules/classes/Index.vue` line 2

`watch` is imported from Vue but never used. (Compare: `ledger/Index.vue` correctly uses `watch` for `typeFilter`.) Once BUG 2 is fixed by adding a watcher on `searchQuery`, this import will be needed — but right now it is dead code.

---

### Q3 — Route guard function parameters typed as `any`

**File:** `frontend/src/router/routes.ts` lines 4, 15

```typescript
const requireAuth = async (to: any, from: any, next: any) => {
```

Should use Vue Router's built-in types for correctness and IDE support:

```typescript
import type { RouteLocationNormalized, NavigationGuardNext } from 'vue-router'

const requireAuth = async (
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) => {
```

---

### Q4 — All `stores/api.ts` write methods accept `data: any`

**File:** `frontend/src/stores/api.ts` — throughout

Every write method (`createStudent`, `createTransaction`, `createPayer`, etc.) accepts `data: any` and returns `any`. This defeats TypeScript's purpose for the most critical paths in the application. Consider adding even lightweight interfaces for the highest-risk methods (transactions, students) to catch API contract mismatches at compile time rather than at runtime.

---

### Q5 — Cancelled requests surface as toast errors

**File:** `frontend/src/services/api.ts` lines 130–134

When a request is aborted via `AbortController` (e.g., rapid navigation), the cancelled request propagates as an error to page `catch` blocks, which call `toast.error(...)`. The user sees a spurious error toast. Pages should guard against this:

```typescript
} catch (err: any) {
  if (axios.isCancel(err)) return  // add this guard
  error.value = err?.response?.data?.detail || 'Failed to load'
  toast.error('Error', { description: error.value })
}
```

---

## Priority Fix List

| Priority | Issue | File | Line(s) |
|---|---|---|---|
| **P1** | Multipart header breaks CSV import | `stores/api.ts` | 105–109 |
| **P1** | Auth token logged to console | `stores/auth.ts` | 69 |
| **P1** | Classes search non-functional | `modules/classes/Index.vue` | 60, 77–80 |
| **P1** | Ledger search non-functional | `modules/accounting/ledger/Index.vue` | 65, 90–98 |
| **P2** | `edit_main` route has wrong `:id` param | `router/routes.ts` | 173 |
| **P2** | No category index/list page | — | — |
| **P3** | `isAuthenticated` OR logic | `stores/auth.ts` | 27 |
| **P3** | Unused `watch` import | `modules/classes/Index.vue` | 2 |
| **P3** | Route guard `any` types | `router/routes.ts` | 4, 15 |
| **P3** | `data: any` on all store write methods | `stores/api.ts` | throughout |
| **P3** | Cancelled requests show error toasts | all list pages | catch blocks |
