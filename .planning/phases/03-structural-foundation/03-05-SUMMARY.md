---
phase: 03-structural-foundation
plan: 05
subsystem: business-logic
tags: [constructor-injection, IDataContextFactory, IConfiguration, connection-cleanup, login]

# Dependency graph
requires:
  - phase: 03-structural-foundation
    plan: 03-02
    provides: IDataContextFactory registered as Singleton in DI container
provides:
  - "ILogin interface without IConfiguration in method signatures"
  - "LoginBusiness with constructor-injected IConfiguration and IDataContextFactory"
  - "All DataContext usage in Login.cs wrapped in using blocks"
affects: [04-api-cleanup, 03-04]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Constructor injection of IConfiguration (D-05)"
    - "using block pattern for DataContext lifecycle (D-07)"

key-files:
  created: []
  modified:
    - Negocio.Business/Login/ILogin.cs
    - Negocio.Business/Login/Login.cs
    - PortalNegocioWS/Controllers/LoginController.cs
    - PortalNegocioWS/Controllers/UsuarioController.cs

key-decisions:
  - "IConfiguration removed from ILogin method signatures, injected via LoginBusiness constructor (D-05)"
  - "All callers updated in same plan to avoid broken intermediate state (D-06)"
  - "LoginController no longer needs IConfiguration injection -- all 3 uses were only for passing to LoginBusiness"

patterns-established:
  - "Constructor injection over method-parameter injection for cross-cutting dependencies"
  - "using block wrapping for all DataContext usage ensures Oracle connection cleanup"

requirements-completed: [DAT-08, DAT-09]

# Metrics
duration: 3min
completed: 2026-04-16
---

# Phase 3 Plan 5: LoginBusiness DI Fix Summary

**IConfiguration moved from method parameters to LoginBusiness constructor injection; all three methods (Authenticate, ChangePassword, ResetPassword) now use IDataContextFactory with proper using blocks for Oracle connection cleanup.**

## Performance

- **Duration:** 3 min
- **Started:** 2026-04-16T04:27:31Z
- **Completed:** 2026-04-16T04:30:47Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- ILogin interface cleaned: all 3 method signatures no longer accept IConfiguration parameter
- LoginBusiness constructor now receives (IUtilidades, IConfiguration, IDataContextFactory)
- All DataContext instantiations in Login.cs replaced with `_factory.Create()` inside using blocks
- LoginController simplified: IConfiguration injection removed entirely (was only used for LoginBusiness calls)
- UsuarioController updated: _configuration argument removed from ChangePassword call (field retained for other uses)
- Build passes with 0 errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Update ILogin.cs interface and Login.cs implementation** - `5a1ac80` (feat)
2. **Task 2: Update callers in LoginController and UsuarioController -- then build** - `a05c8d7` (feat)

## Files Created/Modified
- `Negocio.Business/Login/ILogin.cs` - Removed IConfiguration from all 3 method signatures and Microsoft.Extensions.Configuration import
- `Negocio.Business/Login/Login.cs` - Added IConfiguration and IDataContextFactory to constructor; wrapped all DataContext usage in using blocks; replaced parameter references with _configuration field
- `PortalNegocioWS/Controllers/LoginController.cs` - Removed IConfiguration constructor injection and field (no longer needed); updated 3 method calls
- `PortalNegocioWS/Controllers/UsuarioController.cs` - Removed _configuration argument from ChangePassword call (field retained for EncryptedKey and DiasVenceClave)

## Decisions Made
- **LoginController IConfiguration removal:** LoginController had `_configuration` injected solely to pass to `_loginBusiness` methods. Since IConfiguration is now constructor-injected into LoginBusiness, LoginController no longer needs it. The unused `using Microsoft.Extensions.Configuration;` import was also removed.
- **NotificacionBusiness inline instantiation left unchanged:** The plan's Step 5 suggested passing `_factory` to `new NotificacionBusiness(_utilidades, _factory)`, but NotificacionBusiness has not yet been migrated to accept IDataContextFactory (that is Plan 03-04). The existing `new NotificacionBusiness(_utilidades)` call remains correct.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] NotificacionBusiness constructor does not yet accept IDataContextFactory**
- **Found during:** Task 1 (Update Login.cs implementation)
- **Issue:** Plan Step 5 instructed to pass `_factory` to `new NotificacionBusiness(_utilidades, _factory)`, but NotificacionBusiness migration to IDataContextFactory is part of Plan 03-04 (not yet executed). Its constructor currently only accepts `IUtilidades`.
- **Fix:** Left the inline `new NotificacionBusiness(_utilidades)` call unchanged. This will be updated when Plan 03-04 migrates NotificacionBusiness.
- **Files modified:** None (intentionally left as-is)
- **Verification:** Build succeeds; NotificacionBusiness constructor confirmed via code inspection

**2. [Rule 1 - Bug] Missing opening brace on using block in Authenticate**
- **Found during:** Task 1 (Authenticate method edit)
- **Issue:** After replacing `PORTALNEGOCIODataContext dc = new PORTALNEGOCIODataContext();` with `using (var dc = _factory.Create())`, the original code lacked an opening brace for the using block scope.
- **Fix:** Added `{` after `using (var dc = _factory.Create())` and closing `}` before `return resp`
- **Files modified:** Negocio.Business/Login/Login.cs
- **Verification:** Build passes; grep confirms `using (var dc/cx = _factory.Create())` followed by `{` in all 3 methods
- **Committed in:** `5a1ac80` (Task 1 commit)

---

**Total deviations:** 2 (1 pre-condition mismatch, 1 syntax fix)
**Impact on plan:** Both are correctness adjustments. No scope creep.

## Issues Encountered
None - the changes were mechanical and straightforward.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- LoginBusiness is now fully aligned with the IDataContextFactory pattern
- Plan 03-04 should migrate NotificacionBusiness (among 10 other services) which will resolve the remaining inline `new NotificacionBusiness(_utilidades)` in ResetPassword
- Proveedor.cs (03-06) and SolicitudCompra.cs (03-07) migrations still pending, require transaction boundary map

---
*Phase: 03-structural-foundation*
*Completed: 2026-04-16*
