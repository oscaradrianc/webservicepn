# Project Research Summary

**Project:** Portal Negocios - Technical Debt Mitigation
**Domain:** Brownfield ASP.NET Core 9 REST API (procurement management)
**Researched:** 2026-04-14
**Confidence:** HIGH

## Executive Summary

Portal Negocios is a live procurement API with an Angular client and zero automated tests. The codebase has accrued six categories of debt, two of which are active security vulnerabilities in production right now, not code quality issues. The dominant pattern in all four research documents is the same: do safety-net work first, structural work second, and contract-breaking changes last. The foundation for everything else is the IDataContextFactory abstraction, which already exists as a scaffold in .worktrees/mejoras-etapas/ and requires no new NuGet packages. Every other improvement -- testability, cron job cleanup, dual-context fixes -- is blocked until the 119 direct new PORTALNEGOCIODataContext() calls are replaced service by service.

The two active security vulnerabilities require immediate, isolated commits before any structural work begins. First: app.UseAuthentication() is missing from the middleware pipeline -- [Authorize] attributes are not enforced in production. This is a single-line fix with no API contract impact. Second: UsuarioBusiness.CambiarClaveUsuario stores new passwords in plaintext (line 272 of Usuario.cs). Utilidades.cs is excluded from compilation and the double-hash in General.cs is the only active path, so fixing the hash algorithm without a soft-migration strategy will lock out 100% of users. Fix the plaintext storage bug first; defer hash normalization until a DB audit and soft-migration plan are in place.

The API contract standardization (HTTP status codes, ProblemDetails, input validation) is the direct unlocker for paused Angular features, but requires Angular coordination for every changed endpoint. All three researchers agree on the staged hybrid approach: errors migrate to ProblemDetails with correct status codes; success paths keep the Response<T> envelope that Angular already consumes. No third-party ProblemDetails packages are needed -- ASP.NET Core 9 provides IProblemDetailsService and IExceptionHandler natively. FluentValidation.AspNetCore must NOT be installed (deprecated in v12); use FluentValidation and FluentValidation.DependencyInjectionExtensions only if DataAnnotations prove insufficient.

---

## Key Findings

### Recommended Stack

The existing stack stays unchanged. All additions are zero-package pure C# work or activation of already-declared dependencies.

**Net-new NuGet packages required:**

| Package | Version | Purpose | Phase |
|---------|---------|---------|-------|
| Serilog.Sinks.File | 7.0.0 | Rolling file logs | 2 |
| Serilog.Sinks.Console | 6.1.1 | Structured console | 2 |
| Serilog.Settings.Configuration | 10.0.0 | appsettings.json-driven config | 2 |
| Serilog.Enrichers.Environment | 3.0.1 | MachineName + EnvironmentName | 2 |
| FluentValidation | 12.1.1 | Complex validation rules | 4 (if needed) |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 | Validator scanning | 4 (if needed) |

**Zero-package additions (BCL or ASP.NET Core 9 SDK):**
- IDataContextFactory -- pure C# interface in Negocio.Data
- IExceptionHandler + GlobalExceptionHandler -- Microsoft.AspNetCore.Mvc.Core (already referenced)
- AddProblemDetails() / UseExceptionHandler() -- Microsoft.AspNetCore.Mvc.Core (already referenced)
- Channel<T>-backed IBackgroundEmailQueue -- System.Threading.Channels in .NET 9 BCL
- DataAnnotations validation -- System.ComponentModel.DataAnnotations in .NET 9 BCL

**Do not install:** FluentValidation.AspNetCore (deprecated v12), Hellang.Middleware.ProblemDetails (superseded by native ASP.NET Core 9), Hangfire/Quartz.NET (overkill for fire-and-forget email), MediatR (requires restructuring 17 services).

### Expected Features

**Table stakes (must fix -- system is insecure or broken without these):**
- Add app.UseAuthentication() to middleware pipeline -- active security hole; [Authorize] not enforced in production
- Parameterize raw SQL in General.cs line 187 and Utilidades.cs line 70 -- SQL injection vectors
- Fix CambiarClaveUsuario plaintext password storage (line 272, Usuario.cs) -- active data integrity bug
- Introduce IDataContextFactory and inject into all 17 business services -- structural root cause blocking testability
- Standardize HTTP status codes + ProblemDetails error responses -- direct unlocker for paused Angular features
- Register NotificacionActualizacionDatosJob -- supplier notifications silently inactive in production
- Inject IConfiguration via constructor in LoginBusiness -- inconsistent with every other service
- Delete dead code (StartupCopia.cs, commented Program.cs block) -- after extracting Serilog config first

**Should have (differentiators):**
- Structured logging via Serilog (declared dependency -- 4 new sink packages to activate)
- Input validation via DataAnnotations on request models (zero packages)
- Channel<T>-backed background email queue (replaces 8 new Thread() fire-and-forget calls)
- AssertConfigurationIsValid() on AutoMapper (80+ mappings with no startup validation -- early win, Phase 1)
- xUnit test project with WebApplicationFactory fixture (requires IDataContextFactory first)
- IMemoryCache for SMTP config (6 Oracle queries per email, easy win after utility consolidation)

**Defer to later cycle:**
- SHA-512 hash algorithm normalization -- requires DB audit and soft-migration strategy
- GetAdjudicadoXSolicitud implementation -- Angular adapted to empty response; treat as a feature release
- Redis cache activation -- infrastructure decision outside this cycle
- Full Response<T> removal from success paths -- optional Stage C, not this cycle

### Architecture Approach

Five concrete architectural components were validated across all four research documents. They compose in dependency order: the global exception handler and UseAuthentication() fix are purely additive (no dependencies); IConfiguration injection in LoginBusiness is a 3-file isolated change; IDataContextFactory is the structural foundation everything else builds on; AutoMapper profile splitting is mechanical and independent; Response<T> evolution depends on the global exception handler being live first.

**Major components to add or modify:**

1. GlobalExceptionHandler (IExceptionHandler) in PortalNegocioWS/Infrastructure/ -- replaces scattered per-controller try/catch; register as Singleton; only inject Singleton dependencies
2. IDataContextFactory / DataContextFactory in Negocio.Data/ -- worktree scaffold is the correct shape; register as Singleton; contexts remain using-scoped per method
3. IBackgroundEmailQueue / BackgroundEmailQueue / EmailQueueProcessor in Negocio.Business/ -- Channel-backed; bounded capacity 100; replaces 8 new Thread() calls
4. ApiControllerBase in PortalNegocioWS/Infrastructure/ -- controller base with BusinessError() and NotFoundError() helpers
5. AutoMapper profile split -- one Profile subclass per domain under PortalNegocioWS/Mappings/Profiles/; AutoMapperInstaller switches to cfg.AddMaps() + AssertConfigurationIsValid()

**Critical interface constraint:** The IDataContextFactory interface shape from the worktree (Create() returning PORTALNEGOCIODataContext) is final. Do not add CreateWithSharedConnection() or other overloads. Do not register PORTALNEGOCIODataContext itself in the DI container.

### Critical Pitfalls

1. **SHA-512 fix locks out all users** -- Utilidades.cs does not compile (Compile Remove in .csproj); the double-hash in General.cs CreateSHA512 is the only active path; ALL stored passwords are double-hashed SHA-512. Changing to single-hash with no migration invalidates every password in the DB. Additionally, CambiarClaveUsuario stores plaintext -- those users already have broken passwords. Audit USUACLAVE column length distribution first; fix the plaintext bug separately in Phase 1; defer hash normalization to a forced-reset campaign.

2. **IDataContextFactory in Proveedor.cs breaks transaction atomicity** -- Proveedor.cs (1,240 lines) opens nested DataContext instances inside transaction scopes. factory.Create() returns a new connection each time; Oracle connection pooling does not auto-enlist inner connections in outer transactions. Map every transaction boundary before migrating this file. SolicitudCompra.cs (1,039 lines) has the same pattern. Migrate these two last, alone, with integration tests in place.

3. **HTTP status code changes break Angular silently** -- Angular HttpClient routes non-2xx to .error(), not .next(). Call sites using .subscribe(data => ...) without an error handler silently swallow errors. Change one endpoint at a time; audit Angular call sites before each change; use a temporary Angular interceptor as adapter during transition.

4. **Dead code deletion loses the only Serilog reference** -- StartupCopia.cs contains the only fully-wired Serilog configuration in the codebase including the Oracle sink connection string. Extract Serilog config into Program.cs first, then delete StartupCopia.cs. Separately: Utilidades.cs must be deleted entirely once its methods are consolidated -- leaving the file present without Compile Remove causes namespace conflicts.

5. **app.UseAuthentication() missing is a security hole, not a code smell** -- [Authorize] endpoints may be publicly accessible in production today. Highest-urgency single-line fix. Add it before any other work, in its own commit, then verify a request without a JWT returns 401.

---

## Implications for Roadmap

### Phase 1: Security Triage
**Rationale:** Two active vulnerabilities with zero API contract impact. One- or two-line fixes deployable immediately with no Angular coordination and no regression risk. AssertConfigurationIsValid() on AutoMapper is also an early win with no dependencies.
**Delivers:** Authentication middleware enforced; SQL injection vectors closed; plaintext password storage stopped; AutoMapper regressions caught at startup.
**Addresses:** UseAuthentication() fix, SQL parameterization (General.cs/Utilidades.cs), CambiarClaveUsuario plaintext fix, AssertConfigurationIsValid() on AutoMapper, throw e to throw; in CambiarClaveUsuario.
**Avoids:** Pitfall 5 (auth hole masked by subsequent improvements), Pitfall 1 (never touch hash algorithm before plaintext is fixed and DB audited).

### Phase 2: Observability Foundation
**Rationale:** Activate Serilog before touching anything fragile. Structured logging must be live before the IDataContextFactory rollout begins -- otherwise failures across 17 service migrations are invisible. Also prerequisite for safe deletion of StartupCopia.cs.
**Delivers:** HTTP request logging with duration and status codes; structured exception logging; rolling file sink for production diagnostics; OracleMonitor conditional guard.
**Addresses:** Serilog activation (4 new packages), OracleMonitor conditional guard.
**Avoids:** Pitfall 4 (extract Serilog config from StartupCopia.cs before deleting the file).

### Phase 3: Structural Foundation (IDataContextFactory)
**Rationale:** The factory abstraction is the prerequisite for testability, cron job DI cleanup, and dual-context fixes. Migrate service by service, smallest to largest. Proveedor.cs and SolicitudCompra.cs require transaction boundary mapping before migration and must be done last, alone.
**Delivers:** All 17 business services using injectable IDataContextFactory; LoginBusiness IConfiguration injection fix; NotificacionActualizacionDatosJob registered (dry-run first); dead code deleted.
**Addresses:** IDataContextFactory scaffold from worktree, IConfiguration constructor injection, cron job DI registration, StartupCopia.cs deletion.
**Avoids:** Pitfall 3 (transaction atomicity in Proveedor.cs -- migrate last with transaction boundary map), Pitfall 6 (change all three ILogin methods in one commit), Pitfall 9 (dry-run notification job before activating).
**Research flag:** Transaction boundary mapping in Proveedor.cs and SolicitudCompra.cs is a prerequisite task for the final sub-phase. Inspect these files in full before writing those tasks.

### Phase 4: API Contract Standardization
**Rationale:** HTTP status code corrections are the direct unlocker for paused Angular features. GlobalExceptionHandler is purely additive first; per-controller try/catch removal and status code corrections are incremental, one controller at a time.
**Delivers:** Correct HTTP status codes (200/400/401/404/422/500); GlobalExceptionHandler replacing scattered try/catch; ApiControllerBase with consistent error helpers; DataAnnotations input validation; Angular migration guide per changed endpoint.
**Addresses:** GlobalExceptionHandler + AddProblemDetails(), HTTP status code corrections, DataAnnotations on request models, ApiControllerBase, Angular migration guides.
**Avoids:** Pitfall 2 (audit Angular call sites before each endpoint change), AP2 (never remove Response<T> from success paths -- only error paths migrate).
**Research flag:** Angular codebase not inspected during research. Call site count per endpoint unknown. Treat Phase 4 estimates as provisional; audit first, then scope tasks.

### Phase 5: Hygiene and Background Jobs
**Rationale:** With observability live (Phase 2) and factory abstraction in place (Phase 3), address remaining reliability issues. Email thread fixes gain useful logging only after Serilog is active. Dual-context cron job refactor is safe only after IDataContextFactory is in place.
**Delivers:** Channel-backed background email queue replacing 8 new Thread() calls; dual-context cron job loops fixed; SMTP config cached or moved to appsettings.json; AutoMapper profiles split by domain.
**Addresses:** Fire-and-forget thread replacement, dual-context cron job fix, SMTP config caching, AutoMapper profile split.
**Avoids:** Pitfall 7 (never await the email task at the call site -- keep the fire-and-forget pattern), Pitfall 10 (read all IDs into a list before opening write context in cron loops).

### Phase 6: Testing Safety Net
**Rationale:** WebApplicationFactory with mock IDataContextFactory only works after Phase 3 completes. Testing is sequenced correctly, not deprioritized. AssertConfigurationIsValid() was promoted to Phase 1 as a dependency-free early win.
**Delivers:** PortalNegocioWS.Tests xUnit project; WebApplicationFactory<Program> fixture; smoke tests for auth boundary (401 on unauthorized endpoints); business logic tests for SolicitudCompra and Proveedor.
**Addresses:** xUnit project setup, Microsoft.AspNetCore.Mvc.Testing, integration test scaffolding.

### Phase Ordering Rationale

- Security fixes have no dependencies and close active vulnerabilities -- they go first so no other work proceeds under broken security assumptions.
- Observability goes before structural migration because silent failures during IDataContextFactory rollout across 17 services are hardest to diagnose without structured logging.
- IDataContextFactory is the structural prerequisite for the testing safety net, cron job cleanup, and the Angular feature unblock -- it gates three subsequent phases.
- API contract changes require Angular coordination; they go after the factory migration so new 401 responses do not surprise Angular callers who were previously getting unintentional 200s.
- Testing goes last because WebApplicationFactory mock pattern requires DI-injectable factories; the one dependency-free test was promoted to Phase 1.

### Research Flags

Phases needing additional attention during task planning:
- **Phase 3, Proveedor.cs and SolicitudCompra.cs sub-phase:** Transaction boundary map is a prerequisite not yet produced. Do not write tasks for these two files until the boundaries are documented.
- **Phase 4, Angular migration guide:** Angular codebase not inspected during research. Call site count per endpoint unknown. Treat estimates as provisional; audit first.
- **Phase 1, DB audit:** Distribution of plaintext vs. double-hashed SHA-512 passwords in USUACLAVE unknown. Run the column length audit before closing Phase 1.

Phases with standard patterns (planning proceeds without deeper research):
- **Phase 1** (UseAuthentication, SQL parameterization, plaintext fix): Trivially documented one- to two-line changes.
- **Phase 2** (Serilog activation): Packages declared; StartupCopia.cs is the configuration reference; four packages with verified versions.
- **Phase 3** (IDataContextFactory for simple services): Worktree scaffold is the implementation; mechanical search-replace.
- **Phase 5** (Channel-based queue): Complete implementation specified in STACK.md; zero new packages.

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All packages verified on nuget.org; FluentValidation.AspNetCore deprecation confirmed in official upgrade guide |
| Features | HIGH | Grounded in direct code inspection with specific file and line number references throughout |
| Architecture | HIGH | Patterns verified against official ASP.NET Core 9 docs; worktree scaffold confirmed correct; component designs are implementation-ready |
| Pitfalls | HIGH | Every pitfall grounded in observed code behavior; Utilidades.cs compilation exclusion confirmed in .csproj |

**Overall confidence:** HIGH

### Gaps to Address

- **SHA-512 DB audit:** Distribution of password formats in POGEUSUARIO.USUA_CLAVE is unknown. Run the column length query at the start of Phase 1. If plaintext entries exist in volume, a forced-reset campaign may need to be added to Phase 1 scope.
- **Angular codebase scope:** Angular client not inspected during research. Number of call sites per API endpoint unknown. Phase 4 estimates must include time for Angular call site auditing before writing per-endpoint migration tasks.
- **Proveedor.cs transaction depth:** Identified as 1,240 lines with nested transaction patterns; full boundary map not produced. This map is a prerequisite task for Phase 3 Proveedor.cs sub-phase.
- **NotificacionActualizacionDatosJob business logic:** The job has never fired in production. DoWork must be reviewed to understand what emails it sends and to whom before registering. Staging dry-run or never-execute cron schedule is mandatory.

---

## Sources

### Primary (HIGH confidence)
- Microsoft Learn -- ASP.NET Core 9: error handling, IExceptionHandler, ProblemDetails, hosted services, DI lifetimes
- FluentValidation 12.0 official upgrade guide -- deprecation of FluentValidation.AspNetCore confirmed
- AutoMapper official docs -- AssertConfigurationIsValid(), cfg.AddMaps()
- nuget.org -- all package versions verified
- Direct code inspection: General.cs, Utilidades.cs, Login.cs, Usuario.cs, SolicitudCompra.cs, Proveedor.cs, Program.cs, MappingProfile.cs, AutoMapperInstaller.cs, BusinessInstaller.cs, SolicitudController.cs, Negocio.Business.csproj, .worktrees/mejoras-etapas/Negocio.Data/

### Secondary (MEDIUM confidence)
- Serilog ASP.NET Core GitHub -- activation pattern and sink configuration
- Milan Jovanovic -- ProblemDetails for ASP.NET Core APIs; Serilog best practices
- Atomic Object -- IDbContextFactory for unit testing (pattern confirmed applicable to LinqConnect factory analogy)

---
*Research completed: 2026-04-14*
*Ready for roadmap: yes*