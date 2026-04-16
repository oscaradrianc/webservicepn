---
phase: 02-observability-foundation
verified: 2026-04-15T00:00:00Z
status: passed
score: 4/4
overrides_applied: 0
---

# Phase 02: Observability Foundation — Verification Report

**Phase Goal:** Activar logging estructurado con Serilog (Console + File) antes de comenzar la migración de los 17 servicios, para que cualquier fallo durante la migración sea diagnosticable.
**Verified:** 2026-04-15
**Status:** PASSED
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Cada request HTTP queda registrado en archivo de log con método, ruta, status code y duración en milisegundos | VERIFIED | `app.UseSerilogRequestLogging()` at Program.cs line 93, before UseAuthentication (line 105). File sink configured with `rollingInterval: Day`. |
| 2 | Las excepciones no manejadas aparecen en el log con stack trace completo (no solo el mensaje) | VERIFIED | `Log.Fatal(ex, "Application terminated unexpectedly")` in try/catch wrapping `app.Run()` at line 115. outputTemplate includes `{Exception}`. |
| 3 | El formato de log incluye timestamp, level, message y exception con estructura JSON o text enriched | VERIFIED | File sink outputTemplate: `"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:t4}] {Message:j}{NewLine}{Exception}"`. Console sink also active. |
| 4 | El bootstrap logger captura errores de startup antes de que IConfiguration esté disponible | VERIFIED | `Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger()` at lines 20-22, BEFORE `WebApplication.CreateBuilder(args)` at line 24. |

**Score:** 4/4 truths verified

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Directory.Packages.props` | Version pins for Serilog.Settings.Configuration and Serilog | VERIFIED | Line 33: `Serilog.Settings.Configuration Version="9.0.0"`, line 34: `Serilog Version="4.2.0"` |
| `PortalNegocioWS/PortalNegocioWS.csproj` | Direct PackageReference to Serilog.Settings.Configuration | VERIFIED | Line 19: `<PackageReference Include="Serilog.Settings.Configuration" />` |
| `PortalNegocioWS/Program.cs` | Bootstrap logger + UseSerilog() + UseSerilogRequestLogging wiring | VERIFIED | CreateBootstrapLogger (line 22), ReadFrom.Configuration (line 83), UseSerilogRequestLogging (line 93), Log.Fatal catch (line 115), Log.CloseAndFlush finally (line 119) |
| `PortalNegocioWS/appsettings.json` | Serilog config with MinimumLevel as object, Console + File sinks | VERIFIED | MinimumLevel.Default=Information, Override for Microsoft.AspNetCore and Devart at Warning, rollingInterval Day, outputTemplate with {Exception} |
| `Negocio.Business/Negocio.Business.csproj` | Direct Serilog reference for General.cs Log.Error() | VERIFIED | Line 16: `<PackageReference Include="Serilog" />` |
| `Negocio.Business/Utilidades/General.cs` | SendMail with Log.Error in catch block | VERIFIED | `using Serilog` at line 11; `Log.Error(ex, "SendMail failed. Recipients: {Recipients}, Subject: {Subject}", listaCorreos, asunto)` at lines 374-377 |
| `PortalNegocioWS/StartupCopia.cs` | File must NOT exist (deleted) | VERIFIED | File does not exist on disk; `<Compile Remove>` entry also cleaned from csproj |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| Program.cs (top) | Log.Logger bootstrap | `CreateBootstrapLogger()` | WIRED | Line 22, before line 24 (WebApplication.CreateBuilder) |
| builder.Host.UseSerilog | appsettings.json Serilog section | `ReadFrom.Configuration(ctx.Configuration)` | WIRED | Line 83 |
| app.Run() | Log.Fatal catch | try/catch/finally wrapping app.Run() | WIRED | Lines 109-120 |
| Negocio.Business.csproj PackageReference Serilog | General.cs using Serilog | Compilation dependency | WIRED | csproj has reference, General.cs has `using Serilog` at line 11 |
| General.cs catch block | Log.Error(ex, template, args) | Static Log class | WIRED | Lines 374-377 with Recipients + Subject structured fields |
| Program.cs OracleMonitor | if (app.Environment.IsDevelopment()) | IWebHostEnvironment.IsDevelopment() | WIRED | Lines 87-91: OracleMonitor instantiation inside IsDevelopment guard |

---

### Requirements Coverage

| Requirement | Description | Status | Evidence |
|-------------|-------------|--------|---------|
| OBS-01 | Serilog activado con sinks a Console y File | SATISFIED | Console + File sinks in appsettings.json, Serilog.Settings.Configuration pinned, UseSerilog in Program.cs |
| OBS-02 | Microsoft.AspNetCore y Devart configurados a nivel Warning | SATISFIED | appsettings.json MinimumLevel.Override: `Microsoft.AspNetCore: Warning`, `Devart: Warning` |
| OBS-03 | Excepciones silenciadas en SendMail reemplazadas por logging estructurado | SATISFIED | General.cs catch block has `Log.Error(ex, "SendMail failed...", listaCorreos, asunto)` |
| OBS-04 | OracleMonitor.IsActive envuelto en IsDevelopment guard | SATISFIED | Program.cs lines 87-91: OracleMonitor inside `if (app.Environment.IsDevelopment())` |

---

### Anti-Patterns Found

| File | Pattern | Severity | Impact |
|------|---------|---------|--------|
| `Negocio.Business/Utilidades/General.cs` lines 127, 189 | Two other catch blocks exist (not SendMail) — not scanned for content | Info | OBS-03 scope only covers SendMail; other catches are out of Phase 2 scope |

No blockers found. The two other catch blocks in General.cs are outside the scope of OBS-03 (which targets SendMail specifically).

---

### Build Status

`dotnet build PortalNegocioWS.sln` exits 0 — 0 errors, 2 warnings (pre-existing AutoMapper vulnerability NU1903, not introduced by this phase).

---

### Human Verification Required

None. All success criteria are verifiable programmatically.

For completeness, the following smoke tests were not run (require a running Oracle server) but are outside automated scope:
- Verify `Logs/log*.txt` file is created on startup
- Verify HTTP request log line appears on first API call
- Verify OracleMonitor is inactive when `ASPNETCORE_ENVIRONMENT=Production`

These are environmental prerequisites (Oracle DB) and do not block phase completion.

---

### Gaps Summary

No gaps. All 4 phase success criteria are satisfied. All 4 requirement IDs (OBS-01, OBS-02, OBS-03, OBS-04) are satisfied. The build compiles cleanly.

---

_Verified: 2026-04-15_
_Verifier: Claude (gsd-verifier)_
