---
phase: 03-structural-foundation
plan: 02
subsystem: infra
tags: [di, dependency-injection, datacontext-factory, singleton]

# Dependency graph
requires:
  - phase: 03-01
    provides: "IDataContextFactory interface and DataContextFactory implementation in Negocio.Data"
provides:
  - "IDataContextFactory registered as Singleton in DI container via BusinessInstaller"
  - "Confirmed Negocio.Data project reference already exists in Negocio.Business.csproj"
affects: [03-03, 03-04, 03-05, 03-06, 03-07]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Singleton factory registration before Scoped service registrations in BusinessInstaller"

key-files:
  created: []
  modified:
    - PortalNegocioWS/Installers/BusinessInstaller.cs

key-decisions:
  - "Singleton lifetime for DataContextFactory is correct: it only holds a cached connection string, creates new DataContext per Create() call"

patterns-established:
  - "Factory DI pattern: Singleton IDataContextFactory injected into Scoped business services"

requirements-completed: [DAT-02]

# Metrics
duration: 2min
completed: 2026-04-16
---

# Phase 3 Plan 02: Register DataContextFactory in DI Container Summary

**IDataContextFactory registered as Singleton in BusinessInstaller, enabling constructor injection for all subsequent business service migrations**

## Performance

- **Duration:** 2 min
- **Started:** 2026-04-16T04:18:09Z
- **Completed:** 2026-04-16T04:19:49Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Added `services.AddSingleton<IDataContextFactory, DataContextFactory>()` to BusinessInstaller before all AddScoped registrations
- Added `using Negocio.Data;` import for DataContextFactory namespace resolution
- Confirmed Negocio.Business already references Negocio.Data -- no csproj change needed
- Verified solution builds with 0 errors

## Task Commits

Each task was committed atomically:

1. **Task 1: Add IDataContextFactory Singleton registration to BusinessInstaller** - `05e35f3` (feat)
2. **Task 2: Verify project references and build** - no commit (verification only, no code changes)

## Files Created/Modified
- `PortalNegocioWS/Installers/BusinessInstaller.cs` - Added Singleton DI registration for IDataContextFactory and Negocio.Data using directive

## Decisions Made
- Singleton lifetime confirmed correct for DataContextFactory: it holds only a cached connection string resolved at construction time, and creates fresh DataContext instances per `Create()` call
- No csproj modification needed since Negocio.Data was already referenced by Negocio.Business

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- IDataContextFactory is now available for injection in all business services
- Plans 03-03 through 03-07 can now inject IDataContextFactory into service constructors to replace direct `new PORTALNEGOCIODataContext()` calls
- No blockers identified

---
*Phase: 03-structural-foundation*
*Completed: 2026-04-16*
