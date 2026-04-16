# Phase 4: API Contract Standardization - Research

**Researched:** 2026-04-16
**Domain:** ASP.NET Core 9.0 error handling, ProblemDetails (RFC 7807), DataAnnotations validation, HTTP status codes
**Confidence:** HIGH

## Summary

This phase transforms the API's error handling from an ad-hoc pattern of `Response<T>` wrappers with HTTP 200 for all responses into a proper RESTful contract where errors use correct HTTP status codes (400/401/404/422/500) with RFC 7807 ProblemDetails payloads, while preserving the `Response<T>` envelope for all success paths. The project currently has 19 controllers, all already decorated with `[ApiController]`, which enables automatic ModelState validation (HTTP 400 with ValidationProblemDetails for invalid models).

The current codebase has a critical bug: `Content(HttpStatusCode.BadRequest.ToString(), result)` does NOT set HTTP 400 -- the `Content(string, string)` overload treats the first parameter as the response body and the second as the media type, returning HTTP 200. This means 15+ endpoints across SolicitudController, ProveedorController, CotizacionController, and PreguntasController are returning HTTP 200 with error messages in the body instead of proper error status codes.

LoginController is the highest-risk migration because it returns `Response<T>` directly (not `IActionResult`), meaning ASP.NET Core always serializes it as HTTP 200. The business layer returns authentication failures embedded in the `Response<Usuario>` body via `ResultadoLogin` codes (-2 = wrong credentials, -1 = inactive user, 2 = must change password, 3 = expired password). Migrating this to `IActionResult` is a breaking change for the Angular client and must be documented in the migration guide.

**Primary recommendation:** Implement the GlobalExceptionHandler + custom exception types first (infrastructure), then migrate controllers in order of risk: LoginController first (breaking change), then the 6 controllers using `Content(HttpStatusCode..., ...)`, then the remaining 12 controllers. Each controller migration updates ANGULAR-MIGRATION.md incrementally.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Use ProblemDetails (RFC 7807) for all error responses -- `{ type, title, status, detail, traceId }`. Built into ASP.NET Core via `AddProblemDetails()`.
- **D-02:** Implement a GlobalExceptionHandler that catches ALL unhandled exceptions and returns ProblemDetails automatically. Controllers throw exceptions for error cases -- no more manual try/catch boilerplate in every method.
- **D-03:** Custom business errors thrown as specific exception types (e.g., `BusinessException`, `NotFoundException`) that the GlobalExceptionHandler maps to appropriate HTTP codes and ProblemDetails fields.
- **D-04:** Proper RESTful HTTP codes for error paths: 400 (bad request), 401 (unauthorized), 404 (not found), 422 (validation), 500 (server error).
- **D-05:** Success paths remain HTTP 200 + `Response<T>` envelope -- Angular's success path contracts do NOT change.
- **D-06:** Angular migration guide documents every endpoint whose error response changes from 200-with-error-body to proper HTTP error code + ProblemDetails.
- **D-07:** Change LoginController return type from `Response<Usuario>` to `IActionResult` in Phase 4. Success -> `Ok(Response<Usuario>)`. Authentication failure -> `Unauthorized()` or `BadRequest()`.
- **D-08:** Update all callers and the Angular migration guide to cover this breaking change. The guide entry for LoginController must show old vs new response handling code.
- **D-09:** Add DataAnnotations (`[Required]`, `[MaxLength]`, `[Range]`) to critical request models. `[ApiController]` attribute (already on all 19 controllers) enables automatic ModelState validation -> 422 with ProblemDetails.
- **D-10:** FluentValidation deferred to v2 per PROJECT.md. DataAnnotations-only for this phase.
- **D-11:** Per-endpoint detail format -- each endpoint gets its own entry showing old vs new response format, HTTP code changes, and Angular code snippets for updating the client.
- **D-12:** Guide lives at `.planning/ANGULAR-MIGRATION.md` and is a living document updated incrementally with each plan in this phase.

### Claude's Discretion
- Which specific request models get DataAnnotations (prioritize critical paths: login, solicitudes, cotizaciones)
- Exception type hierarchy design (BusinessException, NotFoundException, etc.)
- GlobalExceptionHandler implementation details
- ProblemDetails extension fields (traceId, instance, etc.)

### Deferred Ideas (OUT OF SCOPE)
- FluentValidation for complex validation rules -- deferred to v2
- Rate limiting / throttling -- not in scope
- API versioning -- future consideration
- Swagger/OpenAPI documentation generation -- future consideration
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| API-01 | GlobalExceptionHandler implementing IExceptionHandler added to pipeline -- unhandled errors return ProblemDetails instead of stack traces | Section: Architecture Patterns, Pattern 1 + Pattern 2 |
| API-02 | `AddProblemDetails()` and `UseExceptionHandler()` configured in `Program.cs` | Section: Architecture Patterns, Pattern 1 + Code Examples |
| API-03 | Base class `ApiControllerBase : ControllerBase` with helpers `BusinessError()` / `NotFound()` / `Unauthorized()` for standardizing error responses | Section: Architecture Patterns, Pattern 3 |
| API-04 | LoginController refactored to return `IActionResult` (currently returns `Response<T>` directly -- cannot return 401) | Section: Common Pitfalls, Pitfall 2 + LoginController analysis |
| API-05 | At least 3 controllers with correct HTTP codes (200/400/401/404/500) instead of `Content(HttpStatusCode.BadRequest.ToString(), e.Message)` | Section: Current State Inventory + Pattern 3 |
| API-06 | All remaining controllers migrated to `ApiControllerBase` with correct HTTP codes | Section: Current State Inventory |
| API-07 | DataAnnotations (`[Required]`, `[MaxLength]`, `[Range]`) added to most critical request models | Section: DataAnnotations Targets |
| API-08 | `[ApiController]` on all controllers for automatic ModelState validation | Already done -- all 19 controllers have `[ApiController]` |
| API-09 | Angular migration guide generated documenting each changed endpoint | Section: Angular Migration Guide approach |
</phase_requirements>

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Global exception handling | API / Backend | -- | Middleware runs in the ASP.NET Core request pipeline, catching all unhandled exceptions before they reach the client |
| HTTP status code mapping | API / Backend | -- | Controller layer owns setting the correct HTTP status code per error type |
| Input validation (DataAnnotations) | API / Backend | -- | `[ApiController]` auto-validation runs in the model binding phase, before controller action executes |
| ProblemDetails serialization | API / Backend | -- | ASP.NET Core built-in ProblemDetails middleware formats RFC 7807 responses |
| Angular migration documentation | Static docs | -- | Markdown file in `.planning/` directory -- no runtime dependency |
| Custom exception types | API / Backend (Business layer) | -- | Business services throw typed exceptions; controllers don't need to catch them (GlobalExceptionHandler handles it) |

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| ASP.NET Core ProblemDetails | Built-in (net9.0) | RFC 7807 error responses | Native to ASP.NET Core since .NET 8 -- `AddProblemDetails()`, `IProblemDetailsService`, `ProblemDetailsContext` [VERIFIED: Context7 /dotnet/aspnetcore.docs] |
| IExceptionHandler | Built-in (net9.0) | Centralized exception handling | Native interface since .NET 8 -- `TryHandleAsync()` method, registered via `AddExceptionHandler<T>()` [VERIFIED: Context7 /dotnet/aspnetcore.docs] |
| System.ComponentModel.DataAnnotations | Built-in (net9.0) | `[Required]`, `[MaxLength]`, `[Range]` validation | Native to .NET, auto-validated by `[ApiController]` attribute [VERIFIED: Context7 /dotnet/aspnetcore.docs] |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 9.0.9 (installed) | JSON serialization via Newtonsoft | Already installed and configured -- `AddNewtonsoftJson()` in Program.cs. ProblemDetails responses are serialized through the configured formatter pipeline [VERIFIED: Directory.Packages.props + Program.cs] |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| (none needed) | -- | -- | All required infrastructure is built into ASP.NET Core 9.0 |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| IExceptionHandler | UseExceptionHandler lambda | Lambda is simpler but less testable and can't be composed. IExceptionHandler supports DI, multiple handlers, and unit testing [CITED: learn.microsoft.com/aspnet/core/fundamentals/error-handling] |
| ProblemDetails | Custom error JSON | ProblemDetails is RFC 7807 standard, auto-configured by ASP.NET Core, and Angular has standard interceptors for it [VERIFIED: Context7] |
| DataAnnotations | FluentValidation | FluentValidation deferred to v2 per D-10. DataAnnotations sufficient for basic required/length/range validation [ASSUMED: per CONTEXT.md D-10] |

**Installation:**
```bash
# No new packages needed -- all functionality is built into ASP.NET Core 9.0
# The project already has Microsoft.AspNetCore.Mvc.NewtonsoftJson 9.0.9
```

## Architecture Patterns

### System Architecture Diagram

```
Client Request (Angular)
       |
       v
+-------------------+
| UseExceptionHandler|  <-- catches ALL unhandled exceptions
|  (middleware)       |
+-------------------+
       |
       v
+-------------------+
| IExceptionHandler  |  <-- GlobalExceptionHandler.cs
| (registered via    |      maps exception type to HTTP code + ProblemDetails
|  DI, singleton)    |
+-------------------+
       |
       v
+-------------------+
| [ApiController]    |  <-- auto-validates ModelState
| ModelState filter  |      invalid model -> HTTP 400 ValidationProblemDetails
+-------------------+      (skip controller action entirely)
       |
       v
+-------------------+
| Controller Action  |  <-- success: Ok(Response<T>) -> HTTP 200 + Response<T>
|                    |      business error: throw exception -> caught by handler
+-------------------+
       |
       v
  Response to Client
  Success: { status: { status: "OK", message: "" }, data: T }     (HTTP 200)
  Error:   { type: "...", title: "...", status: 400, detail: "...", traceId: "..." }  (HTTP 4xx/5xx)
```

### Recommended Project Structure

```
PortalNegocioWS/
├── Controllers/
│   ├── ApiControllerBase.cs          # NEW: base class with error helper methods
│   └── [all 19 controllers].cs       # migrated to throw exceptions instead of try/catch
├── Exceptions/                        # NEW directory
│   ├── BusinessException.cs           # maps to HTTP 400
│   ├── NotFoundException.cs           # maps to HTTP 404
│   ├── UnauthorizedException.cs       # maps to HTTP 401
│   └── ValidationException.cs         # maps to HTTP 422
├── Handlers/                          # NEW directory
│   └── GlobalExceptionHandler.cs      # implements IExceptionHandler
├── Installers/
│   └── ErrorHandlingInstaller.cs      # NEW: registers ProblemDetails + exception handler
└── Program.cs                         # updated: UseExceptionHandler() call

Negocio.Model/
├── Login/
│   └── LoginRequest.cs                # DataAnnotations added
├── ChangePasswordRequest.cs           # DataAnnotations added
├── Solicitud/
│   ├── SolicitudCompra.cs             # DataAnnotations added (critical)
│   └── Autorizacion.cs               # DataAnnotations added
├── Cotizacion/
│   └── Cotizacion.cs                  # DataAnnotations added (critical)
└── Usuario/
    └── CambioClave.cs                 # DataAnnotations added

.planning/
└── ANGULAR-MIGRATION.md               # living document, updated per plan
```

### Pattern 1: GlobalExceptionHandler via IExceptionHandler

**What:** Centralized exception-to-ProblemDetails mapping. Controllers throw typed exceptions; the handler catches them and returns appropriate HTTP responses.

**When to use:** All controllers in the project. Eliminates boilerplate try/catch blocks.

**Example:**
```csharp
// Source: Context7 /dotnet/aspnetcore.docs -- IExceptionHandler pattern
// PortalNegocioWS/Handlers/GlobalExceptionHandler.cs

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            NotFoundException ex => _problemDetailsFactory.CreateProblemDetails(
                httpContext, StatusCodes.Status404NotFound, "Not Found",
                detail: ex.Message),
            UnauthorizedException ex => _problemDetailsFactory.CreateProblemDetails(
                httpContext, StatusCodes.Status401Unauthorized, "Unauthorized",
                detail: ex.Message),
            BusinessException ex => _problemDetailsFactory.CreateProblemDetails(
                httpContext, StatusCodes.Status400BadRequest, "Bad Request",
                detail: ex.Message),
            _ => _problemDetailsFactory.CreateProblemDetails(
                httpContext, StatusCodes.Status500InternalServerError, "Internal Server Error",
                detail: "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // exception handled, stop propagation
    }
}
```

### Pattern 2: Program.cs Registration

**What:** Wire up ProblemDetails and exception handler in the middleware pipeline.

**Example:**
```csharp
// In ErrorHandlingInstaller.cs (follows IInstaller pattern)
public class ErrorHandlingInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                // Add traceId to all ProblemDetails responses
                ctx.ProblemDetails.Extensions["traceId"] =
                    ctx.HttpContext.TraceIdentifier;
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
}

// In Program.cs, AFTER builder.Build() and BEFORE app.MapControllers():
app.UseExceptionHandler();
```

### Pattern 3: ApiControllerBase with Helper Methods

**What:** Base controller class that provides consistent error handling. Controllers inherit from this instead of directly from `ControllerBase`.

**When to use:** All 19 controllers inherit from this base.

**Example:**
```csharp
// PortalNegocioWS/Controllers/ApiControllerBase.cs

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    // No helper methods needed -- controllers throw typed exceptions
    // and GlobalExceptionHandler handles the response.

    // However, if needed for explicit cases (like LoginController migration):
    protected ObjectResult BusinessError(string message)
        => BadRequest(new ProblemDetails
        {
            Status = 400,
            Title = "Bad Request",
            Detail = message
        });
}
```

**Important:** Per D-02, the preferred approach is to throw exceptions, not call helper methods. The base class may be minimal or just a marker. Controllers should throw `BusinessException`, `NotFoundException`, etc. The GlobalExceptionHandler does the mapping.

### Anti-Patterns to Avoid

- **Anti-pattern: `Content(HttpStatusCode.BadRequest.ToString(), message)` -- THIS IS A BUG.** `Content(string, string)` returns HTTP 200 with the first argument as the body and the second as the content-type header. It does NOT set the HTTP status code. The current codebase has 15+ instances of this bug. Replace with throwing `BusinessException(message)` or returning `BadRequest()`.
- **Anti-pattern: Try/catch in every controller method.** Per D-02, controllers should throw exceptions. The GlobalExceptionHandler catches them centrally.
- **Anti-pattern: Returning `Response<T>` with `Status = "ERROR"` and HTTP 200.** Error paths must return proper HTTP status codes. Only success paths use `Response<T>` with HTTP 200.
- **Anti-pattern: Wrapping ProblemDetails inside `Response<T>`.** ProblemDetails is a separate format. Error responses are ProblemDetails; success responses are `Response<T>`. Never mix them.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Error response format | Custom error JSON envelope | ASP.NET Core `AddProblemDetails()` + `ProblemDetails` class | RFC 7807 standard, auto-serialized, includes traceId, client libraries understand it |
| Exception handling middleware | Custom middleware with try/catch | `IExceptionHandler` + `UseExceptionHandler()` | Built-in since .NET 8, supports DI, ordered execution, testable |
| Validation error formatting | Manual ModelState iteration | `[ApiController]` auto-validation -> `ValidationProblemDetails` | Automatic HTTP 400/422 with field-level error details |
| HTTP status code mapping | Manual switch/if in every controller | Typed exceptions + GlobalExceptionHandler switch expression | Single place to maintain the mapping, consistent across all controllers |

**Key insight:** ASP.NET Core 9 provides all the infrastructure needed. No new NuGet packages are required. The entire error handling transformation uses built-in APIs: `IExceptionHandler`, `ProblemDetails`, `AddProblemDetails()`, `UseExceptionHandler()`, and DataAnnotations.

## Common Pitfalls

### Pitfall 1: `Content(HttpStatusCode.BadRequest.ToString(), message)` Returns HTTP 200

**What goes wrong:** The `Content(string, string)` overload of `ControllerBase.Content()` treats the first parameter as the response body and the second as the content-type. `Content("BadRequest", "error message")` returns HTTP 200 with body `"BadRequest"` and content type `"error message"`. The actual HTTP status code is never set.

**Why it happens:** This is a legacy pattern from ASP.NET Web API 2 where `Content(HttpStatusCode, string)` existed. In ASP.NET Core, the overloads are different. The code was likely ported without updating the calls.

**How to avoid:** Replace all `Content(HttpStatusCode.xxx.ToString(), ...)` calls with either:
- Throw a typed exception (e.g., `throw new BusinessException(message)`) for the GlobalExceptionHandler to catch
- Or use `BadRequest()`, `NotFound()`, `StatusCode(500, ...)` for cases where you need explicit control

**Warning signs:** Any `Content(` call that passes `HttpStatusCode.*` as the first argument.

**Current occurrence count:** 15 instances across 4 controllers:
- SolicitudController: 7 instances
- ProveedorController: 5 instances
- CotizacionController: 2 instances
- PreguntasController: 2 instances (using `Content` with `BadRequest` as content-type -- different but same bug pattern)

[VERIFIED: grep audit of all controllers]

### Pitfall 2: LoginController Cannot Return Non-200 Status Codes

**What goes wrong:** LoginController.Authenticate() returns `Response<Usuario>` directly, not `IActionResult`. ASP.NET Core serializes the return value as HTTP 200 regardless of content. Authentication failures (wrong password, inactive user) are communicated via `ResultadoLogin` codes in the response body.

**Why it happens:** The method signature `public Response<Usuario> Authenticate(...)` doesn't allow setting HTTP status codes. Only `IActionResult` return types give access to `Unauthorized()`, `BadRequest()`, etc.

**How to avoid:** Change to `IActionResult` return type (per D-07). This is a breaking change that requires Angular client updates -- documented in migration guide.

**Warning signs:** Any controller method returning `Response<T>` or `ResponseStatus` directly instead of `IActionResult`.

**Current scope:** 5 methods return `Response<T>` or `ResponseStatus` directly:
- LoginController: `Authenticate` (returns `Response<Usuario>`), `ChangePassword` (returns `ResponseStatus`), `ResetPassword` (returns `ResponseStatus`)
- UsuarioController: `CambiarClaveUsuario` (returns `ResponseStatus`)
- But only LoginController is in scope per D-07/D-08

[VERIFIED: grep audit of return types]

### Pitfall 3: ProblemDetails Serialization with Newtonsoft.Json

**What goes wrong:** The project uses `AddNewtonsoftJson()` for JSON serialization. ProblemDetails is a System.Text.Json type. If not handled correctly, ProblemDetails responses may serialize differently from the rest of the API (different casing, null handling, etc.).

**Why it happens:** `AddNewtonsoftJson()` replaces the default System.Text.Json formatter for controller actions, but the exception handler middleware writes directly to the response stream using `WriteAsJsonAsync()` which uses System.Text.Json.

**How to avoid:** The `IExceptionHandler` implementation should write ProblemDetails using the same formatter that the rest of the API uses. Either:
1. Use `IProblemDetailsService.WriteAsync()` which respects the configured formatters (preferred)
2. Or explicitly use the Newtonsoft formatter in the exception handler

**Warning signs:** ProblemDetails responses with different JSON casing (PascalCase vs camelCase) compared to `Response<T>` responses.

[ASSUMED -- based on ASP.NET Core formatter pipeline behavior. Needs verification during implementation.]

### Pitfall 4: Dual JSON Serializer Confusion

**What goes wrong:** The project configures `AddNewtonsoftJson()` with `DefaultContractResolver` (PascalCase). If the exception handler writes ProblemDetails with `WriteAsJsonAsync()` (System.Text.Json), the property names will be camelCase (`type`, `title`, `status`, `detail`, `traceId`) while the rest of the API uses PascalCase (`Status`, `Message`, `Data`).

**Why it happens:** Two different JSON libraries with different default conventions.

**How to avoid:** Use `IProblemDetailsService.WriteAsync()` which respects the configured output formatters. Or configure ProblemDetails to use the same serializer settings. The key is consistency.

[ASSUMED -- based on ASP.NET Core formatter pipeline behavior. The ProblemDetails RFC 7807 spec uses lowercase property names, which is the default for System.Text.Json. The project uses Newtonsoft with DefaultContractResolver (PascalCase). This mismatch needs resolution during implementation.]

### Pitfall 5: [ApiController] Auto-Validation Already Active

**What goes wrong:** All 19 controllers already have `[ApiController]`. This means `[ApiController]` is already auto-returning HTTP 400 ValidationProblemDetails for invalid models. Adding DataAnnotations to request models will immediately change behavior for any endpoint that currently receives invalid input -- no code changes in controllers needed.

**Why it happens:** The `[ApiController]` attribute enables `ModelStateInvalidFilter` which automatically rejects invalid models with HTTP 400 before the action method executes.

**How to avoid:** This is actually the desired behavior (per D-09). But the planner must be aware that adding `[Required]` to a model property is an immediate breaking change for Angular clients that currently send requests without that field.

**Warning signs:** Endpoints that currently accept null/missing fields gracefully will start returning 400 after DataAnnotations are added.

[VERIFIED: all 19 controllers have `[ApiController]` attribute confirmed by grep audit]

### Pitfall 6: Middletonware Order in Program.cs

**What goes wrong:** `UseExceptionHandler()` must be placed BEFORE `UseAuthentication()` and `UseAuthorization()` in the middleware pipeline, otherwise exceptions thrown during auth won't be caught.

**Why it happens:** Middleware executes in registration order. If exception handler is registered after auth middleware, exceptions from auth are not caught.

**How to avoid:** Place `app.UseExceptionHandler()` early in the pipeline, right after `app.UseSerilogRequestLogging()`.

[VERIFIED: ASP.NET Core middleware ordering documentation -- Context7 /dotnet/aspnetcore.docs]

## DataAnnotations Targets

Critical request models that need DataAnnotations (per Claude's Discretion):

### Priority 1 -- Login/Auth (highest risk, publicly accessible)
| Model | Properties to Annotate | Reason |
|-------|----------------------|--------|
| `LoginRequest` | `[Required] Username`, `[Required] Password`, `[Required] Origen` | Public endpoint, no auth required |
| `ChangePasswordRequest` | `[Required] Username`, `[Required] Password`, `[Required] NewPassword`, `[Required] Origen` | Security-critical |
| `ResetPassRequest` | `[Required] Username`, `[Required] Email` | Public endpoint |
| `CambioClave` | `[Required] Usuario`, `[Required] ClaveAnterior`, `[Required] NuevaClave` | Security-critical |

### Priority 2 -- Solicitudes (core business)
| Model | Properties to Annotate | Reason |
|-------|----------------------|--------|
| `SolicitudCompra` | `[Required] Descripcion`, `[Required] TipoContratacion`, `[Required] Area`, `[Required] TipoSolicitud` | Core business entity |
| `Autorizacion` | `[Required] CodigoSolicitud`, `[Required] EstadoAutorizacion`, `[Required] IdUsuario`, `[Required] TipoAutorizacion` | Authorization workflow |
| `SolicitudMasiva` | `[Required] ArchivoB64` | File upload |

### Priority 3 -- Cotizaciones (core business)
| Model | Properties to Annotate | Reason |
|-------|----------------------|--------|
| `Cotizacion` | `[Required] CodigoProveedor`, `[Required] CodigoSolicitud`, `[Required] CodigoUsuario` | Core business entity |
| `CotizacionMasiva` | `[Required] CodigoSolicitud`, `[Required] ArchivoB64` | File upload |
| `Adjudicacion` | `[Required] CodigoSolicitud`, `[Required] CodigoUsuario`, `[Required] EstadoSolicitud` | Business workflow |

### Priority 4 -- Proveedores
| Model | Properties to Annotate | Reason |
|-------|----------------------|--------|
| `Proveedor` | `[Required] Nombre`, `[Required] Documento`, `[Required] Email` | Registration flow |

[ASSUMED: model field importance is based on code analysis. Exact required fields should be confirmed with the user.]

## Current State Inventory

### Controller Return Type Analysis

| Pattern | Count | Controllers | Risk Level |
|---------|-------|-------------|------------|
| Returns `IActionResult` with proper status codes (Ok, NotFound, BadRequest) | ~30 methods | CatalogoController, AutorizadorGerenciaController, FormatoController, ConstanteController, parts of UsuarioController, OpcionController, RolController, parts of ProveedorController | LOW -- minimal changes needed |
| Returns `IActionResult` with `Content(HttpStatusCode..., ...)` bug | 15 instances | SolicitudController (7), ProveedorController (5), CotizacionController (2), PreguntasController (2) | HIGH -- currently returning HTTP 200 for errors |
| Returns `Response<T>` directly (no status code control) | ~33 methods | ConsultaController (5), CotizacionController (8), SolicitudController (8), UtilidadesController (10), ProveedorController (3), PreguntasController (2), OpcionController (1) | MEDIUM -- always HTTP 200, but these are GET endpoints returning data, no error paths to fix |
| Returns `ResponseStatus` directly | 5 methods | LoginController (2), UsuarioController (1), OpcionController (2, commented out) | HIGH -- cannot set HTTP status codes |
| Mixed: some IActionResult, some Response<T> | ~4 controllers | CotizacionController, SolicitudController, ProveedorController, UsuarioController | MEDIUM -- inconsistent within same controller |

### Key Statistics
- Total controllers: 19
- Controllers with `[ApiController]`: 19 (all) -- API-08 already satisfied
- Methods returning `Response<T>` directly: ~33 (mostly GET data endpoints)
- Methods returning `ResponseStatus` directly: 3 active (LoginController: 2, UsuarioController: 1)
- `Content(HttpStatusCode..., ...)` bug instances: 15
- Total try/catch blocks in controllers: ~35+
- `throw e;` instances: 0 (good -- no stack trace destruction found)

[VERIFIED: grep audit across all 19 controller files]

### Exception Type Inventory

The codebase currently has NO custom exception types. All error handling is done inline in controllers via:
- `return Content(...)` (buggy, HTTP 200)
- `return StatusCode(500, ...)` (returns plain text, not ProblemDetails)
- `return NotFound(...)` (passes exception object directly, not ProblemDetails)
- `return BadRequest(...)` (some use it correctly with ModelState)
- Setting `resp.Status = Configuracion.StatusError` (returns HTTP 200 with error in body)

[VERIFIED: grep for "class.*Exception" found zero custom exception types]

## Code Examples

### GlobalExceptionHandler Registration

```csharp
// Source: Context7 /dotnet/aspnetcore.docs -- AddExceptionHandler pattern
// PortalNegocioWS/Installers/ErrorHandlingInstaller.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PortalNegocioWS.Installers
{
    public class ErrorHandlingInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["traceId"] =
                        ctx.HttpContext.TraceIdentifier;
                };
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();
        }
    }
}
```

### Custom Exception Types

```csharp
// PortalNegocioWS/Exceptions/BusinessException.cs
namespace PortalNegocioWS.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
```

### Controller Migration: Before and After

```csharp
// BEFORE: SolicitudController.RegistrarSolicitud (current)
[HttpPost]
[Route("registrar")]
public async Task<IActionResult> RegistrarSolicitud(SolicitudCompra request)
{
    string result = await _solicitudBusiness.RegistrarSolicitud(request);
    if (result == "OK") {
        return Ok();
    } else
    {
        return Content(HttpStatusCode.BadRequest.ToString(), result);
        // BUG: returns HTTP 200, body="BadRequest", content-type=result
    }
}

// AFTER: migrated (throw exception, let GlobalExceptionHandler handle it)
[HttpPost]
[Route("registrar")]
public async Task<IActionResult> RegistrarSolicitud(SolicitudCompra request)
{
    string result = await _solicitudBusiness.RegistrarSolicitud(request);
    if (result != "OK")
        throw new BusinessException(result);
    return Ok();
}
```

### Controller Migration: LoginController

```csharp
// BEFORE: LoginController.Authenticate (current)
[HttpPost]
[EnableCors]
[Route("authenticate")]
public Response<Usuario> Authenticate(LoginRequest login)
{
    Response<Usuario> resp = new Response<Usuario>();
    if (login == null)
    {
        resp.Status = new ResponseStatus { Status = Configuracion.StatusError, Message = HttpStatusCode.BadRequest.ToString() };
        resp.Data = null;
    }
    else
    {
        resp = _loginBusiness.Authenticate(login);
    }
    return resp;
}

// AFTER: migrated to IActionResult
[HttpPost]
[EnableCors]
[Route("authenticate")]
public IActionResult Authenticate(LoginRequest login)
{
    if (login == null)
        return BadRequest(); // [ApiController] would catch this, but defensive coding

    var resp = _loginBusiness.Authenticate(login);

    // Map ResultadoLogin codes to proper HTTP responses
    if (resp.Data?.ResultadoLogin == -2)
        return Unauthorized(new { message = "Credenciales incorrectas" });
    if (resp.Data?.ResultadoLogin == -1)
        return Unauthorized(new { message = "Usuario inactivo" });
    if (resp.Data?.ResultadoLogin == 2)
        return Ok(resp);  // must change password -- still success path with data
    if (resp.Data?.ResultadoLogin == 3)
        return Ok(resp);  // expired password -- still success path with data
    if (resp.Data?.ResultadoLogin == 1)
        return Ok(resp);  // success

    return Unauthorized(new { message = "Credenciales incorrectas" });
}
```

### DataAnnotations on LoginRequest

```csharp
// Negocio.Model/Login/LoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Negocio.Model
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contrasena es requerida")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El origen es requerido")]
        public string Origen { get; set; }
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Custom try/catch per controller action | `IExceptionHandler` (since .NET 8) | .NET 8 (Nov 2023) | Centralized exception handling, no middleware boilerplate |
| Custom error JSON | `ProblemDetails` (RFC 7807) | .NET 8 (enhanced) | Standard error format across all ASP.NET Core apps |
| Manual ModelState.IsValid checks | `[ApiController]` auto-validation | .NET Core 2.1+ | Already active in this project -- DataAnnotations will immediately trigger validation |
| `UseExceptionHandler(lambda)` | `IExceptionHandler` interface | .NET 8 | Testable, composable, supports DI |

**Deprecated/outdated:**
- `UseExceptionHandler(lambda)` pattern: Still works but `IExceptionHandler` is preferred for new code since .NET 8. [VERIFIED: Context7 /dotnet/aspnetcore.docs]
- `IExceptionHandlerPathFeature`: Still available but `IExceptionHandlerFeature` is the primary interface for accessing exception details.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `IProblemDetailsService.WriteAsync()` respects the configured Newtonsoft.Json formatter | Pitfall 3 | ProblemDetails would serialize with different casing than `Response<T>`. Mitigation: test during implementation and adjust. |
| A2 | Model field importance for DataAnnotations (which fields are truly required) | DataAnnotations Targets | Over-validating would break existing Angular calls. Mitigation: only annotate fields that are already validated server-side in business layer. |
| A3 | `ResultadoLogin` codes -2, -1 are error cases; 2, 3 are success-with-warning cases | LoginController migration | If Angular treats 2/3 differently, the migration guide needs specific handling notes. Mitigation: document in migration guide. |
| A4 | No new NuGet packages needed for this phase | Standard Stack | If ProblemDetails serialization needs customization beyond built-in, a package might be needed. Low risk -- built-in should suffice. |
| A5 | The `Content(string, string)` overload analysis is correct -- the first arg is body, second is content-type | Pitfall 1 | If wrong, the bug description is incorrect. But this is verified by the ASP.NET Core source code (Context7 PublicAPI.Shipped.txt). |

## Open Questions (RESOLVED)

1. **ProblemDetails JSON casing consistency**
   - What we know: Project uses Newtonsoft.Json with `DefaultContractResolver` (PascalCase). ProblemDetails RFC 7807 uses lowercase (`type`, `title`, `status`, `detail`).
   - What's unclear: Whether to enforce lowercase for ProblemDetails (standard) or PascalCase (consistency with `Response<T>`).
   - RESOLVED: Use RFC 7807 lowercase for ProblemDetails (standard format). Angular's error interceptor should handle this. The two response formats (success vs error) are intentionally different per the phase design.

2. **LoginController `ResultadoLogin` codes 2 and 3 (must change password / expired password)**
   - What we know: These return HTTP 200 today with user data including the code. Angular likely reads the code to redirect to password change.
   - What's unclear: Whether Angular treats these as success or error flows.
   - RESOLVED: Keep HTTP 200 for codes 2 and 3 (they return user data and need the `Response<T>` envelope). Only codes -1 and -2 should become HTTP 401.

3. **Controllers returning `Response<T>` directly for GET endpoints**
   - What we know: ~33 methods return `Response<T>` directly (not `IActionResult`). These are mostly GET endpoints that always succeed.
   - What's unclear: Whether to also migrate these to `IActionResult` in this phase.
   - RESOLVED: Per D-05, success paths keep `Response<T>`. These GET endpoints only have success paths, so they can stay as-is. Only migrate if they have error paths that need fixing.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 9.0 SDK | Build and run | Verified | net9.0 | -- |
| ASP.NET Core ProblemDetails | Error handling | Built-in | 9.0 | -- |
| IExceptionHandler | Exception handling | Built-in | 9.0 | -- |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | JSON serialization | Installed | 9.0.9 | -- |
| System.ComponentModel.DataAnnotations | Input validation | Built-in | 9.0 | -- |

**Missing dependencies with no fallback:** None -- all functionality uses built-in or already-installed packages.

**Missing dependencies with fallback:** N/A

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None -- no test project exists in solution |
| Config file | none |
| Quick run command | `dotnet build PortalNegocioWS.sln` |
| Full suite command | N/A -- no tests |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| API-01 | Unhandled exception returns ProblemDetails JSON with HTTP 500 | manual-only | -- | No |
| API-02 | AddProblemDetails + UseExceptionHandler configured | build verification | `dotnet build` | N/A |
| API-03 | ApiControllerBase exists with helper methods | build verification | `dotnet build` | N/A |
| API-04 | LoginController returns IActionResult with 401 for bad credentials | manual-only | -- | No |
| API-05 | 3+ controllers have correct HTTP codes | manual-only | -- | No |
| API-06 | All controllers migrated to ApiControllerBase | build verification | `dotnet build` | N/A |
| API-07 | DataAnnotations on critical request models | build verification | `dotnet build` | N/A |
| API-08 | [ApiController] on all controllers | already verified | -- | N/A |
| API-09 | Angular migration guide exists | file existence | -- | No |

**Justification for manual-only tests:** No test project exists (CLAUDE.md confirms "No test projects exist in this solution"). Phase 6 is dedicated to testing infrastructure. This phase is purely code/config changes verifiable by compilation and manual API testing.

### Sampling Rate
- **Per task commit:** `dotnet build PortalNegocioWS.sln`
- **Per wave merge:** `dotnet build PortalNegocioWS.sln`
- **Phase gate:** Full build succeeds + manual verification of error responses via curl/Swagger

### Wave 0 Gaps
- No test framework gaps to fill -- testing is deferred to Phase 6 per REQUIREMENTS.md
- Build verification (`dotnet build`) is the quality gate for this phase

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | yes | LoginController migration to proper 401 responses |
| V3 Session Management | no | JWT handling unchanged |
| V4 Access Control | no | [Authorize] attributes unchanged |
| V5 Input Validation | yes | DataAnnotations on request models + [ApiController] auto-validation |
| V6 Cryptography | no | No crypto changes |

### Known Threat Patterns for ASP.NET Core API Error Handling

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Stack trace leakage in error responses | Information Disclosure | GlobalExceptionHandler returns generic message for 500, never `exception.Message` in production |
| Error response content-type mismatch | Tampering | Ensure ProblemDetails always returns `application/problem+json` |
| Validation bypass | Tampering | `[ApiController]` auto-validation cannot be bypassed at controller level |

**Security note:** The GlobalExceptionHandler must NOT include `exception.Message` or stack traces in HTTP 500 responses for non-development environments. Only `BusinessException.Message` should be exposed to clients (400-level errors are safe because they contain user-facing messages). For 500 errors, return a generic "An unexpected error occurred" message and log the full exception via Serilog.

## Sources

### Primary (HIGH confidence)
- Context7 /dotnet/aspnetcore.docs -- IExceptionHandler, ProblemDetails, AddProblemDetails, UseExceptionHandler, ApiController auto-validation, DataAnnotations
- Context7 /dotnet/aspnetcore -- ControllerBase.Content method overloads (PublicAPI.Shipped.txt)
- Codebase grep audit -- all 19 controllers analyzed for return types, error patterns, `[ApiController]` presence
- Directory.Packages.props -- verified package versions
- Program.cs -- verified middleware pipeline and Newtonsoft.Json configuration

### Secondary (MEDIUM confidence)
- ASP.NET Core error handling documentation patterns from Context7 snippets
- ASP.NET Core middleware ordering conventions

### Tertiary (LOW confidence)
- None -- all findings are verified against codebase or Context7 docs

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all functionality is built into ASP.NET Core 9.0, verified via Context7 and codebase analysis
- Architecture: HIGH -- IExceptionHandler + ProblemDetails pattern is well-documented and directly applicable
- Pitfalls: HIGH -- `Content()` bug verified by ASP.NET Core source API; Newtonsoft/ProblemDetails interaction is the only uncertainty
- DataAnnotations targets: MEDIUM -- model field importance based on code analysis, not confirmed by user

**Research date:** 2026-04-16
**Valid until:** 2026-05-16 (ASP.NET Core 9.0 is stable, patterns unlikely to change)
