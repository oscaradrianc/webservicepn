---
phase: 06-testing-safety-net
plan: 03
subsystem: testing
tags: [xunit, integration-testing, moq, solicitud]

requires:
  - phase: 06-testing-safety-net
    plan: 01
    provides: CustomWebApplicationFactory, JwtTokenHelper

provides:
  - 2 passing integration tests in PortalNegocioWS.Tests/Business/
  - Regression guard for SolicitudController Autorizar and Registrar endpoints

affects:
  - 06-testing-safety-net

tech-stack:
  added: []
  patterns: [per-test factory with ExtraServices, model validation in request body]

key-files:
  created:
    - PortalNegocioWS.Tests/Business/SolicitudTests.cs
  modified: []

key-decisions:
  - "Per-test factory instances to avoid shared state and client disposal issues"
  - "Populate required model properties to satisfy [ApiController] model validation (422 guard)"

patterns-established:
  - "Business tests mock the service interface and assert HTTP 200 when business returns OK"

requirements-completed: [TST-03]

duration: 20min
completed: 2026-04-17
---

# Phase 6 Plan 3: Solicitud Business Tests Summary

**Integration tests for SolicitudController critical business operations**

## Performance

- **Duration:** 20 min
- **Started:** 2026-04-17T20:30:00Z
- **Completed:** 2026-04-17T20:50:00Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Created `SolicitudTests` with 2 tests: AutorizarSolicitud and RegistrarSolicitud
- Mocked `ISolicitudCompra` to return `"OK"`, asserting HTTP 200 from controller
- Used authenticated `HttpClient` with Bearer token from `JwtTokenHelper`
- Populated required request model properties to pass `[ApiController]` model validation

## Task Commits

1. **Task 1: Inspect ISolicitudCompra and models** - no commit (read-only)
2. **Task 2: Create SolicitudTests.cs** - `99768ba` (test)

**Plan metadata:** `99768ba`

## Files Created/Modified
- `PortalNegocioWS.Tests/Business/SolicitudTests.cs` - 2 integration tests for SolicitudController

## Decisions Made
- Used per-test `CustomWebApplicationFactory` instances instead of `IClassFixture` to avoid `HttpClient` disposal when the factory is disposed inside a helper method
- Added required fields (`CodigoSolicitud`, `EstadoAutorizacion`, `IdUsuario`, `TipoAutorizacion`, `Descripcion`, `TipoSolicitud`) to request bodies after initial 422 failures

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Non-blocking] Fixed 422 UnprocessableEntity from empty request bodies**
- **Found during:** Test execution
- **Issue:** `new Autorizacion()` and `new SolicitudCompra()` triggered `[ApiController]` model validation (missing `[Required]` fields)
- **Fix:** Populated required properties in request bodies
- **Committed in:** `99768ba`

---

**Total deviations:** 1 auto-fixed (non-blocking)

## Next Phase Readiness
- `dotnet test --filter SolicitudTests` exits 0 with 2 passing tests
- Ready for 06-04 (AutoMapper validation test)

---
*Phase: 06-testing-safety-net*
*Completed: 2026-04-17*
