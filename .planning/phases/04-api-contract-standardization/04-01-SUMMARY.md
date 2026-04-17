# Plan 04-01 Summary: Exception Handling Infrastructure

**Phase:** 04-api-contract-standardization
**Plan:** 01
**Status:** Completed
**Date:** 2026-04-17

## Objective

Create the exception-handling infrastructure for Phase 4: three custom exception types, the GlobalExceptionHandler that maps them to ProblemDetails responses, the ErrorHandlingInstaller that registers everything via the existing IInstaller pattern (including InvalidModelStateResponseFactory for HTTP 422), and the middleware pipeline call in Program.cs.

## Tasks Completed

### Task 1: Create exception types and GlobalExceptionHandler

**Files Created:**
- `PortalNegocioWS/Exceptions/BusinessException.cs`
- `PortalNegocioWS/Exceptions/NotFoundException.cs`
- `PortalNegocioWS/Exceptions/UnauthorizedException.cs`
- `PortalNegocioWS/Handlers/GlobalExceptionHandler.cs`

**Implementation:**
- Three custom exception types inheriting from `System.Exception`
- GlobalExceptionHandler implements `IExceptionHandler` from `Microsoft.AspNetCore.Diagnostics`
- Constructor-injects `ILogger<GlobalExceptionHandler>` and `IProblemDetailsService`
- Switch expression maps exception types to HTTP status codes:
  - `BusinessException` → HTTP 400 Bad Request
  - `NotFoundException` → HTTP 404 Not Found
  - `UnauthorizedException` → HTTP 401 Unauthorized
  - Any other exception → HTTP 500 Internal Server Error (generic message only)
- Logs full exception details via Serilog before returning response
- Uses `IProblemDetailsService.WriteAsync()` to respect configured JSON formatters

**Security Considerations:**
- HTTP 500 responses return generic "An unexpected error occurred." message
- Never exposes `exception.Message` for non-business exceptions (prevents information disclosure)
- Business exception messages are exposed (400-level, intentionally user-facing)
- TraceId added to all ProblemDetails responses via ErrorHandlingInstaller

**Commit:** `b3597e3`

---

### Task 2: Create ErrorHandlingInstaller and wire UseExceptionHandler in Program.cs

**Files Created/Modified:**
- `PortalNegocioWS/Installers/ErrorHandlingInstaller.cs` (new)
- `PortalNegocioWS/Program.cs` (modified)

**Implementation:**
- ErrorHandlingInstaller implements `IInstaller` (auto-discovered via reflection)
- Registers `AddProblemDetails()` with traceId extension
- Registers `AddExceptionHandler<GlobalExceptionHandler>()`
- Configures `InvalidModelStateResponseFactory` to return HTTP 422 instead of HTTP 400 for validation failures
- Uses `UnprocessableEntityObjectResult` with `ValidationProblemDetails` for field-level error details
- Added `app.UseExceptionHandler()` to middleware pipeline before `app.UseAuthentication()`

**Middleware Order:**
```
app.UseSerilogRequestLogging();
app.UseExceptionHandler();        // <-- NEW
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("OrigenLocal");
app.UseAuthentication();          // <-- Must come after exception handler
app.UseAuthorization();
app.MapControllers();
```

**Commit:** `0d3291a`

---

### Task 3: Create ANGULAR-MIGRATION.md stub

**Files Created:**
- `.planning/ANGULAR-MIGRATION.md`

**Content:**
- Purpose statement and format specification
- Infrastructure changes documented (GlobalExceptionHandler, HTTP 422 validation errors)
- TypeScript interceptor recommendations for handling 500 and 422 errors
- Placeholder sections for LoginController (marked as BREAKING CHANGE)
- Placeholder sections for SolicitudController, ProveedorController, CotizacionController, PreguntasController
- Placeholder section for remaining controllers (ApiControllerBase migration)
- Success-path invariance clearly stated (HTTP 200 + Response<T> unchanged)

**Commit:** `865f112`

---

## Requirements Satisfied

| Requirement | Status | Evidence |
|-------------|--------|----------|
| API-01 | Complete | GlobalExceptionHandler implements IExceptionHandler, returns ProblemDetails for all unhandled errors |
| API-02 | Complete | AddProblemDetails() and UseExceptionHandler() configured via ErrorHandlingInstaller and Program.cs |

## Key Decisions Applied

- **D-01:** Use ProblemDetails (RFC 7807) for all error responses — implemented via `AddProblemDetails()` and `IProblemDetailsService.WriteAsync()`
- **D-02:** GlobalExceptionHandler catches ALL unhandled exceptions and returns ProblemDetails automatically — no manual try/catch in controllers needed
- **D-03:** Custom business errors thrown as specific exception types — BusinessException, NotFoundException, UnauthorizedException implemented
- **D-04:** Proper RESTful HTTP codes for error paths — 400/401/404/422/500 mapped via switch expression
- **D-11:** Per-endpoint detail format — ANGULAR-MIGRATION.md template created with format specification
- **D-12:** Living document updated incrementally — stub created with placeholders for subsequent plans to fill

## Threat Mitigation

| Threat | Mitigation | Status |
|--------|-----------|--------|
| T-04-01: Information Disclosure in HTTP 500 detail | Generic "An unexpected error occurred." message for non-business exceptions | Mitigated |
| T-04-02: TraceId in response | TraceId is a correlation ID, not sensitive. RFC 7807 standard. | Accepted |
| T-04-03: Tampering via reflection auto-discovery | Existing project pattern used by all 8 installers | Accepted |
| T-04-04: DoS via exception logging | Serilog already active, bounded by request rate | Accepted |

## Verification Results

### Build Verification
```bash
dotnet build PortalNegocioWS.sln
```
**Result:** Success (0 errors, 12 warnings — all pre-existing)

### Structure Verification
```bash
grep -r "class GlobalExceptionHandler : IExceptionHandler" PortalNegocioWS/Handlers/
# ✅ Match found

grep -r "class ErrorHandlingInstaller : IInstaller" PortalNegocioWS/Installers/
# ✅ Match found

grep -n "InvalidModelStateResponseFactory" PortalNegocioWS/Installers/ErrorHandlingInstaller.cs
# ✅ Match found (line 26)

grep -n "UseExceptionHandler" PortalNegocioWS/Program.cs
# ✅ Match found (line 100)

grep -n "UseAuthentication" PortalNegocioWS/Program.cs
# ✅ Match found (line 112)
# ✅ UseExceptionHandler (100) < UseAuthentication (112) — correct order
```

### Angular Stub Verification
```bash
test -f .planning/ANGULAR-MIGRATION.md
# ✅ EXISTS

grep "GlobalExceptionHandler" .planning/ANGULAR-MIGRATION.md
# ✅ Match found

grep "LoginController" .planning/ANGULAR-MIGRATION.md
# ✅ Match found

grep "success paths" .planning/ANGULAR-MIGRATION.md
# ✅ Match found

grep "BREAKING CHANGE" .planning/ANGULAR-MIGRATION.md
# ✅ Match found

grep "422" .planning/ANGULAR-MIGRATION.md
# ✅ Match found
```

## Next Steps

This infrastructure plan (04-01) enables subsequent controller migration plans:

- **Plan 04-02:** Add DataAnnotations to critical request models
- **Plan 04-03:** Migrate LoginController to IActionResult (BREAKING CHANGE)
- **Plan 04-04:** Fix Content(HttpStatusCode...) bugs in SolicitudController and ProveedorController
- **Plan 04-05:** Fix Content(HttpStatusCode...) bugs in CotizacionController and PreguntasController
- **Plan 04-06:** Add DataAnnotations to remaining request models
- **Plan 04-07:** Create ApiControllerBase marker class and migrate remaining controllers
- **Plan 04-08:** Update ANGULAR-MIGRATION.md with complete endpoint mapping

## Lessons Learned

1. **IProblemDetailsService vs direct serialization:** Using `IProblemDetailsService.WriteAsync()` ensures ProblemDetails responses use the same JSON formatter (Newtonsoft.Json with PascalCase) as the rest of the API, maintaining consistency.

2. **Middleware ordering is critical:** `UseExceptionHandler()` must be placed before `UseAuthentication()` to catch exceptions thrown during authentication.

3. **HTTP 422 vs 400:** ASP.NET Core defaults to HTTP 400 for ModelState validation failures. Overriding `InvalidModelStateResponseFactory` to return HTTP 422 (Unprocessable Entity) follows RESTful best practices and signals validation errors distinct from bad request syntax errors.

4. **Exception message exposure:** Only business exceptions (400-level) expose their messages to clients. All other exceptions (500-level) return a generic message to prevent information disclosure. Full exception details are logged server-side via Serilog.

5. **Living documentation:** Creating ANGULAR-MIGRATION.md as a stub at the infrastructure stage allows subsequent plans to incrementally document each endpoint's contract changes, rather than attempting a massive documentation task at the end of the phase.

## Files Modified

```
PortalNegocioWS/
├── Exceptions/
│   ├── BusinessException.cs        (new)
│   ├── NotFoundException.cs        (new)
│   └── UnauthorizedException.cs    (new)
├── Handlers/
│   └── GlobalExceptionHandler.cs   (new)
├── Installers/
│   └── ErrorHandlingInstaller.cs   (new)
└── Program.cs                      (modified)

.planning/
└── ANGULAR-MIGRATION.md            (new)
```

---

**Summary:** All tasks completed successfully. Exception handling infrastructure is in place, with proper HTTP status code mapping, secure error messages, HTTP 422 validation support, and Angular migration guide stub ready for incremental updates.
