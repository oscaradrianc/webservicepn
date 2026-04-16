---
phase: 02-observability-foundation
plan: "01"
subsystem: logging-infrastructure
tags: [serilog, bootstrap-logger, program-cs, packages]
dependency_graph:
  requires: []
  provides: [serilog-bootstrap-logger, serilog-request-logging, serilog-config-driven]
  affects: [PortalNegocioWS/Program.cs, Directory.Packages.props, PortalNegocioWS/PortalNegocioWS.csproj]
tech_stack:
  added: [Serilog.Settings.Configuration 9.0.0, Serilog 4.2.0 (pin)]
  patterns: [two-stage bootstrap logger, config-driven Serilog via ReadFrom.Configuration]
key_files:
  created: []
  modified:
    - PortalNegocioWS/Program.cs
    - Directory.Packages.props
    - PortalNegocioWS/PortalNegocioWS.csproj
decisions:
  - "UseSerilogRequestLogging placed before UseAuthentication so logging middleware captures request before auth middleware runs"
  - "OracleMonitor moved to post-Build but without IsDevelopment guard — guard is Plan 02-04's responsibility"
  - "Serilog core package (4.2.0) pinned in Directory.Packages.props but NOT added as direct PackageReference in PortalNegocioWS.csproj — arrives transitively from Serilog.AspNetCore"
metrics:
  duration_minutes: 10
  completed_date: "2026-04-15"
  tasks_completed: 2
  tasks_total: 2
  files_modified: 3
---

# Phase 02 Plan 01: Serilog Bootstrap Logger Summary

**One-liner:** Bootstrap logger two-stage pattern wired in Program.cs via CreateBootstrapLogger + UseSerilog(ReadFrom.Configuration) with request logging and fatal exception capture.

## What Was Built

Serilog plumbing instalado en Program.cs para capturar errores de startup (Oracle connection fail, config load fail) antes de que el host complete la inicializacion. El logger de dos etapas reemplaza el bootstrap logger con uno config-driven desde appsettings.json una vez que el host esta construido.

## Files Modified

### Directory.Packages.props
- Agregado: `<PackageVersion Include="Serilog.Settings.Configuration" Version="9.0.0" />`
- Agregado: `<PackageVersion Include="Serilog" Version="4.2.0" />`

### PortalNegocioWS/PortalNegocioWS.csproj
- Agregado: `<PackageReference Include="Serilog.Settings.Configuration" />`

### PortalNegocioWS/Program.cs
- **Bootstrap logger** insertado en lineas 19-22, ANTES de `WebApplication.CreateBuilder` (linea 24)
- **builder.Host.UseSerilog** con `ReadFrom.Configuration` agregado en linea 81-83, despues de los CronJob registrations y ANTES de `builder.Build()`
- **OracleMonitor** movido de pre-Build (linea 76-77 original) a post-Build (lineas 87-88), listo para guard IsDevelopment en Plan 02-04
- **app.UseSerilogRequestLogging()** insertado en linea 90, ANTES de UseAuthentication (linea 102)
- **try/catch/finally** wrapping app.Run() con Log.Fatal y Log.CloseAndFlush
- **Bloque comentado eliminado**: 42 lineas del patron IHostBuilder antiguo (lineas 98-139 originales)

## Key Positions in Final Program.cs

| Element | Line | Relative Position |
|---------|------|-------------------|
| CreateBootstrapLogger | 22 | BEFORE WebApplication.CreateBuilder (24) |
| builder.Host.UseSerilog | 81 | After CronJob registrations, before Build() |
| builder.Build() | 85 | Post-service registration |
| OracleMonitor | 87 | After Build(), before middleware |
| UseSerilogRequestLogging | 90 | Before UseAuthentication (102) |
| Log.Fatal catch | 112 | In catch of app.Run() wrap |
| Log.CloseAndFlush | 116 | In finally of app.Run() wrap |

## Build Result

`dotnet build PortalNegocioWS.sln` — **0 errores, 13 advertencias** (advertencias preexistentes, ninguna introducida por este plan).

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| Task 1 | eda86a8 | chore(02-01): pin Serilog.Settings.Configuration 9.0.0 and Serilog 4.2.0 in central packages |
| Task 2 | 0ba801c | feat(02-01): restructure Program.cs with two-stage Serilog bootstrap pattern |

## Deviations from Plan

None — plan ejecutado exactamente como estaba escrito.

## Known Stubs

None — no hay stubs o placeholders introducidos.

## Threat Flags

None — no hay nueva superficie de seguridad mas alla de lo documentado en el threat model del plan (T-02-01-01, T-02-01-02, T-02-01-03).

## Self-Check: PASSED

- [x] Directory.Packages.props contiene Serilog.Settings.Configuration 9.0.0
- [x] Directory.Packages.props contiene Serilog 4.2.0
- [x] PortalNegocioWS.csproj contiene referencia directa a Serilog.Settings.Configuration
- [x] Program.cs: CreateBootstrapLogger en linea 22 (antes de WebApplication.CreateBuilder linea 24)
- [x] Program.cs: ReadFrom.Configuration exactamente 1 resultado
- [x] Program.cs: UseSerilogRequestLogging linea 90 (antes de UseAuthentication linea 102)
- [x] Program.cs: OracleMonitor linea 87 (despues de builder.Build() linea 85)
- [x] Program.cs: Log.Fatal exactamente 1 resultado
- [x] Program.cs: Log.CloseAndFlush exactamente 1 resultado
- [x] Program.cs: public class Program = 0 (bloque comentado eliminado)
- [x] dotnet build exits 0
- [x] Commits eda86a8 y 0ba801c existen en git log
