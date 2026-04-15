# Architecture

**Analysis Date:** 2026-04-14

## Pattern Overview

**Overall:** N-Tier / Layered Architecture (4 layers)

**Key Characteristics:**
- Strict layer separation: API → Business → Data, with Models shared across all layers
- Business logic is entirely in `Negocio.Business`; controllers are thin dispatchers
- Data access is done directly via `PORTALNEGOCIODataContext` (Devart LinqConnect ORM), without a repository abstraction layer
- DI wiring is modularized through reflection-discovered `IInstaller` classes
- Background scheduled jobs run as `IHostedService` implementations within the API process

## Layers

**API Layer (`PortalNegocioWS`):**
- Purpose: HTTP request handling, middleware pipeline, DI configuration, background jobs
- Location: `PortalNegocioWS/`
- Contains: Controllers, Installers, Background Job Services, AutoMapper profile, `Program.cs`
- Depends on: `Negocio.Business` (interfaces), `Negocio.Model` (DTOs)
- Used by: External HTTP clients (frontend, other services)

**Business Layer (`Negocio.Business`):**
- Purpose: All domain logic, authentication, validation, email notification triggering, storage
- Location: `Negocio.Business/`
- Contains: Domain service interfaces (`I{Domain}.cs`) and implementations (`{Domain}Business.cs` or `{Domain}.cs`), storage abstractions
- Depends on: `Negocio.Data` (LinqConnect DataContext), `Negocio.Model` (DTOs)
- Used by: `PortalNegocioWS` controllers and background jobs

**Model Layer (`Negocio.Model`):**
- Purpose: Shared DTOs, request/response objects, and application-wide constants
- Location: `Negocio.Model/`
- Contains: Domain-organized DTOs, `Response<T>`, `ResponseStatus`, `Configuracion` (static constants), `ChangePasswordRequest`
- Depends on: Nothing (lowest-level project)
- Used by: All other layers

**Data Layer (`Negocio.Data`):**
- Purpose: Auto-generated Oracle ORM context. Single source of truth for DB entity types
- Location: `Negocio.Data/`
- Contains: `DataContext.Designer.cs` (auto-generated, do not edit), `DataContext.lqml` (LinqConnect schema definition)
- Depends on: Devart LinqConnect Oracle driver
- Used by: `Negocio.Business` service implementations and background jobs

## Data Flow

**Authenticated API Request:**
1. HTTP request arrives at controller (e.g., `SolicitudController`)
2. JWT middleware validates Bearer token via `AuthenticationInstaller` config
3. Controller calls the injected business interface (e.g., `ISolicitudCompra`)
4. Business implementation creates a `PORTALNEGOCIODataContext` instance inside a `using` block
5. LINQ queries execute against the Oracle DB via LinqConnect
6. Results are projected into `Negocio.Model` DTOs (sometimes using AutoMapper)
7. Controller wraps result in `Response<T>` and returns to caller

**Authentication Flow:**
1. POST `/api/login/authenticate` with `LoginRequest`
2. `LoginController` → `ILogin.Authenticate()`
3. `LoginBusiness` queries `POGEUSUARIO` table, validates encrypted password
4. On success: JWT token generated with HMAC-SHA256 using `JWT:SecretKey` from config
5. Response: `Response<Usuario>` with JWT token in the `Usuario` object

**Background Job Flow:**
1. `CronJobService` (abstract base, `IHostedService`) schedules using cron expression
2. Timer fires → `DoWork()` executes
3. Job creates `PORTALNEGOCIODataContext` directly (no DI injection for DataContext)
4. Data updated via LINQ + `SubmitChanges()` with manual transaction when needed

**State Management:**
- No in-process state. All state in Oracle DB.
- Redis cache infrastructure exists in `RedisManager/` project but is fully disabled (commented out in `CacheInstaller.cs`)
- File storage state managed by `IStorageService`; only `LocalStorageService` is active

## Key Abstractions

**`IInstaller`:**
- Purpose: Modular DI registration — each installer handles one cross-cutting concern
- Examples: `PortalNegocioWS/Installers/BusinessInstaller.cs`, `AuthenticationInstaller.cs`, `CorsInstaller.cs`, `AutoMapperInstaller.cs`, `CacheInstaller.cs`, `SwaggerInstaller.cs`
- Pattern: All classes implementing `IInstaller` are discovered by reflection at startup and registered automatically; no explicit listing in `Program.cs`

**`Response<T>` + `ResponseStatus`:**
- Purpose: Unified API response envelope used across most endpoints
- Examples: `Negocio.Model/Response/Response.cs`, `Negocio.Model/Response/ResponseStatus.cs`
- Pattern: `Response<T>` has `Status` (ResponseStatus with `Status` string "OK"/"ERROR" and `Message`) and `Data` of type `T`. Constants `Configuracion.StatusOk` and `Configuracion.StatusError` used for status values.

**Business Service Interface Pattern:**
- Purpose: Decouples controller from implementation; enables DI
- Examples: `Negocio.Business/Solicitud/ISolicitudCompra.cs` + implementation in `SolicitudCompra.cs`; `Negocio.Business/Login/ILogin.cs` + `Login.cs`; `Negocio.Business/Notificacion/INotificacion.cs` + `Notificacion.cs`
- Pattern: `I{Domain}.cs` defines the interface, `{Domain}Business.cs` or `{Domain}.cs` implements it. All registered as `Scoped` in `BusinessInstaller`.

**`CronJobService`:**
- Purpose: Abstract base class for scheduled background jobs
- Examples: `PortalNegocioWS/Services/CronJobService.cs`; subclasses: `ActualizarEstadoSolicitudJob.cs`, `EnviarNotificacionInvitacionJob.cs`, `NotificacionActualizacionDatosJob.cs`
- Pattern: Subclass overrides `DoWork(CancellationToken)`. Cron expression configured at registration in `Program.cs`.

**`IStorageService`:**
- Purpose: Pluggable file storage abstraction
- Examples: `Negocio.Business/Utilidades/IStorageService.cs`; active implementation: `Negocio.Business/Utilidades/LocalStorageService.cs`; disabled: `S3StorageService.cs`, `RemoteStorageService.cs`
- Pattern: Registered as Singleton in `Program.cs` using a switch on `Storage:Type` config key

**`Configuracion` (static constants):**
- Purpose: Centralized application-wide string/int constants (statuses, types, notification references)
- Location: `Negocio.Model/Configuracion/Configuracion.cs`
- Pattern: Static class with `public const` members; used across Business and API layers

## Entry Points

**HTTP API Entry Point:**
- Location: `PortalNegocioWS/Program.cs`
- Triggers: ASP.NET Core host startup
- Responsibilities: Discovers and runs all `IInstaller` registrations, configures storage singleton, registers cron jobs, configures middleware pipeline (CORS, Authorization, Controllers)

**Controller Entry Points:**
- Location: `PortalNegocioWS/Controllers/`
- Triggers: HTTP requests matched by `[Route("api/[controller]")]`
- Responsibilities: Input validation, calling business service, wrapping result in `Response<T>`, logging errors
- Authentication: Most controllers use `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`; `LoginController` is unauthenticated

**Background Job Entry Points:**
- Location: `PortalNegocioWS/Services/ActualizarEstadoSolicitudJob.cs`, `EnviarNotificacionInvitacionJob.cs`, `NotificacionActualizacionDatosJob.cs`
- Triggers: Timer-based cron schedule configured in `appsettings.json` under `Settings:CronEnviarInvitacion`; `ActualizarEstadoSolicitudJob` hardcoded to `"0 0 * * *"` (daily midnight)
- Responsibilities: Bulk DB state updates, email notification dispatch

## Error Handling

**Strategy:** Mixed — no unified exception middleware. Each controller method handles its own errors.

**Patterns:**
- Controllers catch exceptions, log with `ILogger<T>`, and return `Content(HttpStatusCode.BadRequest.ToString(), e.Message)`
- Business methods return string `"OK"` on success or an error message string on failure (for write operations)
- For data reads, business methods return typed lists/objects; null or empty collections on no results
- `Response<T>` with `Status = "ERROR"` used for some authentication/validation flows
- No global exception filter or `ProblemDetails` middleware

## Cross-Cutting Concerns

**Logging:** `ILogger<T>` injected into controllers and some background jobs; Serilog was partially configured but the full Serilog setup is commented out in `Program.cs`. Currently uses default .NET logging.

**Validation:** Manual null checks in controllers. No `[Required]` annotation validation middleware or FluentValidation.

**Authentication:** JWT Bearer via `AuthenticationInstaller`. Token validated on every request to `[Authorize]` endpoints. Role-based access tables exist (`POGEROL`, `POGEOPCIONXROL`) but role enforcement is done in business logic, not via `[Authorize(Roles = "...")]`.

**AutoMapper:** Singleton registered by `AutoMapperInstaller`. Single profile: `PortalNegocioWS/Mappings/MappingProfile.cs` with 80+ DB entity → DTO mappings.

---

*Architecture analysis: 2026-04-14*
