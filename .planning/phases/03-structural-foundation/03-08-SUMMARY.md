---
phase: 03-structural-foundation
plan: 08
subsystem: infra
tags: [data-context-factory, cron-jobs, di-registration, devart-linqconnect]

# Dependency graph
requires:
  - phase: 03-structural-foundation
    plan: "03-04"
    provides: "IDataContextFactory registration in DI; NotificacionBusiness already uses factory"
  - phase: 03-structural-foundation
    plan: "03-06"
    provides: "Medium-complexity services migrated to factory (Proveedor, Notificacion, etc.)"
  - phase: 03-structural-foundation
    plan: "03-07"
    provides: "Large services migrated (SolicitudCompra, Preguntas, etc.)"
provides:
  - "Zero new PORTALNEGOCIODataContext() in Negocio.Business/ and PortalNegocioWS/Services/"
  - "NotificacionActualizacionDatosJob registered with AddCronJob in Program.cs"
  - "ActualizarEstadoSolicitudJob and EnviarNotificacionInvitacionJob use IDataContextFactory"
affects: [04-api-consistency, 05-cleanup]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "IDataContextFactory injection in CronJobService subclasses"
    - "Dual-context read/write pattern preserved with factory.Create()"

key-files:
  created: []
  modified:
    - "PortalNegocioWS/Services/ActualizarEstadoSolicitudJob.cs"
    - "PortalNegocioWS/Services/EnviarNotificacionInvitacionJob.cs"
    - "PortalNegocioWS/Program.cs"

key-decisions:
  - "Injected IDataContextFactory via constructor in both cron jobs, following same pattern as all migrated business services"

patterns-established:
  - "CronJobService subclasses receive IDataContextFactory via constructor DI"

requirements-completed: [DAT-07, DAT-10]

# Metrics
duration: 3min
completed: 2026-04-16
---

# Phase 3 Plan 08: Migrate Cron Jobs to Factory Summary

**Two cron jobs (ActualizarEstadoSolicitudJob, EnviarNotificacionInvitacionJob) migrated to IDataContextFactory, and NotificacionActualizacionDatosJob registered in DI -- completing Phase 3 factory migration and fixing the unregistered job (DAT-07, DAT-10)**

## Performance

- **Duration:** 3 min
- **Started:** 2026-04-16T22:09:05Z
- **Completed:** 2026-04-16T22:12:00Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Migrated ActualizarEstadoSolicitudJob to use IDataContextFactory (dual-context read/write pattern preserved)
- Migrated EnviarNotificacionInvitacionJob to use IDataContextFactory alongside existing IServiceScopeFactory
- Registered NotificacionActualizacionDatosJob with AddCronJob in Program.cs using CronEnviarActualizacionDatos schedule
- Phase 3 Success Criterion #1 verified: zero `new PORTALNEGOCIODataContext()` across Negocio.Business/ and Services/

## Task Commits

Each task was committed atomically:

1. **Task 1: Migrate ActualizarEstadoSolicitudJob and EnviarNotificacionInvitacionJob to factory** - `424f9e5` (feat)
2. **Task 2: Register NotificacionActualizacionDatosJob in Program.cs -- then build and verify** - `ab67e9f` (feat)

## Files Created/Modified
- `PortalNegocioWS/Services/ActualizarEstadoSolicitudJob.cs` - Added IDataContextFactory field and constructor parameter; replaced 2 `new PORTALNEGOCIODataContext()` with `_factory.Create()`
- `PortalNegocioWS/Services/EnviarNotificacionInvitacionJob.cs` - Added IDataContextFactory field and constructor parameter; replaced 2 `new PORTALNEGOCIODataContext()` with `_factory.Create()`
- `PortalNegocioWS/Program.cs` - Added `AddCronJob<NotificacionActualizacionDatosJob>` registration with `CronEnviarActualizacionDatos` config key

## Decisions Made
None - followed plan as specified. Constructor injection of IDataContextFactory follows the established pattern from all prior migration plans.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 3 factory migration complete: zero direct DataContext instantiation in Negocio.Business/ and PortalNegocioWS/Services/
- NotificacionActualizacionDatosJob now runs on schedule (once per year on Jan 1, per `CronEnviarActualizacionDatos: "0 0 1 1 *"`)
- Ready for Phase 4 (API consistency) and Phase 5 (cleanup) which both depend on Phase 3 completion

---
*Phase: 03-structural-foundation*
*Completed: 2026-04-16*
