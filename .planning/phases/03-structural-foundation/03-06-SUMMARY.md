---
phase: 03-structural-foundation
plan: 06
subsystem: database
tags: [datacontext, factory, migration, oracle, linqconnect]

# Dependency graph
requires:
  - phase: 03-structural-foundation
    provides: IDataContextFactory interface and DataContextFactory implementation
provides:
  - ProveedorBusiness fully migrated to IDataContextFactory (13 instantiations replaced)
  - ObtenerEmailxProveedor changed from static to instance method
  - NotificacionBusiness updated to accept ProveedorBusiness for email lookup calls
affects: [04-refactor-frontend, 05-api-contracts]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "IDataContextFactory injection for all DataContext creation"

key-files:
  created: []
  modified:
    - Negocio.Business/Proveedor/Proveedor.cs
    - Negocio.Business/Notificacion/Notificacion.cs

key-decisions:
  - "ProveedorBusiness.ProveedorBusiness optional param in NotificacionBusiness avoids breaking 6 inline new calls"
  - "ObtenerEmailxProveedor converted from static to instance method (IProveedor interface unchanged)"

patterns-established:
  - "Static methods on business services that create DataContext must be converted to instance methods when migrating to factory"

requirements-completed: [DAT-05]

# Metrics
duration: 3min
completed: 2026-04-16
---

# Phase 03 Plan 06: Migrate Proveedor.cs to IDataContextFactory Summary

**Proveedor.cs 13 DataContext instantiations replaced with factory pattern; static ObtenerEmailxProveedor converted to instance method**

## Performance

- **Duration:** 3 min
- **Started:** 2026-04-16T22:02:47Z
- **Completed:** 2026-04-16T22:05:52Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- ProveedorBusiness constructor now receives IDataContextFactory via DI
- All 13 `using (PORTALNEGOCIODataContext cx = new())` replaced with `using (var cx = _factory.Create())`
- All transaction boundaries (BeginTransaction/Commit/Rollback) preserved intact
- ObtenerEmailxProveedor converted from static to instance method
- NotificacionBusiness callers updated to use instance method via injected ProveedorBusiness

## Task Commits

Each task was committed atomically:

1. **Task 1: Add IDataContextFactory to ProveedorBusiness constructor** - `074c7f5` (feat)
2. **Task 2: Replace all 13 DataContext instantiations** - `0e0b531` (feat)

## Files Created/Modified
- `Negocio.Business/Proveedor/Proveedor.cs` - Added IDataContextFactory field/parameter, replaced 13 instantiations, removed static from ObtenerEmailxProveedor
- `Negocio.Business/Notificacion/Notificacion.cs` - Injected ProveedorBusiness (optional param), updated 3 call sites from static to instance

## Decisions Made
- Made ProveedorBusiness parameter optional (`ProveedorBusiness proveedor = null`) in NotificacionBusiness to avoid breaking 6 existing inline `new NotificacionBusiness(_utilidades)` calls in Login.cs, Proveedor.cs, and Preguntas.cs that don't need ObtenerEmailxProveedor
- Used concrete `ProveedorBusiness` type instead of `IProveedor` interface for the NotificacionBusiness field since `ObtenerEmailxProveedor` is not defined on the interface (it was previously a static-only method)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Static method accessing instance field after migration**
- **Found during:** Task 2 (Replace all DataContext instantiations)
- **Issue:** `ObtenerEmailxProveedor` was `public static` but after replacing `new PORTALNEGOCIODataContext()` with `_factory.Create()`, the instance field `_factory` cannot be accessed from a static method (CS0120)
- **Fix:** Removed `static` keyword from `ObtenerEmailxProveedor`, injected `ProveedorBusiness` into `NotificacionBusiness` (optional parameter), and updated 3 call sites from `ProveedorBusiness.ObtenerEmailxProveedor(...)` to `_proveedor.ObtenerEmailxProveedor(...)`
- **Files modified:** `Negocio.Business/Proveedor/Proveedor.cs`, `Negocio.Business/Notificacion/Notificacion.cs`
- **Verification:** Build succeeds with 0 errors; all 13 `_factory.Create()` calls present; zero direct DataContext instantiations
- **Committed in:** `0e0b531` (part of Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** Fix necessary for correctness after migration. Added 1 file (Notificacion.cs) outside plan scope but change is minimal and required to unblock the build.

## Issues Encountered
- The plan's transaction boundary map correctly identified all 13 instantiation sites. No additional instantiations were discovered.
- The static method issue was not anticipated in the plan but was straightforward to resolve.

## Next Phase Readiness
- Proveedor.cs is fully migrated -- the last large batch of direct DataContext instantiation is eliminated
- SolicitudCompra.cs (Plan 03-07) remains as the final migration target
- NotificacionBusiness still has its own `new PORTALNEGOCIODataContext()` calls at line 28 -- to be addressed in a future plan

---
*Phase: 03-structural-foundation*
*Completed: 2026-04-16*
