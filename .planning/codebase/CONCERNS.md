# Codebase Concerns

**Analysis Date:** 2026-04-14

---

## Security Considerations

**JWT Issuer and Audience Not Validated:**
- Risk: Any token signed with the same key but for a different service/environment is accepted.
- Files: `PortalNegocioWS/Installers/AuthenticationInstaller.cs` (lines 27–28)
- Current mitigation: Signing key is validated (`ValidateIssuerSigningKey = true`).
- Recommendations: Set `ValidateIssuer = true`, `ValidateAudience = true`, and configure `ValidIssuer` and `ValidAudience`.

**HTTPS Disabled in JWT and HTTP Middleware:**
- Risk: Credentials and tokens transmitted in plaintext if deployed without a reverse proxy enforcing TLS.
- Files: `PortalNegocioWS/Installers/AuthenticationInstaller.cs` (line 21 `RequireHttpsMetadata = false`), `PortalNegocioWS/Program.cs` (line 91 `//app.UseHttpsRedirection();`)
- Current mitigation: Assumed to be behind a TLS-terminating proxy in production.
- Recommendations: Enable `RequireHttpsMetadata = true` and uncomment `UseHttpsRedirection()` or document explicitly that TLS is enforced at the proxy.

**`app.UseAuthentication()` Missing from Middleware Pipeline:**
- Risk: JWT authentication middleware is registered but never activated. The `[Authorize]` attribute on controllers falls back to the default behavior, which may silently allow unauthenticated access depending on the ASP.NET Core version.
- Files: `PortalNegocioWS/Program.cs` (lines 89–93 — only `UseAuthorization` is called, `UseAuthentication` is absent)
- Current mitigation: None detected.
- Recommendations: Add `app.UseAuthentication()` before `app.UseAuthorization()`.

**SQL Injection via Raw Query String Interpolation:**
- Risk: The `constante` parameter value is concatenated directly into an Oracle SQL string without parameterization.
- Files: `Negocio.Business/Utilidades/General.cs` (line 187), `Negocio.Business/Utilidades/Utilidades.cs` (line 70)
- Vulnerable pattern: `cx.ExecuteQuery<string>(string.Format("SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", constante))`
- Current mitigation: The `constante` value is typically an internal string literal, reducing exploitation likelihood.
- Recommendations: Replace with parameterized queries using `ExecuteQuery<T>(sql, param)` or an ORM-safe alternative.

**Hardcoded Identification Type for Legal Entities:**
- Risk: Business logic silently defaults identification type to `4` for legal entities with no configuration escape.
- Files: `Negocio.Business/Proveedor/Proveedor.cs` (lines 74, 255)
- Comment: `//TODO: Revisar para que no quede quemado`
- Recommendations: Retrieve from database catalog (`POGECLASEVALORs`) or application configuration.

**Unauthenticated Endpoints Exposing Sensitive Data:**
- Risk: `[AllowAnonymous]` is applied to endpoints that serve non-trivial business data (solicitud invitations, solicitud attachments).
- Files: `PortalNegocioWS/Controllers/SolicitudController.cs` (lines 153, 210 — `listinvitacion`, `getadjuntos`), `PortalNegocioWS/Controllers/ProveedorController.cs` (line 34 — `registrar`)
- Current mitigation: The invitation list endpoint is likely intentionally public; `registrar` for a new supplier registration is reasonable. Attachments warrant review.
- Recommendations: Audit `getadjuntos` — attachment files may contain commercial-sensitive documents that should require authentication.

**Email Credentials Stored in Database as Plain Constants:**
- Risk: SMTP credentials (`pwd_mail`) are retrieved from the `POGE_CONSTANTE` table, readable to any user with DB access or via the SQL injection vector above.
- Files: `Negocio.Business/Utilidades/General.cs` (lines 330–341), `Negocio.Business/Utilidades/Utilidades.cs` (lines 85–90)
- Recommendations: Move SMTP credentials to `appsettings.json` or environment variables, protected by the ASP.NET Core secrets manager.

---

## Tech Debt

**Duplicate Utility Classes (`General.cs` vs `Utilidades.cs`):**
- Issue: Two classes implement the same static utility methods (`GetStringEncriptado`, `GetSecuencia`, `GetConstante`, `SendMail`, `ConvertirMensaje`, `DecodificarArchivo`, `ObtenerBlob`). Both are in active use.
- Files: `Negocio.Business/Utilidades/General.cs` (523 lines), `Negocio.Business/Utilidades/Utilidades.cs` (182 lines)
- Impact: Bug fixes or changes must be applied in two places; divergence has already occurred (SHA-512 double-hash in `General.cs` vs single-hash in `Utilidades.cs`).
- Fix approach: Consolidate into the instance-based `UtilidadesBusiness` (which implements `IUtilidades`). Remove the `static Utilidades` class.

**SHA-512 Double-Hash Password Divergence:**
- Issue: `UtilidadesBusiness.CreateSHA512` (in `General.cs`) hashes the data twice (`alg.ComputeHash(alg.ComputeHash(hashValue))`). The legacy `Utilidades.GetStringEncriptado` (in `Utilidades.cs`) hashes once. Passwords encrypted by different code paths will not match.
- Files: `Negocio.Business/Utilidades/General.cs` (lines 282–287), `Negocio.Business/Utilidades/Utilidades.cs` (lines 23–34)
- Impact: Login may silently fail for users whose password was hashed with the older implementation, or vice versa.
- Fix approach: Standardize on a single deterministic hash function, migrate existing passwords if needed.

**DataContext Instantiated Directly Without Injection (119 occurrences):**
- Issue: `new PORTALNEGOCIODataContext()` is called directly in all 17 business service classes. Connection string comes from the machine's configuration rather than a DI-provided factory.
- Files: All files in `Negocio.Business/` — highest density in `Negocio.Business/Solicitud/SolicitudCompra.cs` (19 occurrences), `Negocio.Business/Proveedor/Proveedor.cs` (13 occurrences), `Negocio.Business/Cotizacion/Cotizacion.cs` (12 occurrences)
- Impact: Cannot swap connections for testing, cannot control connection pooling, prevents unit testing of business logic.
- Fix approach: Introduce `IDataContextFactory` (already exists in worktree at `.worktrees/mejoras-etapas/Negocio.Data/DataContextFactory.cs`), inject it, and replace direct instantiation.

**`IConfiguration` Passed as Method Parameter Instead of Injected:**
- Issue: `IConfiguration` is threaded through method signatures in the login business layer instead of being injected in the constructor.
- Files: `Negocio.Business/Login/ILogin.cs` (lines 11–13), `Negocio.Business/Login/Login.cs` (lines 32, 220, 287)
- Impact: Callers must obtain and pass `IConfiguration` manually; inconsistent with the rest of the layer.
- Fix approach: Inject `IConfiguration` via the `LoginBusiness` constructor.

**`IConfiguration` Not Disposed in `ResetPassword`:**
- Issue: `PORTALNEGOCIODataContext cx` is instantiated without a `using` block in `ResetPassword`, meaning the connection is not explicitly closed on the normal or exception path.
- Files: `Negocio.Business/Login/Login.cs` (line 290)
- Fix approach: Wrap in a `using` statement.

**Fire-and-Forget Threads for Email Notifications:**
- Issue: 8 separate `new Thread(...)` calls spawn background threads for email without error handling, structured concurrency, or cancellation.
- Files: `Negocio.Business/Solicitud/SolicitudCompra.cs`, `Negocio.Business/Cotizacion/Cotizacion.cs`, `Negocio.Business/Preguntas/Preguntas.cs`
- Impact: Notification failures are silent; threads cannot be monitored, cancelled, or retried; thread-pool starvation risk under load.
- Fix approach: Use `Task.Run` with proper async/await and logging, or a background queue via `IHostedService`.

**Silent Email Failure Swallows Exceptions:**
- Issue: `catch (Exception ex) { }` in both `SendMail` implementations discards all errors with no logging.
- Files: `Negocio.Business/Utilidades/Utilidades.cs` (lines 122–125), `Negocio.Business/Utilidades/General.cs` (lines 373–376)
- Impact: Email delivery failures are invisible. Operational support cannot detect broken SMTP configurations.
- Fix approach: At minimum, log the exception with `ILogger`. Consider re-throwing after logging.

**Dual Two-Context Pattern in Cron Jobs:**
- Issue: `ActualizarEstadoSolicitudJob` and `EnviarNotificacionInvitacionJob` open two separate `PORTALNEGOCIODataContext` instances — one to read, one to write — within the same loop iteration.
- Files: `PortalNegocioWS/Services/ActualizarEstadoSolicitudJob.cs` (lines 33–63), `PortalNegocioWS/Services/EnviarNotificacionInvitacionJob.cs` (lines 43–80)
- Impact: Read context may return stale data relative to write context; unnecessary Oracle connections held open during loop iteration.
- Fix approach: Use a single context or read IDs into a list before opening the write context.

**`StartupCopia.cs` — Dead Production Code Committed to Repo:**
- Issue: A full duplicate `Startup` class with commented-out Serilog wiring exists alongside the active `Program.cs` top-level statements.
- Files: `PortalNegocioWS/StartupCopia.cs` (102 lines)
- Impact: Creates confusion about which startup code is active; may confuse future contributors.
- Fix approach: Delete the file.

**`Program.cs` Contains Large Commented-Out Code Block:**
- Issue: The old `Program` class with `Main`, `CreateHostBuilder`, and Serilog sink wiring is retained as a comment from lines 97–138.
- Files: `PortalNegocioWS/Program.cs` (lines 97–138)
- Fix approach: Remove commented-out code; history is preserved in git.

**`NotificacionActualizacionDatosJob` Defined but Never Registered:**
- Issue: The job class exists but is not added to the DI container in `Program.cs`.
- Files: `PortalNegocioWS/Services/NotificacionActualizacionDatosJob.cs`, `PortalNegocioWS/Program.cs`
- Impact: The supplier data-update notification is silently not running.
- Fix approach: Register with `builder.Services.AddCronJob<NotificacionActualizacionDatosJob>(...)` and confirm the cron schedule in configuration.

**`GetAdjudicadoXSolicitud` Returns Empty Object:**
- Issue: The query result selects into an `Adjudicacion` with all properties commented out, always returning an empty object.
- Files: `Negocio.Business/Cotizacion/Cotizacion.cs` (lines 465–481)
- Comment: `//TODO: Revisar ya que la fecha de adjudicacion debe salir del historco`
- Impact: Consumers of this endpoint receive an empty adjudication record.
- Fix approach: Implement correct logic mapping `ADJUFECHA`, `COTICOTIZACION`, and supplier name from the join.

**`FichasTecnicas` N+1 BLOB Query:**
- Issue: `ObtenerListaFichasTecnicas` runs one DB query per document to fetch its BLOB data inside a `foreach` loop.
- Files: `Negocio.Business/Cotizacion/Cotizacion.cs` (lines 484–520)
- Impact: Latency and Oracle round-trips scale linearly with the number of documents per quotation. Quotations with many attachments will be slow.
- Fix approach: Fetch BLOBs in a single query joining on `BLOBBLOB` IDs collected from the first query.

---

## Fragile Areas

**`SolicitudCompra.cs` — 1,039 Lines Single Business Class:**
- Files: `Negocio.Business/Solicitud/SolicitudCompra.cs`
- Why fragile: Handles registration, update, authorization, listing, attachment loading, and status queries in one class. High coupling makes isolated changes risky.
- Safe modification: Add new methods without touching existing ones; write integration test against DB before changing `RegistrarSolicitud` or `AutorizarSolicitud`.
- Test coverage: None.

**`Proveedor.cs` — 1,240 Lines with Complex Nested Transactions:**
- Files: `Negocio.Business/Proveedor/Proveedor.cs`
- Why fragile: Supplier registration spans many tables inside a manually managed transaction. Any new field addition risks breaking the commit sequence.
- Safe modification: Open a transaction, test rollback paths explicitly. Keep new fields at the end of the `InsertOnSubmit` block to avoid merge conflicts.
- Test coverage: None.

**`MappingProfile.cs` — 80+ AutoMapper Mappings in a Single Class:**
- Files: `PortalNegocioWS/Mappings/MappingProfile.cs` (108 lines)
- Why fragile: Adding or renaming an entity column risks silently breaking any mapping that references it. AutoMapper mismatches are typically runtime errors, not compile-time.
- Safe modification: Use `AssertConfigurationIsValid()` in a startup check or test.
- Test coverage: None.

---

## Performance Bottlenecks

**Mail Configuration Read from DB on Every Email Send:**
- Problem: `GetConstante` executes six separate Oracle queries to retrieve SMTP server, port, username, password, sender, and SSL flag each time an email is sent.
- Files: `Negocio.Business/Utilidades/General.cs` (lines 336–341), `Negocio.Business/Utilidades/Utilidades.cs` (lines 85–90)
- Cause: No caching of mail configuration.
- Improvement path: Cache constants in `IMemoryCache` with a short TTL, or move SMTP config to `appsettings.json`.

**`OracleMonitor` Active in All Environments:**
- Problem: `OracleMonitor.IsActive = true` is set unconditionally in `Program.cs` at startup.
- Files: `PortalNegocioWS/Program.cs` (lines 76–77)
- Cause: Diagnostic listener enabled without an environment guard.
- Improvement path: Wrap in `if (app.Environment.IsDevelopment())` or remove if not actively using Devart's monitoring tool.

---

## Dependencies at Risk

**Devart LinqConnect (Proprietary ORM):**
- Risk: Devart is a commercial vendor with license costs and a smaller community than EF Core. The auto-generated `DataContext.Designer.cs` cannot be manually modified and is tied to the `.lqml` file format. Migration to another ORM is a large-scale refactor.
- Impact: All 119 direct `PORTALNEGOCIODataContext` instantiations would need to change.
- Migration plan: Introduce `IDataContextFactory` abstraction first (already scaffolded in the `mejoras-etapas` worktree), then incrementally migrate per-domain.

**S3 and Remote Storage — Incomplete Implementations:**
- Risk: `S3StorageService` and `RemoteStorageService` both throw `NotImplementedException` for all interface methods. They cannot be activated without completing the implementation.
- Files: `Negocio.Business/Utilidades/S3StorageService.cs`, `Negocio.Business/Utilidades/RemoteStorageService.cs`
- Impact: Any attempt to configure `Storage:Type = "S3"` or `"Remote"` will cause a runtime `NotImplementedException` on first file operation.
- Migration plan: Complete the S3 implementation using the commented-out code in `S3StorageService.cs` as a starting point; fix `RemoteStorageService` to implement `IStorageService` (currently does not).

**Redis Cache — Dead Infrastructure:**
- Risk: `CacheInstaller.cs` is entirely commented out. The Redis dependency was integrated but never activated.
- Files: `PortalNegocioWS/Installers/CacheInstaller.cs`
- Impact: No request-level caching is active. High-frequency catalog/constant queries hit Oracle directly on every call.
- Migration plan: Either remove the dead installer and its dependencies, or complete the implementation and enable it.

---

## Missing Critical Features

**No Automated Tests:**
- Problem: Zero test projects exist in the solution. No unit tests, integration tests, or contract tests.
- Blocks: Safe refactoring, regression detection, and CI gating.

**No Structured Logging in Production:**
- Problem: Serilog is a dependency and was partially wired in the now-dead `StartupCopia.cs`. `Program.cs` does not configure any structured log sink. Cron jobs use `ILogger<T>`, but there is no persistence target for those logs.
- Files: `PortalNegocioWS/Program.cs` (lines 97–138 — commented-out Serilog configuration), `PortalNegocioWS/StartupCopia.cs`
- Blocks: Operational visibility into background job failures, email errors, and business exceptions.

**No Input Validation on Request Models:**
- Problem: No `[Required]`, `[MaxLength]`, or `[Range]` annotations are present on any model in `Negocio.Model/`. `ModelState.IsValid` is not checked in any controller.
- Impact: Malformed or oversized input reaches business layer and Oracle without sanitization. Out-of-range values can cause silent truncation or DB constraint errors surfaced as raw exception messages.

**Business Day Calculation for Solicitud Close Date:**
- Problem: The close date (`SOCOFECHACIERRE`) is set to `null` on insert. The business-day calculation is explicitly deferred.
- Files: `Negocio.Business/Solicitud/SolicitudCompra.cs` (line 74)
- Comment: `//TODO Realizar logica para dias habiles`
- Blocks: Solicitud records have no defined expiry, so the daily status-update cron job cannot close them on schedule.

---

## Test Coverage Gaps

**All Business Logic Untested:**
- What's not tested: Every method in `Negocio.Business/` — registration, authorization, quotation, login, password change, notifications.
- Files: Entire `Negocio.Business/` directory.
- Risk: Any refactoring of the data-access or business logic layers can break production silently.
- Priority: High.

**Cron Jobs Untested:**
- What's not tested: `ActualizarEstadoSolicitudJob.DoWork`, `EnviarNotificacionInvitacionJob.DoWork`, `NotificacionActualizacionDatosJob.DoWork`.
- Files: `PortalNegocioWS/Services/`
- Risk: State transition logic (Publicado → Cerrado) could mis-close solicitudes if the date comparison logic changes.
- Priority: High.

**AutoMapper Profile Untested:**
- What's not tested: 80+ mappings in `MappingProfile.cs` are not validated with `AssertConfigurationIsValid()`.
- Files: `PortalNegocioWS/Mappings/MappingProfile.cs`
- Risk: Entity property renames silently produce incorrect or null-mapped fields at runtime.
- Priority: Medium.

---

*Concerns audit: 2026-04-14*
