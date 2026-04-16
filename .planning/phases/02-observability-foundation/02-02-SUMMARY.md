---
phase: 02-observability-foundation
plan: "02"
subsystem: logging-infrastructure
tags: [serilog, appsettings, config, log-levels, file-sink]
dependency_graph:
  requires: [serilog-bootstrap-logger]
  provides: [serilog-production-config, serilog-file-sink, serilog-namespace-filters]
  affects: [PortalNegocioWS/appsettings.json]
tech_stack:
  added: []
  patterns: [Serilog.Settings.Configuration JSON structure, namespace-prefix log filtering]
key_files:
  created: []
  modified:
    - PortalNegocioWS/appsettings.json
decisions:
  - "outputTemplate includes {Exception} to capture stack traces in log file"
  - "Devart filtered at Warning to prevent SQL query parameter logging"
  - "Using and Enrich keys omitted — auto-discovery works via .deps.json in .NET Core"
  - "path is Logs/log.txt (not Logs/log-.txt) matching existing log file naming convention"
metrics:
  duration_minutes: 5
  completed_date: "2026-04-15"
  tasks_completed: 1
  tasks_total: 1
  files_modified: 1
---

# Phase 02 Plan 02: Serilog appsettings Config Summary

**One-liner:** Replaced broken flat-string Serilog config with correct nested MinimumLevel object structure — Console and File sinks now active with namespace filters for Microsoft.AspNetCore and Devart at Warning.

## What Was Built

La seccion Serilog en appsettings.json tenia 4 errores estructurales que causaban que `ReadFrom.Configuration()` (instalado en Plan 02-01) ignorara los niveles y sinks definidos. Esta corrección activa la configuracion de produccion correcta.

**Errores corregidos:**

| Error | Antes | Despues |
|-------|-------|---------|
| MinimumLevel tipo | `"MinimumLevel": "Error"` (string plana) | `"MinimumLevel": { "Default": "Information", "Override": {...} }` |
| Override ubicacion | Al root level del objeto Serilog | Anidado dentro de MinimumLevel |
| Namespaces filtrados | `Microsoft` y `System` en Debug | `Microsoft.AspNetCore` y `Devart` en Warning |
| File sink incompleto | Solo `path` | Agrega `rollingInterval: Day` y `outputTemplate` con `{Exception}` |
| Using key invalida | `"Using": []` (vacio) | Omitida (auto-discovery) |
| Enrich key invalida | `"Enrich": [" ", ...]` (entrada blank invalida) | Omitida |

## Files Modified

### PortalNegocioWS/appsettings.json

**Seccion Serilog — estado final:**

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft.AspNetCore": "Warning",
      "Devart": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "Logs/log.txt",
        "rollingInterval": "Day",
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:t4}] {Message:j}{NewLine}{Exception}"
      }
    }
  ]
},
```

El resto del archivo (AllowedHosts, RedisCacheSettings, Settings, EncryptedKey, JWT, connectionStrings, baseStorage, Storage) no fue modificado.

## Build Result

`dotnet build PortalNegocioWS.sln` — **0 errores, 13 advertencias** (advertencias preexistentes, ninguna introducida por este plan).

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| Task 1 | 55b7263 | feat(02-02): replace broken Serilog config section with correct MinimumLevel object structure |

## Deviations from Plan

None — plan ejecutado exactamente como estaba escrito.

## Known Stubs

None.

## Threat Flags

None — no hay nueva superficie de seguridad mas alla de lo documentado en el threat model del plan (T-02-02-01, T-02-02-02, T-02-02-03). El outputTemplate con {Exception} esta aceptado explicitamente en el threat model.

## Self-Check: PASSED

- [x] dotnet build exits 0
- [x] `grep -A2 '"MinimumLevel"'` muestra `"Default": "Information"` (no string plana)
- [x] `grep '"Override"'` devuelve 1 resultado, anidado dentro de MinimumLevel
- [x] `grep '"Microsoft.AspNetCore"'` devuelve 1 resultado con valor "Warning"
- [x] `grep '"Devart"'` devuelve 1 resultado con valor "Warning"
- [x] `grep 'rollingInterval'` devuelve 1 resultado con valor "Day"
- [x] `grep '{Exception}'` devuelve 1 resultado en outputTemplate del File sink
- [x] `grep '"Using"'` devuelve 0 resultados
- [x] `grep '"Enrich"'` devuelve 0 resultados
- [x] `grep 'MinimumLevel.*Error'` devuelve 0 resultados
- [x] Commit 55b7263 existe en git log
