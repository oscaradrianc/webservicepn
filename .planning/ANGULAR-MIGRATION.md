# Angular Migration Guide — API Contract Standardization (Phase 4)

**Purpose:** Documents every endpoint whose error response contract changed in Phase 4.
Success paths (HTTP 200 + Response<T>) are unchanged — no Angular changes needed for success paths.

**Format per endpoint:**
- Route: HTTP METHOD /api/Controller/route
- Before: HTTP status + response body shape
- After: HTTP status + response body shape
- Angular change: Code snippet or description of required update

---

## Infrastructure Changes (all endpoints)

### GlobalExceptionHandler — Active
Unhandled server exceptions now return HTTP 500 with ProblemDetails instead of a stack trace or plain text.

**Before:** HTTP 200 or HTTP 500 with plain-text body (inconsistent)
**After:** HTTP 500 + `{ "type": "...", "title": "Internal Server Error", "status": 500, "detail": "An unexpected error occurred.", "traceId": "..." }`

**Angular interceptor recommendation:**
```typescript
// In your HTTP interceptor:
if (error.status === 500) {
  // error.error is now { title, status, detail, traceId }
  console.error('Server error:', error.error?.detail);
}
```

### InvalidModelStateResponseFactory — HTTP 422 for validation errors
Requests with invalid bodies (missing required fields, failed DataAnnotations) now return HTTP 422 instead of HTTP 400.

**Before:** HTTP 400 (ASP.NET Core default) or HTTP 200 with embedded error message
**After:** HTTP 422 + `ValidationProblemDetails` with per-field errors

**Angular interceptor recommendation:**
```typescript
if (error.status === 422) {
  // error.error.errors is a map of field name → array of error messages
  const fieldErrors = error.error?.errors;
}
```

---

## LoginController (BREAKING CHANGE — D-07, D-08)

*Filled by Plan 04-03*

---

## SolicitudController

*Filled by Plan 04-04*

---

## ProveedorController

*Filled by Plan 04-04*

---

## CotizacionController

*Filled by Plan 04-05*

---

## PreguntasController

*Filled by Plan 04-05*

---

## Remaining Controllers (ApiControllerBase migration)

*Filled by Plan 04-07*

---

*Last updated: Phase 4, Plan 01 (infrastructure stub)*
