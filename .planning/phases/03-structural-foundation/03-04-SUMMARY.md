---
plan: 03-04
phase: 03-structural-foundation
status: partial
commits: 2
---

# Plan 03-04: Migrate Medium-Complexity Services

## Objective
Migrate 10 remaining medium-complexity business services from direct DataContext instantiation to IDataContextFactory.

## Result

### Migrated (9 services)
- Preguntas/Preguntas.cs
- Consultas/ConsultasBusiness.cs
- Opcion/Opcion.cs
- Catalogo/Catalogo.cs
- Cotizacion/Cotizacion.cs
- Rol/Rol.cs
- Usuario/Usuario.cs
- ArchivoExcel/ArchivoExcel.cs
- Utilidades/General.cs

### Deferred (1 service)
- **Notificacion/Notificacion.cs** — Deferred to wave 5 alongside Proveedor.cs (plan 03-06). Reason: Proveedor.cs directly instantiates `new NotificacionBusiness(_utilidades)` at 4 locations. Adding IDataContextFactory to NotificacionBusiness's constructor would break Proveedor.cs before it is migrated. When 03-06 migrates Proveedor.cs, NotificacionBusiness can be migrated simultaneously and all callers updated in one commit.

## Key Changes
- All 9 migrated services now receive `IDataContextFactory` via constructor injection
- All `new PORTALNEGOCIODataContext()` replaced with `_factory.Create()`
- Transaction patterns (BeginTransaction/Commit/Rollback) preserved unchanged
- `using` block patterns preserved

## Deviations
- Notificacion.cs intentionally deferred (see above)
- Original plan specified 10 services in 2 batches; actual execution was split across agent (5 services) and orchestrator (4 services) with 1 deferred

## Build Status
0 errors, 11 warnings (pre-existing, unrelated to migration)

## Self-Check: PASSED
- [x] All 9 migrated files have zero `new PORTALNEGOCIODataContext`
- [x] All 9 files have `private readonly IDataContextFactory _factory`
- [x] Solution builds with 0 errors
