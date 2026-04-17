---
phase: 06-testing-safety-net
plan: 02
subsystem: testing
tags: [xunit, integration-testing, jwt, moq, auth]

requires:
  - phase: 06-testing-safety-net
    plan: 01
    provides: CustomWebApplicationFactory, JwtTokenHelper

provides:
  - 4 passing integration tests in PortalNegocioWS.Tests/Auth/
  - Auth boundary regression guard (401 without token, not-401 with token)
  - Login flow regression guard (200 valid credentials, 401 invalid credentials)

affects:
  - 06-testing-safety-net

tech-stack:
  added: []
  patterns: [WebApplicationFactory per-test DI override, Moq ILogin replacement]

key-files:
  created:
    - PortalNegocioWS.Tests/Auth/AuthBoundaryTests.cs
    - PortalNegocioWS.Tests/Auth/LoginFlowTests.cs
  modified:
    - PortalNegocioWS.Tests/Infrastructure/CustomWebApplicationFactory.cs
    - PortalNegocioWS.Tests/PortalNegocioWS.Tests.csproj
    - PortalNegocioWS/Program.cs
    - PortalNegocioWS/Services/CronJobService.cs
    - PortalNegocioWS/Mappings/Profiles/AuthProfile.cs
    - PortalNegocioWS/Mappings/Profiles/CatalogoProfile.cs
    - PortalNegocioWS/Mappings/Profiles/NotificacionProfile.cs
    - PortalNegocioWS/appsettings.json

key-decisions:
  - "Per-test factory instances for LoginFlowTests to avoid WithWebHostBuilder Serilog freeze"
  - "Non-reloadable bootstrap logger when Microsoft.AspNetCore.Mvc.Testing is loaded"
  - "CronJobService refactored from System.Timers.Timer to BackgroundService loop for delays > int.MaxValue ms"

patterns-established:
  - "CustomWebApplicationFactory.WithExtraServices(Action<IServiceCollection>) enables per-test mock injection"
  - "Auth tests use SolicitudController list endpoint as representative [Authorize] target"

requirements-completed: [TST-02]

duration: 75min
completed: 2026-04-17
---

# Phase 6 Plan 2: Auth Boundary Tests Summary

**Integration tests for authentication boundary and login flow**

## Performance

- **Duration:** 75 min
- **Started:** 2026-04-17T19:15:00Z
- **Completed:** 2026-04-17T20:30:00Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- Created `AuthBoundaryTests` with 2 tests: 401 without token, not-401 with token
- Created `LoginFlowTests` with 2 tests: 200 with valid credentials mock, 401 with invalid credentials mock
- Added `WithExtraServices` to `CustomWebApplicationFactory` for per-test DI overrides
- Fixed `appsettings.json` duplicate key `Settings:TipoIdentificacionJuridica`
- Fixed AutoMapper profile unmapped members (Usuario.ResultadoLogin, Rol.ListaOpciones, Catalogo.Medida, Noticias.CorreoNotificacion/ArchivoB64/FotoB64)
- Refactored `CronJobService` to `BackgroundService` with chunked `Task.Delay` to support cron intervals > 24 days
- Switched Serilog bootstrap to non-reloadable logger when `Microsoft.AspNetCore.Mvc.Testing` is present, preventing "logger already frozen" across multiple factory instances

## Task Commits

1. **Task 1: Inspect controllers and interfaces** - no commit (read-only)
2. **Task 2: Create AuthBoundaryTests.cs and LoginFlowTests.cs** - `32b4453` (test)

**Plan metadata:** `32b4453`

## Files Created/Modified
- `PortalNegocioWS.Tests/Auth/AuthBoundaryTests.cs` - 401 boundary tests
- `PortalNegocioWS.Tests/Auth/LoginFlowTests.cs` - Login flow tests with mocked ILogin
- `PortalNegocioWS.Tests/Infrastructure/CustomWebApplicationFactory.cs` - Added `WithExtraServices` method
- `PortalNegocioWS.Tests/PortalNegocioWS.Tests.csproj` - Added direct project refs to Negocio.Business and Negocio.Model
- `PortalNegocioWS/Program.cs` - Conditional non-reloadable bootstrap logger for test runs
- `PortalNegocioWS/Services/CronJobService.cs` - BackgroundService loop replacing System.Timers.Timer
- `PortalNegocioWS/Mappings/Profiles/AuthProfile.cs` - Ignore unmapped Usuario/Rol properties
- `PortalNegocioWS/Mappings/Profiles/CatalogoProfile.cs` - Ignore unmapped Catalogo.Medida
- `PortalNegocioWS/Mappings/Profiles/NotificacionProfile.cs` - Ignore unmapped Noticias properties
- `PortalNegocioWS/appsettings.json` - Removed duplicate `TipoIdentificacionJuridica` key

## Decisions Made
- Used per-test `CustomWebApplicationFactory` instances in `LoginFlowTests` instead of `WithWebHostBuilder` to avoid Serilog reloadable logger freeze when `Program.Main` runs multiple times
- Detected test runtime via `AppDomain.CurrentDomain.GetAssemblies()` check for `Microsoft.AspNetCore.Mvc.Testing`
- Chose `BackgroundService` with chunked `Task.Delay` over `System.Threading.Timer` because even `Timer` with `TimeSpan` has a `dueTime` ceiling of ~49 days

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed duplicate key in appsettings.json**
- **Found during:** Test run after creating auth tests
- **Issue:** `appsettings.json` contained `"TipoIdentificacionJuridica": 4` twice under `Settings`, causing `InvalidDataException` on host build
- **Fix:** Removed duplicate line
- **Committed in:** `32b4453`

**2. [Rule 3 - Blocking] Fixed AutoMapper unmapped members**
- **Found during:** Host build validation after fixing appsettings.json
- **Issue:** `AssertConfigurationIsValid()` threw for missing mappings: `Usuario.ResultadoLogin/Token/Proveedor/Opciones`, `Rol.ListaOpciones`, `Catalogo.Medida`, `Noticias.CorreoNotificacion/ArchivoB64/FotoB64`
- **Fix:** Added `.ForMember(..., opt => opt.Ignore())` for unmapped destination properties in AuthProfile, CatalogoProfile, and NotificacionProfile
- **Committed in:** `32b4453`

**3. [Rule 3 - Blocking] Fixed CronJobService crash on long cron intervals**
- **Found during:** Host build validation in Testing environment
- **Issue:** `CronJobService` used `System.Timers.Timer` (and later `System.Threading.Timer`) with intervals computed from cron expression. For `0 0 1 1 *` (yearly), interval exceeded `int.MaxValue` ms, crashing host startup
- **Fix:** Replaced timer-based scheduling with `BackgroundService.ExecuteAsync` loop using chunked `Task.Delay` (max `int.MaxValue - 1` ms per chunk)
- **Committed in:** `32b4453`

**4. [Rule 3 - Blocking] Fixed Serilog "logger already frozen" across factory instances**
- **Found during:** Running all auth tests together
- **Issue:** `Program.cs` used `CreateBootstrapLogger()` which returns a `ReloadableLogger`. `UseSerilog` freezes it on host build. When a second `WebApplicationFactory` instance runs `Program.Main`, the new bootstrap logger is frozen by the prior factory's `AddSerilog` registration or by shared static state
- **Fix:** Replaced `CreateBootstrapLogger()` with `CreateLogger()` (non-reloadable) when `Microsoft.AspNetCore.Mvc.Testing` assembly is loaded
- **Committed in:** `32b4453`

---

**Total deviations:** 4 auto-fixed (all blocking test execution)
**Impact on plan:** All fixes were necessary to achieve passing auth tests. No scope creep — fixes address pre-existing production bugs exposed by test infrastructure.

## Issues Encountered
- `WithWebHostBuilder` on `CustomWebApplicationFactory` caused `InvalidOperationException: The logger is already frozen` when run after `AuthBoundaryTests` fixture
- `System.Timers.Timer` and `System.Threading.Timer` both have hard ceilings on delay duration, making yearly cron jobs impossible without chunked delays

## Next Phase Readiness
- `dotnet test --filter AuthBoundaryTests` exits 0 with 2 passing tests
- `dotnet test --filter LoginFlowTests` exits 0 with 2 passing tests
- Wave 2 remaining plans (06-03, 06-04, 06-05) can proceed

---
*Phase: 06-testing-safety-net*
*Completed: 2026-04-17*
