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

**POST /api/Login/authenticate** — Breaking Change
- Before: Always HTTP 200. Authentication failures returned `{ "status": { "Status": "ERROR", "Message": "BadRequest" }, "data": { ..., "ResultadoLogin": -2 } }`
- After: Wrong credentials or inactive user → HTTP 401 (empty body or `{}`). Success → HTTP 200 + `{ "status": { "Status": "OK", "Message": "" }, "data": { ..., "ResultadoLogin": 1 } }`. "Must change password" (code 2) and "Expired password" (code 3) remain HTTP 200.
- Angular change: Update error handling in the login service to check for 401 instead of checking `response.status.Status === 'ERROR'`

**POST /api/Login/changepassword** — HTTP code change for errors
- Before: Always HTTP 200. Errors returned `{ "Status": "ERROR", "Message": "..." }`
- After: Business error → HTTP 400 ProblemDetails `{ "title": "Bad Request", "status": 400, "detail": "error message", "traceId": "..." }`. Success → HTTP 200 + `{ "Status": "OK", "Message": "" }`
- Angular change: Catch `error.status === 400` instead of checking `response.Status === 'ERROR'`

**POST /api/Login/resetpassword** — Same as changepassword

Include Angular interceptor snippet showing how to handle 401 from authenticate:
```typescript
// In Angular login service (example):
login(credentials: LoginRequest): Observable<Response<Usuario>> {
  return this.http.post<Response<Usuario>>('/api/Login/authenticate', credentials).pipe(
    catchError(err => {
      if (err.status === 401) {
        // Was: checking err.error?.data?.ResultadoLogin === -2
        // Now: HTTP 401 means bad credentials or inactive user
        return throwError(() => new Error('Credenciales incorrectas'));
      }
      return throwError(() => err);
    })
  );
}
```

Note for ResultadoLogin codes 2 and 3: Angular code that reads `response.data.ResultadoLogin` to redirect to password-change flow continues to work unchanged (still HTTP 200).

---

## SolicitudController

**POST /api/Solicitud/registrar** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails `{ "title": "Bad Request", "status": 400, "detail": "error message", "traceId": "..." }`. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body for "BadRequest"

**POST /api/Solicitud/actualizar** — HTTP code changes for business and system errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug). Unhandled exceptions returned HTTP 200 with body = exception message
- After: Business error → HTTP 400 ProblemDetails. Unhandled exceptions → HTTP 500 ProblemDetails
- Angular change: Catch both 400 and 500 errors instead of parsing response body

**POST /api/Solicitud/Autorizar** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body

**POST /api/Solicitud/actualizarfechas** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body

**POST /api/Solicitud/cargamasiva** — HTTP code change for system errors
- Before: Unhandled exception returned HTTP 200 with body = exception message
- After: Unhandled exception → HTTP 500 ProblemDetails
- Angular change: Catch `error.status === 500` instead of parsing response body

**Angular pattern for business and system errors:**
```typescript
// Example for SolicitudController endpoints
this.http.post('/api/Solicitud/registrar', request).pipe(
  catchError(err => {
    if (err.status === 400) {
      // Business error: err.error is { title: "Bad Request", status: 400, detail: "error message", traceId: "..." }
      return throwError(() => new Error(err.error?.detail));
    }
    if (err.status === 500) {
      // System error: err.error is { title: "Internal Server Error", status: 500, detail: "An unexpected error occurred.", traceId: "..." }
      return throwError(() => new Error('Error del servidor'));
    }
    return throwError(() => err);
  })
);
```

## ProveedorController

**POST /api/Proveedor/registrar** — HTTP code change for system errors
- Before: Unhandled exception returned HTTP 200 with body = exception message (Content bug)
- After: Unhandled exception → HTTP 500 ProblemDetails
- Angular change: Catch `error.status === 500` instead of parsing response body

**POST /api/Proveedor/autorizar** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body

**POST /api/Proveedor/actualizarestado** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body

**POST /api/Proveedor/actualizardocs** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body

**GET /api/Proveedor/proveedorporestado** — HTTP code change for system errors
- Before: Unhandled exception returned HTTP 500 with plain text body ( StatusCode bug)
- After: Unhandled exception → HTTP 500 ProblemDetails
- Angular change: Catch `error.status === 500` — error.error now has consistent structure

**GET /api/Proveedor/proveedorpormes** — HTTP code change for system errors
- Before: Unhandled exception returned HTTP 500 with plain text body ( StatusCode bug)
- After: Unhandled exception → HTTP 500 ProblemDetails
- Angular change: Catch `error.status === 500` — error.error now has consistent structure

**Angular pattern for ProveedorController:**
```typescript
// Example for ProveedorController endpoints
this.http.post('/api/Proveedor/autorizar', request).pipe(
  catchError(err => {
    if (err.status === 400) {
      // Business error
      return throwError(() => new Error(err.error?.detail));
    }
    if (err.status === 500) {
      // System error
      return throwError(() => new Error('Error del servidor'));
    }
    return throwError(() => err);
  })
);
```

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
