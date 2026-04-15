# Portal Negocios — Mitigación de Deuda Técnica

## What This Is

Refactor incremental del API REST ASP.NET Core 9.0 de Portal Negocios (gestión de licitaciones, cotizaciones y proveedores). El objetivo es eliminar la deuda técnica acumulada —119 instanciaciones directas de DataContext, respuestas HTTP inconsistentes, código duplicado y seguridad débil— sin interrumpir el sistema en producción. Todo cambio en el contrato del API viene acompañado de una guía de migración para el cliente Angular.

## Core Value

Cualquier desarrollador puede cambiar una regla de negocio tocando un único lugar, con confianza de que no rompe otra cosa.

## Requirements

### Validated

- ✓ JWT Bearer auth con HMAC-SHA256 — existente
- ✓ Arquitectura N-Tier 4 capas (API → Business → Data + Models) — existente
- ✓ Patrón `Response<T>` + `ResponseStatus` como envelope — existente (mal implementado, candidato a estandarizar)
- ✓ Oracle DB con Devart LinqConnect ORM — existente
- ✓ Background jobs via `CronJobService` (`IHostedService`) — existente
- ✓ AutoMapper para mapeo DB entity → DTO — existente
- ✓ Installer-based DI por reflexión (`IInstaller`) — existente
- ✓ Almacenamiento local de archivos via `IStorageService` — existente

### Active

- [ ] Introducir `IDataContextFactory` e inyectarlo en los 17 servicios de negocio (eliminar 119 `new PORTALNEGOCIODataContext()` directos)
- [ ] Estandarizar respuestas del API: códigos HTTP correctos (200/400/401/404/500), envelope `Response<T>` uniforme, mensajes de error descriptivos
- [ ] Producir guía de migración Angular para cada endpoint cuyo contrato cambie
- [ ] Consolidar clases utilitarias duplicadas (`General.cs` vs `Utilidades.cs`) — bug activo: doble-hash SHA-512 divergente
- [ ] Inyectar `IConfiguration` vía constructor en `LoginBusiness` (no como parámetro de método)
- [ ] Reemplazar `new Thread(...)` fire-and-forget para emails por `Task.Run` con logging y manejo de errores
- [ ] Añadir `app.UseAuthentication()` al pipeline de middleware (actualmente ausente — `[Authorize]` no funciona correctamente)
- [ ] Registrar `NotificacionActualizacionDatosJob` en el contenedor DI (actualmente definido pero nunca ejecutado)
- [ ] Parametrizar las consultas SQL raw con interpolación de strings (vulnerabilidad SQL injection en `General.cs` y `Utilidades.cs`)
- [ ] Agregar logging estructurado (Serilog) para reemplazar el bloque comentado en `Program.cs`
- [ ] Eliminar código muerto: `StartupCopia.cs`, bloque comentado en `Program.cs` (líneas 97–138)
- [ ] Documentar convenciones del proyecto en `CONVENTIONS.md` para prevenir nueva deuda
- [ ] Validación de input en modelos de request (`[Required]`, `[MaxLength]`, verificación de `ModelState`)
- [ ] Añadir `AssertConfigurationIsValid()` al perfil de AutoMapper (80+ mappings sin validación)

### Out of Scope

- Cambios en el esquema Oracle — el modelo de datos no se toca
- Reemplazo del ORM (LinqConnect → EF Core) — migración masiva de 119 usos, riesgo demasiado alto para este ciclo
- Nuevas features de negocio — primero estabilizar, luego crecer
- Redis cache — infraestructura muerta que requiere decisión de arquitectura separada; se limpia el dead code pero no se implementa
- Mobile / frontend — solo se entrega guía de migración Angular, no se modifica el cliente

## Context

El sistema gestiona el ciclo completo de compras institucionales: solicitudes, invitaciones a proveedores, cotizaciones y adjudicaciones. La BD Oracle y el ORM Devart LinqConnect son restricciones duras — el `DataContext.Designer.cs` se genera automáticamente desde el `.lqml` y no se edita a mano.

**Estado actual del codebase:**
- `SolicitudCompra.cs`: 1,039 líneas — clase de negocio más grande y más frágil
- `Proveedor.cs`: 1,240 líneas — transacciones anidadas complejas
- 119 instanciaciones directas de DataContext en 17 servicios de negocio
- 0 tests automatizados en toda la solución
- Serilog parcialmente configurado pero completamente comentado
- `NotificacionActualizacionDatosJob` definido pero nunca registrado (notificaciones silenciosamente inactivas)
- `GetAdjudicadoXSolicitud` siempre devuelve objeto vacío (TODO sin implementar)
- Hay features en pausa porque el código actual es demasiado riesgoso de cambiar

**Worktree previo:** `.worktrees/mejoras-etapas/` contiene un intento anterior con `IDataContextFactory` scaffoldeado — revisarlo como punto de partida.

## Constraints

- **Operacional**: El API debe seguir funcionando durante todo el refactor — no hay ventana de mantenimiento larga disponible
- **Oracle**: Esquema y tablas no cambian — todo el trabajo es en la capa de aplicación
- **Angular**: Cada cambio en contrato del API (ruta, estructura de respuesta, códigos HTTP) debe documentarse en la guía de migración
- **LinqConnect**: El `DataContext.Designer.cs` es auto-generado — no se modifica manualmente; la abstracción se introduce por encima
- **Sin tests de partida**: El refactor avanza creando la red de seguridad mientras trabaja (no puede depender de tests preexistentes)

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Refactor incremental, no reescritura | El API está en producción con features pausadas — no hay tiempo para una reescritura | — Pending |
| Introducir `IDataContextFactory` antes de romper instanciaciones directas | El worktree previo ya tiene el scaffold; minimiza el diff de cambio | — Pending |
| Contratos del API sí pueden cambiar, con guía de migración | Fueron "mal implementados" según el usuario — corregirlos es parte del objetivo | — Pending |
| No reemplazar LinqConnect en este ciclo | 119 usos — demasiado riesgo y alcance para deuda técnica v1 | — Pending |

---

## Evolution

Este documento evoluciona en cada transición de fase y al cerrar milestones.

**Después de cada fase** (via `/gsd-transition`):
1. ¿Requisitos invalidados? → Mover a Out of Scope con razón
2. ¿Requisitos validados? → Mover a Validated con referencia de fase
3. ¿Nuevos requisitos emergieron? → Agregar a Active
4. ¿Decisiones que registrar? → Agregar a Key Decisions
5. ¿"What This Is" sigue siendo preciso? → Actualizar si cambió

**Después de cada milestone** (via `/gsd-complete-milestone`):
1. Revisión completa de todas las secciones
2. ¿Core Value sigue siendo la prioridad correcta?
3. ¿Los Out of Scope siguen siendo válidos?
4. Actualizar Context con el estado actual

---
*Last updated: 2026-04-14 after initialization*
