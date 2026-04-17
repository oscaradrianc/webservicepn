# Roadmap: Portal Negocios — Mitigación de Deuda Técnica

**Created:** 2026-04-14
**Milestone:** v1 — Deuda Técnica
**Core Value:** Cualquier desarrollador puede cambiar una regla de negocio tocando un único lugar, con confianza de que no rompe otra cosa.

## Overview

El refactor avanza en seis fases ordenadas por dependencia: primero cierran vulnerabilidades de seguridad activas sin tocar contratos del API, luego se activa logging estructurado para que las 17 migraciones de servicios sean diagnosticables, luego se inyecta IDataContextFactory servicio por servicio (de menor a mayor complejidad), luego se estandarizan los contratos HTTP con guía de migración Angular, luego se elimina código duplicado y se corrigen los background jobs, y finalmente se instala la red de seguridad de tests automatizados que blinda todo el refactor previo.

## Phases

- [ ] **Phase 1: Security Triage** - Cerrar vulnerabilidades de seguridad activas sin cambiar contratos del API
- [ ] **Phase 2: Observability Foundation** - Activar Serilog estructurado antes de cualquier migración estructural
- [ ] **Phase 3: Structural Foundation** - Inyectar IDataContextFactory en los 17 servicios de negocio
- [ ] **Phase 4: API Contract Standardization** - Corregir códigos HTTP, GlobalExceptionHandler, validación de input
- [ ] **Phase 5: Code Hygiene & Background Jobs** - Eliminar código duplicado, corregir threading de emails
- [ ] **Phase 6: Testing Safety Net** - Agregar tests automatizados que hacen seguros los cambios futuros

---

## Phase Details

### Phase 1: Security Triage

**Goal:** Cerrar las dos vulnerabilidades de seguridad activas en producción y agregar la validación de AutoMapper, sin ningún cambio en contratos del API ni coordinación Angular.
**Depends on:** Nothing (first phase)
**Requirements:** SEC-01, SEC-02, SEC-03, SEC-04, SEC-05, HYG-06, HYG-07
**Success Criteria** (what must be TRUE):
  1. Un request a cualquier endpoint `[Authorize]` sin token JWT devuelve HTTP 401 (verificable con curl o Postman)
  2. `CambiarClaveUsuario` guarda la contraseña encriptada — una contraseña cambiada puede usarse para hacer login exitosamente
  3. Los parámetros de `GetConstante` en `General.cs` y `Utilidades.cs` son parámetros de query, no string interpolation (visible en code review)
  4. La aplicación arranca con error explícito si alguno de los 80+ mappings de AutoMapper está mal configurado
  5. El JWT valida issuer y audience — un token de otro entorno es rechazado con 401
**Plans:** 5 plans

Plans:
- [x] 01-01: Agregar UseAuthentication + auditar getadjuntos
- [x] 01-02: Fix CambiarClaveUsuario — texto plano + throw e
- [x] 01-03: Parametrizar SQL raw en General.cs y Utilidades.cs
- [x] 01-04: Configurar ValidateIssuer y ValidateAudience en JWT
- [x] 01-05: AssertConfigurationIsValid() en AutoMapper

**Pre-work obligatorio antes de Plan 01-02:**
Ejecutar consulta de auditoría sobre `POGEUSUARIO.USUA_CLAVE`: contar filas por longitud de columna para distinguir texto plano vs. hashes SHA-512 (128 chars). Si hay passwords en texto plano en volumen, agregar plan de forced-reset campaign antes de cerrar la fase.

---

### Phase 2: Observability Foundation

**Goal:** Activar logging estructurado con Serilog (Console + File) antes de comenzar la migración de los 17 servicios, para que cualquier fallo durante la migración sea diagnosticable.
**Depends on:** Phase 1
**Requirements:** OBS-01, OBS-02, OBS-03, OBS-04
**Success Criteria** (what must be TRUE):
  1. Cada request HTTP queda registrado en archivo de log con método, ruta, status code y duración en milisegundos
  2. Las excepciones no manejadas aparecen en el log con stack trace completo (no solo el mensaje)
  3. Los errores de email (SMTP falla, destinatario inválido) aparecen en el log estructurado — dejan de ser silenciosos
  4. `OracleMonitor` solo está activo en el ambiente Development — en producción no genera output de diagnóstico Devart
  5. `StartupCopia.cs` está eliminado del repositorio (la configuración de Serilog fue extraída a `Program.cs` primero)
**Plans:** 4 plans

**Pre-work obligatorio antes de Plan 02-03:**
Leer `StartupCopia.cs` completo y copiar su configuración de Serilog (incluyendo connection string del Oracle sink si existe) a un archivo temporal ANTES de eliminarlo.

Plans:
- [x] 02-01: Agregar paquetes Serilog y activar bootstrap logging en Program.cs
- [x] 02-02: Configurar sinks Console + File con filtros de namespace
- [x] 02-03: Extraer config Serilog de StartupCopia.cs y eliminar archivo
- [x] 02-04: Corregir catch vacío en SendMail (ambas implementaciones) + OracleMonitor guard

---

### Phase 3: Structural Foundation

**Goal:** Inyectar IDataContextFactory en los 17 servicios de negocio y cron jobs, eliminando las 119 instanciaciones directas de DataContext, con LoginBusiness corregido y NotificacionActualizacionDatosJob registrado.
**Depends on:** Phase 2
**Requirements:** DAT-01, DAT-02, DAT-03, DAT-04, DAT-05, DAT-06, DAT-07, DAT-08, DAT-09, DAT-10
**Success Criteria** (what must be TRUE):
  1. Cero ocurrencias de `new PORTALNEGOCIODataContext()` en `Negocio.Business/` y `PortalNegocioWS/Services/` (verificable con grep)
  2. `IConfiguration` se inyecta por constructor en `LoginBusiness` — las firmas de `ILogin` y `Login.cs` no tienen `IConfiguration` como parámetro de método
  3. `ResetPassword` cierra la conexión Oracle correctamente (usa `using` block)
  4. `NotificacionActualizacionDatosJob` está registrado en DI y su `DoWork` se puede ejecutar en staging sin errores
  5. El API sigue procesando requests normalmente después de la migración (ningún endpoint regresa error que antes funcionaba)
**Plans:** 8 plans

**Pre-work obligatorio antes de Plan 03-06 y 03-07:**
Mapear todos los boundaries de transacciones en `Proveedor.cs` (1,240 líneas) y `SolicitudCompra.cs` (1,039 líneas): identificar cada bloque `BeginTransaction()` / `Commit()` / `Rollback()`, qué instancias de DataContext participan y si hay contextos anidados. Documentar el mapa antes de escribir tareas de migración para esos dos archivos.

Plans:
- [x] 03-01: Copiar IDataContextFactory y DataContextFactory del worktree a Negocio.Data/
- [x] 03-02: Registrar DataContextFactory como Singleton en BusinessInstaller
- [x] 03-03: Migrar los 5 servicios más pequeños al factory (validación del patrón)
- [x] 03-04: Migrar los 10 servicios restantes de complejidad media al factory
- [x] 03-05: Fix LoginBusiness — IConfiguration por constructor + using en ResetPassword
- [x] 03-06: Migrar Proveedor.cs al factory (requiere mapa de transacciones previo)
- [x] 03-07: Migrar SolicitudCompra.cs al factory (requiere mapa de transacciones previo)
- [x] 03-08: Migrar cron jobs al factory + registrar NotificacionActualizacionDatosJob

---

### Phase 4: API Contract Standardization

**Goal:** Corregir códigos HTTP (400/401/404/422/500), introducir GlobalExceptionHandler + ProblemDetails para rutas de error, agregar validación de input, y generar guía de migración Angular actualizada por controlador.
**Depends on:** Phase 3
**Requirements:** API-01, API-02, API-03, API-04, API-05, API-06, API-07, API-08, API-09
**Success Criteria** (what must be TRUE):
  1. Un error no manejado en cualquier endpoint devuelve ProblemDetails JSON con HTTP 500 — nunca un stack trace en texto plano
  2. Un request con body inválido (campo requerido ausente) devuelve HTTP 422 con detalle del campo, no HTTP 200 con mensaje de error embebido
  3. `LoginController` devuelve HTTP 401 cuando las credenciales son incorrectas (actualmente devuelve HTTP 200 con mensaje de error en el body)
  4. Las rutas de SUCCESS de todos los endpoints siguen devolviendo `Response<T>` con HTTP 200 — el contrato Angular de success paths no cambia
  5. La guía de migración Angular existe en `.planning/ANGULAR-MIGRATION.md` con una entrada por cada endpoint cuyo contrato de error cambió
**Plans:** 8 plans

**Constraint crítico:** `Response<T>` en rutas de SUCCESS no se toca — Angular depende de ese envelope. Solo las rutas de ERROR migran a ProblemDetails. `LoginController` retorna `Response<T>` directamente (no `IActionResult`) — cambiar esa firma es breaking change que requiere coordinación Angular.

**Nota Angular:** La guía de migración (API-09) es un documento vivo actualizado en cada plan de esta fase, no una tarea final.

Plans:
- [x] 04-01-PLAN.md — Implementar GlobalExceptionHandler + InvalidModelStateResponseFactory (HTTP 422) + AddProblemDetails()
- [x] 04-02-PLAN.md — Crear ApiControllerBase con helpers BusinessError() / NotFoundError() / Unauthorized()
- [x] 04-03-PLAN.md — Refactorizar LoginController para retornar IActionResult + actualizar guía Angular
- [x] 04-04-PLAN.md — Migrar 3 controladores prioritarios a ApiControllerBase con HTTP codes correctos + actualizar guía Angular
- [x] 04-05-PLAN.md — Migrar controladores restantes a ApiControllerBase + actualizar guía Angular
- [x] 04-06-PLAN.md — Agregar DataAnnotations ([Required], [MaxLength], [Range]) a modelos de request críticos
- [x] 04-07-PLAN.md — Migrar 7 SWNegocio.Controllers controllers a ApiControllerBase
- [x] 04-08-PLAN.md — Migrar 7 PortalNegocioWS.Controllers controllers a ApiControllerBase + finalizar ANGULAR-MIGRATION.md

**UI hint**: yes

---

### Phase 5: Code Hygiene & Background Jobs

**Goal:** Eliminar clases utilitarias duplicadas, consolidar AutoMapper profiles por dominio, reemplazar los 8 threads de email por un Channel-backed queue, y corregir el patrón dual-contexto en los cron jobs.
**Depends on:** Phase 3
**Requirements:** HYG-01, HYG-02, HYG-03, HYG-04, HYG-05, HYG-07, HYG-08, HYG-09, HYG-10, BGD-01, BGD-02, BGD-03
**Success Criteria** (what must be TRUE):
  1. No existen `General.cs` ni la clase estática `Utilidades` — todos los callers usan `IUtilidades` (verificable con grep de `using` statements y llamadas estáticas)
  2. Los 8 `new Thread(...)` de email reemplazados — los errores de envío aparecen en el log Serilog (ya activo desde Phase 2)
  3. El cron job `ActualizarEstadoSolicitudJob` usa un único DataContext por iteración, no dos
  4. `MappingProfile.cs` está dividido en perfiles por dominio bajo `PortalNegocioWS/Mappings/Profiles/`
  5. `GetAdjudicadoXSolicitud` devuelve datos reales de la adjudicación, no un objeto vacío
**Plans:** 7 plans

**Constraint de orden:** HYG-01 (consolidar Utilidades) debe completarse ANTES de BGD-02 (reemplazar threads), porque `BGD-02` depende de que `IUtilidades.SendMailAsync` exista como método async.

Plans:
- [x] 05-01-PLAN.md — Eliminar Utilidades.cs estático + remover SendMail de IUtilidades + HYG-10 hardcoded tipo identificacion
- [x] 05-02-PLAN.md — Verificar HYG-03 (StartupCopia.cs), HYG-04 (Program.cs dead blocks), HYG-07 (throw e)
- [x] 05-03-PLAN.md — Crear IEmailQueue + EmailMessage + EmailQueueService con Channel + IMemoryCache SMTP cache
- [x] 05-04-PLAN.md — Inyectar IEmailQueue en NotificacionBusiness + fix PreguntasBusiness (INotificacion)
- [x] 05-05-PLAN.md — Remover 10 new Thread(...) de Cotizacion, SolicitudCompra, UsuarioController
- [x] 05-06-PLAN.md — Fix dual-context en cron jobs (.ToList) + implementar GetAdjudicadoXSolicitud
- [x] 05-07-PLAN.md — Dividir MappingProfile.cs en 5 perfiles por dominio + actualizar AutoMapperInstaller

---

### Phase 6: Testing Safety Net

**Goal:** Instalar la red de seguridad de tests automatizados que hace seguros los cambios futuros: proyecto xUnit con WebApplicationFactory, tests de auth boundary, tests de operaciones críticas y validación de AutoMapper.
**Depends on:** Phase 3, Phase 4
**Requirements:** TST-01, TST-02, TST-03, TST-04, TST-05
**Success Criteria** (what must be TRUE):
  1. `dotnet test` ejecuta sin errores desde la raíz del repositorio
  2. Un request a un endpoint `[Authorize]` sin JWT en los tests de integración devuelve 401 (verifica que Phase 1 SEC-01 permanece en efecto)
  3. `AssertConfigurationIsValid()` ejecutado como test detecta cualquier mapping roto en los 80+ mappings de AutoMapper
  4. Los tests de `AutorizarSolicitud` y `RegistrarSolicitud` pasan — las operaciones más críticas tienen cobertura básica
  5. Los tests de cron jobs ejecutan `DoWork` sin lanzar excepción no manejada (usando mock factory de IDataContextFactory)
**Plans:** 5 plans

Plans:
- [x] 06-01-PLAN.md — Crear PortalNegocioWS.Tests: csproj (Sdk.Web), sln add, CustomWebApplicationFactory, JwtTokenHelper, public partial class Program
- [ ] 06-02-PLAN.md — Tests de auth boundary: 401 sin token, no-401 con token, login 200/401 con mock ILogin
- [ ] 06-03-PLAN.md — Tests para AutorizarSolicitud y RegistrarSolicitud con mock ISolicitudCompra
- [ ] 06-04-PLAN.md — Test AssertConfigurationIsValid() para los 80+ AutoMapper mappings (pure unit test)
- [ ] 06-05-PLAN.md — Fix try/catch en ActualizarEstadoSolicitudJob.DoWork + tests para 3 cron jobs con mock factory

---

## Progress

**Execution Order:** 1 → 2 → 3 → 4 → 5 → 6
Note: Phase 5 can begin in parallel with Phase 4 once Phase 3 is complete, since Phase 5 depends on Phase 3, not Phase 4.

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Security Triage | 0/5 | Not started | - |
| 2. Observability Foundation | 0/4 | Not started | - |
| 3. Structural Foundation | 0/8 | Not started | - |
| 4. API Contract Standardization | 0/8 | Not started | - |
| 5. Code Hygiene & Background Jobs | 0/7 | Not started | - |
| 6. Testing Safety Net | 0/5 | Not started | - |

---
*Roadmap created: 2026-04-14*
