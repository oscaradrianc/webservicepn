---
phase: 02-observability-foundation
plan: "03"
subsystem: project-structure
tags: [cleanup, legacy, csproj]
dependency_graph:
  requires: [02-01]
  provides: []
  affects: [PortalNegocioWS/PortalNegocioWS.csproj]
tech_stack:
  added: []
  patterns: []
key_files:
  deleted:
    - PortalNegocioWS/StartupCopia.cs
  modified:
    - PortalNegocioWS/PortalNegocioWS.csproj
decisions:
  - "No content from StartupCopia.cs required migration — UseSerilogRequestLogging was already in Program.cs (Plan 02-01); all other middleware in StartupCopia.cs was not used in the current configuration by design"
metrics:
  duration: "< 5 min"
  completed: "2026-04-15"
  tasks_completed: 1
  files_changed: 2
---

# Phase 02 Plan 03: Remove StartupCopia.cs Summary

**One-liner:** Eliminado StartupCopia.cs (archivo legacy excluido de compilación) y limpiado el ItemGroup vacío del csproj.

## Pre-Check Result

`UseSerilogRequestLogging` confirmado en `Program.cs` antes de eliminar StartupCopia.cs:

```
app.UseSerilogRequestLogging(); // Registra método, ruta, status code y duración de cada request HTTP
```

Pre-check **PASÓ** — eliminación procedió con seguridad.

## Confirmación de Eliminación

- `git rm PortalNegocioWS/StartupCopia.cs` ejecutado — archivo eliminado del disco y del índice git
- `<Compile Remove="StartupCopia.cs" />` eliminado de PortalNegocioWS.csproj
- ItemGroup que quedó vacío también eliminado

## Contenido de StartupCopia.cs NO migrado a Program.cs

El archivo contenía los siguientes elementos que **no fueron migrados** (por diseño — no son necesarios en la configuración actual):

| Elemento | Razón para no migrar |
|----------|----------------------|
| `app.UseDeveloperExceptionPage()` | .NET 9 maneja esto automáticamente con `WebApplication.CreateBuilder` |
| `app.UseHttpsRedirection()` | Excluido por diseño en configuración actual (compatible con proxy reverso) |
| `app.UseResponseCompression()` | No activo en Program.cs actual — no forma parte del alcance de Phase 02 |
| Clase `Startup` completa | Patrón legacy `IWebHostBuilder` — reemplazado completamente por `WebApplication` en Program.cs |

Nada relevante fue perdido. El único método con valor real (`UseSerilogRequestLogging`) ya estaba en Program.cs desde Plan 02-01.

## Build Result

```
0 Errores
13 Advertencias (pre-existentes, ninguna relacionada con StartupCopia.cs)
Tiempo transcurrido 00:00:06.74
```

Build: **PASS**

## Deviations from Plan

None — plan ejecutado exactamente como fue escrito.

## Self-Check: PASSED

- StartupCopia.cs: DELETED (no existe en filesystem)
- csproj sin entrada `Compile Remove`: CONFIRMED (`grep "StartupCopia" PortalNegocioWS.csproj` = 0 resultados)
- `dotnet build` exits 0: CONFIRMED
- Commit 8f2c2fd trackeado en git: CONFIRMED
