---
phase: 02-observability-foundation
plan: "04"
subsystem: logging-observability
tags: [serilog, general-cs, oracle-monitor, send-mail]
dependency_graph:
  requires: [02-01, 02-02, 02-03]
  provides: [sendmail-log-error, oracle-monitor-dev-guard, negocio-business-serilog-ref]
  affects:
    - Negocio.Business/Negocio.Business.csproj
    - Negocio.Business/Utilidades/General.cs
    - PortalNegocioWS/Program.cs
tech_stack:
  added: []
  patterns: [structured-log-error, environment-guard]
key_files:
  created: []
  modified:
    - Negocio.Business/Negocio.Business.csproj
    - Negocio.Business/Utilidades/General.cs
    - PortalNegocioWS/Program.cs
decisions:
  - "Log.Error uses static Log class (Serilog) — picks up the global Log.Logger configured in Program.cs bootstrap; no-op SilentLogger before host init"
  - "OracleMonitor wrapped in its own IsDevelopment block separate from the Swagger block for clarity"
  - "Utilidades.cs NOT modified — excluded from compilation via <Compile Remove> in csproj, General.cs is the only active SendMail implementation"
metrics:
  duration_minutes: 6
  completed_date: "2026-04-15"
  tasks_completed: 2
  tasks_total: 2
  files_modified: 3
---

# Phase 02 Plan 04: SendMail Log.Error + OracleMonitor Guard Summary

**One-liner:** Silent email failure catch replaced with structured Log.Error in General.cs, and OracleMonitor activation wrapped in IsDevelopment guard in Program.cs.

## What Was Built

Dos correcciones de observabilidad finales para completar Phase 2:

1. **General.cs SendMail catch**: El catch vacio que silenciaba todos los fallos SMTP ahora emite `Log.Error(ex, "SendMail failed. Recipients: {Recipients}, Subject: {Subject}", listaCorreos, asunto)` usando el logger estatico de Serilog.

2. **OracleMonitor IsDevelopment guard**: El bloque `OracleMonitor myMonitor = new OracleMonitor(); myMonitor.IsActive = true;` esta ahora envuelto en `if (app.Environment.IsDevelopment())` para que el diagnostico Devart no se active en produccion.

3. **Negocio.Business.csproj**: Agregada referencia directa a `<PackageReference Include="Serilog" />` (sin version — viene de Directory.Packages.props pin 4.2.0) para que General.cs pueda usar `using Serilog`.

## Files Modified

### Negocio.Business/Negocio.Business.csproj
- Agregado: `<PackageReference Include="Serilog" />` en el ItemGroup de PackageReferences

### Negocio.Business/Utilidades/General.cs
- Agregado: `using Serilog;` despues de `using System.Threading.Tasks;`
- **Catch block antes:**
  ```csharp
  catch (Exception ex)
  {
      
  }
  ```
- **Catch block despues:**
  ```csharp
  catch (Exception ex)
  {
      Log.Error(ex,
          "SendMail failed. Recipients: {Recipients}, Subject: {Subject}",
          listaCorreos,
          asunto);
  }
  ```

### PortalNegocioWS/Program.cs
- **OracleMonitor antes** (lineas 87-88, post Build(), sin guard):
  ```csharp
  OracleMonitor myMonitor = new OracleMonitor();
  myMonitor.IsActive = true;
  ```
- **OracleMonitor despues** (envuelto en IsDevelopment):
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      OracleMonitor myMonitor = new OracleMonitor();
      myMonitor.IsActive = true;
  }
  ```

## Build Result

`dotnet build PortalNegocioWS.sln` — **0 errores, 12 advertencias** (advertencias preexistentes, ninguna introducida por este plan).

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| Task 1 | d0dfe23 | chore(02-04): add Serilog PackageReference to Negocio.Business.csproj |
| Task 2 | b143459 | feat(02-04): add Log.Error to SendMail catch and wrap OracleMonitor with IsDevelopment guard |

## Phase 2 Completion Status

| Criterio | Estado |
|---------|--------|
| 1. Cada request HTTP en Logs/log*.txt (UseSerilogRequestLogging activo) | CUMPLIDO (Plan 02-01) |
| 2. Excepciones no manejadas con stack trace (Log.Fatal + {Exception} outputTemplate) | CUMPLIDO (Plan 02-01) |
| 3. Errores SMTP como Log.Error en log estructurado (General.cs fix) | CUMPLIDO (este plan) |
| 4. OracleMonitor solo activo en Development (IsDevelopment guard) | CUMPLIDO (este plan) |
| 5. StartupCopia.cs eliminado del repositorio | CUMPLIDO (Plan 02-03) |

**Phase 2 COMPLETA — los 5 criterios son TRUE.**

## Deviations from Plan

None — plan ejecutado exactamente como estaba escrito.

## Known Stubs

None.

## Threat Flags

None — las amenazas T-02-04-01 a T-02-04-04 fueron evaluadas durante el plan. T-02-04-03 (OracleMonitor sin guard en produccion) fue mitigada por este plan con el guard IsDevelopment.

## Self-Check: PASSED

- [x] Negocio.Business.csproj contiene `<PackageReference Include="Serilog" />`
- [x] Directory.Packages.props contiene `Serilog Version="4.2.0"` (pin del Plan 02-01, sin duplicar)
- [x] General.cs contiene `using Serilog;`
- [x] General.cs contiene `Log.Error(ex,` en el catch de SendMail
- [x] General.cs template tiene `{Recipients}` y `{Subject}`
- [x] Program.cs tiene OracleMonitor dentro de `if (app.Environment.IsDevelopment())`
- [x] Negocio.Business.csproj sigue con `<Compile Remove="Utilidades\Utilidades.cs" />` (Utilidades.cs no tocado)
- [x] dotnet build exits 0
- [x] Commits d0dfe23 y b143459 existen en git log
