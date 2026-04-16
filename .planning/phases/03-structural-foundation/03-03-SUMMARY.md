---
phase: 03-structural-foundation
plan: 03
subsystem: infra
tags: [di, dependency-injection, datacontext-factory, migration]

# Dependency graph
requires:
  - phase: 03-02
    provides: "IDataContextFactory registered as Singleton in DI container via BusinessInstaller"
provides:
  - "5 smallest business services migrated from direct new PORTALNEGOCIODataContext() to IDataContextFactory.Create()"
  - "Validated migration pattern is safe for services with and without manual transactions"
affects: [03-04, 03-05, 03-06, 03-07]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Constructor-injected IDataContextFactory with _factory.Create() replacing all new PORTALNEGOCIODataContext() calls"
    - "Factory parameter added after existing constructor parameters, preserving DI resolution order"

key-files:
  created: []
  modified:
    - Negocio.Business/Noticias/Noticias.cs
    - Negocio.Business/AutorizadorGerencia/AutorizadorGerenciaBusiness.cs
    - Negocio.Business/ParametroGeneral/ParametroGeneral.cs
    - Negocio.Business/Constante/ConstanteBusiness.cs
    - Negocio.Business/NotificacionUsuario/NotificacionUsuarioBusiness.cs

key-decisions:
  - "Factory parameter appended as last constructor argument to avoid breaking DI resolution of existing parameters"

patterns-established:
  - "Migration pattern: add readonly IDataContextFactory _factory field, inject via constructor, replace all new PORTALNEGOCIODataContext() with _factory.Create()"

requirements-completed: [DAT-03]

# Metrics
duration: 3min
completed: 2026-04-16
---

# Phase 3 Plan 03: Migrate 5 Small Services to IDataContextFactory Summary

**5 smallest business services migrated from direct DataContext instantiation to factory pattern, validating the approach before larger service migrations**

## Performance

- **Duration:** 3 min
- **Started:** 2026-04-16T04:21:59Z
- **Completed:** 2026-04-16T04:24:33Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Migrated NoticiasBusiness, AutorizadorGerenciaBusiness, ParametroGeneral, ConstanteBusiness, and NotificacionUsuarioBusiness from `new PORTALNEGOCIODataContext()` to `_factory.Create()`
- Confirmed pattern works with both `using (var cx = _factory.Create())` and `using var cx = _factory.Create()` declaration styles
- Verified build succeeds with 0 errors after all migrations
- Zero remaining direct DataContext instantiations across all 5 files

## Task Commits

Each task was committed atomically:

1. **Task 1: Migrate Noticias.cs and AutorizadorGerenciaBusiness.cs** - `6f1ffad` (feat)
2. **Task 2: Migrate ParametroGeneral.cs, ConstanteBusiness.cs, NotificacionUsuarioBusiness.cs** - `0c2499f` (feat)

## Files Created/Modified
- `Negocio.Business/Noticias/Noticias.cs` - Added IDataContextFactory injection, 3 Create() calls
- `Negocio.Business/AutorizadorGerencia/AutorizadorGerenciaBusiness.cs` - Added IDataContextFactory injection, 4 Create() calls
- `Negocio.Business/ParametroGeneral/ParametroGeneral.cs` - Added IDataContextFactory injection, 5 Create() calls
- `Negocio.Business/Constante/ConstanteBusiness.cs` - Added IDataContextFactory injection after existing IConfiguration/IUtilidades params, 4 Create() calls
- `Negocio.Business/NotificacionUsuario/NotificacionUsuarioBusiness.cs` - Added IDataContextFactory injection, 4 Create() calls

## Decisions Made
- Factory parameter added as last constructor argument in all services to preserve existing DI parameter resolution order
- ConstanteBusiness already had 2 constructor params (IConfiguration, IUtilidades) -- factory appended as 3rd parameter, confirming DI resolves multi-param constructors correctly

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Migration pattern validated on 5 services including both read-only and transactional methods
- Plans 03-04 through 03-07 can confidently apply the same mechanical pattern to remaining services
- DI auto-resolution confirmed: no changes to BusinessInstaller needed for constructor parameter changes

---
*Phase: 03-structural-foundation*
*Completed: 2026-04-16*
