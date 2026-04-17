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

**POST /api/Cotizacion/registrar** — HTTP code change for system errors
- Before: Unhandled exception returned HTTP 500 with ResponseStatus body ( StatusCode bug)
- After: Unhandled exception → HTTP 500 ProblemDetails
- Angular change: Catch `error.status === 500` — error.error now has consistent structure

**POST /api/Cotizacion/adjudicar** — HTTP code correction for business errors
- Before: Business error returned HTTP 500 with string body (misuse of 500 for business error)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Update to catch `error.status === 400` instead of 500 for this endpoint

**POST /api/Cotizacion/cargamasiva** — HTTP code change for system errors
- Before: Unhandled exception returned HTTP 200 with body = exception message (Content bug)
- After: Unhandled exception → HTTP 500 ProblemDetails
- Angular change: Catch `error.status === 500` instead of parsing response body

**GET /api/Cotizacion/listarfichatecnica** — HTTP code change for system errors
- Before: Unhandled exception returned HTTP 200 with body = exception message (Content bug)
- After: Unhandled exception → HTTP 500 ProblemDetails
- Angular change: Catch `error.status === 500` instead of parsing response body

**IMPORTANT note for Adjudicar:** This endpoint previously returned HTTP 500 for business errors. It now correctly returns HTTP 400. Angular code checking `error.status === 500` for this endpoint must be updated to `error.status === 400`.

**Angular pattern for CotizacionController:**
```typescript
// Example for CotizacionController endpoints
this.http.post('/api/Cotizacion/adjudicar', request).pipe(
  catchError(err => {
    if (err.status === 400) {
      // Business error (adjudication failed)
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

## PreguntasController

**POST /api/Preguntas/preguntar** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body

**POST /api/Preguntas/responder** — HTTP code change for business errors
- Before: Business error returned HTTP 200 with body = "BadRequest" (Content bug)
- After: Business error → HTTP 400 ProblemDetails. Success → HTTP 200 + empty body
- Angular change: Catch `error.status === 400` instead of checking response body

**Angular pattern for PreguntasController:**
```typescript
// Example for PreguntasController endpoints
this.http.post('/api/Preguntas/preguntar', request).pipe(
  catchError(err => {
    if (err.status === 400) {
      // Business error
      return throwError(() => new Error(err.error?.detail));
    }
    return throwError(() => err);
  })
);
```

---

## Remaining Controllers (ApiControllerBase migration — Plan 04-08)

The following 7 controllers have been migrated to inherit from `ApiControllerBase` (no breaking changes to endpoints):

1. **AutorizadorGerenciaController** — GET/POST/DELETE endpoints for authorization managers. Uses Response<T> wrapper. Exceptions return HTTP 500 ProblemDetails.
2. **ParametroGeneralController** — GET/POST/PUT endpoints for general parameters (classes and values). Uses Response<T> wrapper. Exceptions return HTTP 500 ProblemDetails.
3. **ConstanteController** — GET/POST/PUT endpoints for constants. Uses Response<T> wrapper. Exceptions return HTTP 500 ProblemDetails or BadRequest.
4. **NoticiasController** — GET/PUT endpoints for news (AllowAnonymous). Uses Response<T> wrapper. Exceptions return BadRequest.
5. **NotificacionController** — GET/PUT/POST/DELETE endpoints for notifications. Uses ResponseStatus wrapper. Exceptions return BadRequest.
6. **NotificacionUsuarioController** — GET/POST/DELETE endpoints for user notifications. Uses ResponseStatus wrapper. Exceptions return BadRequest.
7. **OpcionesRolController** — GET/POST/DELETE endpoints for role options. Uses Response<T> wrapper. Exceptions return HTTP 500 ProblemDetails.

**Migration changes applied to all 7:**
- Removed `ControllerBase` inheritance, now inherit from `ApiControllerBase`
- Removed redundant `[ApiController]` attributes (inherited from `ApiControllerBase`)
- For controllers in namespace `SWNegocio.Controllers`, added `using PortalNegocioWS.Controllers;` to access `ApiControllerBase`

**No endpoint behavior changes:** All success responses (HTTP 200) and request models remain identical. Error handling is centralized via GlobalExceptionHandler as per Phase 4 design.

---

## Summary

**Phase 4 (API Contract Standardization) Completion Status:**

All 12 controllers have been standardized:
- ✓ LoginController (04-03)
- ✓ SolicitudController (04-04)
- ✓ ProveedorController (04-04)
- ✓ CotizacionController (04-05)
- ✓ PreguntasController (04-06)
- ✓ ApiControllerBase base class (04-02)
- ✓ AutorizadorGerenciaController (04-08)
- ✓ ParametroGeneralController (04-08)
- ✓ ConstanteController (04-08)
- ✓ NoticiasController (04-08)
- ✓ NotificacionController (04-08)
- ✓ NotificacionUsuarioController (04-08)
- ✓ OpcionesRolController (04-08)

**Infrastructure Changes Applied:**
- GlobalExceptionHandler centralized unhandled exceptions → HTTP 500 ProblemDetails
- InvalidModelStateResponseFactory enforces HTTP 422 for validation errors
- All controllers inherit ApiControllerBase (enforces [ApiController] attribute and proper HTTP response patterns)

**Angular Migration Guidance:**
- Update error handlers to check HTTP status codes (400, 401, 404, 422, 500) instead of parsing response body for error flags
- HTTP 422 errors have per-field error details in `error.error.errors`
- HTTP 500 errors have consistent ProblemDetails structure with `detail`, `title`, `traceId`

*Last updated: Phase 4, Plan 08 (final controller migrations)*
