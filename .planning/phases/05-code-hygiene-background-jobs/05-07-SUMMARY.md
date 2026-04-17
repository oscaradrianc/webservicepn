---
phase: 05-code-hygiene-background-jobs
plan: "07"
subsystem: automapper-mapping-profiles
tags: [automapper, profiles, refactoring, hygiene]
dependency_graph:
  requires: []
  provides: [domain-mapping-profiles, assembly-scan-automapper]
  affects: [PortalNegocioWS/Installers/AutoMapperInstaller.cs]
tech_stack:
  added: []
  patterns: [AutoMapper assembly scanning via AddMaps(Assembly)]
key_files:
  created:
    - PortalNegocioWS/Mappings/Profiles/AuthProfile.cs
    - PortalNegocioWS/Mappings/Profiles/CatalogoProfile.cs
    - PortalNegocioWS/Mappings/Profiles/ProveedorProfile.cs
    - PortalNegocioWS/Mappings/Profiles/AdministracionProfile.cs
    - PortalNegocioWS/Mappings/Profiles/NotificacionProfile.cs
  modified:
    - PortalNegocioWS/Installers/AutoMapperInstaller.cs
  deleted:
    - PortalNegocioWS/Mappings/MappingProfile.cs
decisions:
  - AddMaps(typeof(AutoMapperInstaller).Assembly) chosen over explicit profile registration — auto-discovers all Profile subclasses in the assembly without requiring installer updates when new profiles are added
metrics:
  duration_minutes: 5
  completed_date: "2026-04-17T05:49:25Z"
  tasks_completed: 2
  tasks_total: 2
  files_created: 5
  files_modified: 1
  files_deleted: 1
---

# Phase 05 Plan 07: AutoMapper Profile Split Summary

**One-liner:** Split 108-line monolithic MappingProfile.cs into 5 domain profiles (Auth, Catalogo, Proveedor, Administracion, Notificacion) with AutoMapper assembly-scan auto-discovery replacing explicit profile registration.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Create 5 domain profile files and delete MappingProfile.cs | 535523c | Profiles/AuthProfile.cs, CatalogoProfile.cs, ProveedorProfile.cs, AdministracionProfile.cs, NotificacionProfile.cs; deleted MappingProfile.cs |
| 2 | Update AutoMapperInstaller to use AddMaps(assembly) + verify AssertConfigurationIsValid | a6f75fa | AutoMapperInstaller.cs |

## What Was Built

MappingProfile.cs (108 lines, 10 mappings across 5 domains) was split into domain-specific Profile classes:

- **AuthProfile.cs** — POGEUSUARIO→Usuario (18 ForMember), POGEROL→Rol (5 ForMember), POGEOPCIONXROL→OpcionxRol (4 ForMember)
- **CatalogoProfile.cs** — PONECATALOGO→Catalogo (8 ForMember), POGECLASE→Clases (3 ForMember), POGECLASEVALOR→ClaseValor (7 ForMember)
- **ProveedorProfile.cs** — FPROVEEDORESREGISTRADOSMEResult→ProveedoresPorMes (3 ForMember)
- **AdministracionProfile.cs** — POGEAUTORIZADORGERENCIA→AutorizadorGerencia (5 ForMember), POGECONSTANTE→Constante (6 ForMember)
- **NotificacionProfile.cs** — PONENOTICIA→Noticias (7 ForMember)

AutoMapperInstaller now uses `c.AddMaps(typeof(AutoMapperInstaller).Assembly)` — future profiles added to the `Mappings/Profiles/` directory are automatically discovered without any installer changes. `AssertConfigurationIsValid()` remains active and validates all 10 mappings at startup.

## Verification Results

- 5 profile files confirmed in `PortalNegocioWS/Mappings/Profiles/`
- MappingProfile.cs confirmed deleted
- `grep -r "CreateMap<" PortalNegocioWS/Mappings/Profiles/ | wc -l` = 10
- `grep "AddMaps" PortalNegocioWS/Installers/AutoMapperInstaller.cs` — match found
- `grep "AddProfile.*MappingProfile" PortalNegocioWS/Installers/AutoMapperInstaller.cs` — no matches
- `grep "AssertConfigurationIsValid" PortalNegocioWS/Installers/AutoMapperInstaller.cs` — match found
- `dotnet build PortalNegocioWS.sln` — 0 errors, build succeeded

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

None.

## Threat Flags

None. The `AssertConfigurationIsValid()` guard (T-05-07-01 mitigation) was already present and remains active. The POGEUSUARIO→Usuario mapping including Clave is unchanged from the prior implementation (T-05-07-02 accepted).

## Self-Check: PASSED

- PortalNegocioWS/Mappings/Profiles/AuthProfile.cs: FOUND
- PortalNegocioWS/Mappings/Profiles/CatalogoProfile.cs: FOUND
- PortalNegocioWS/Mappings/Profiles/ProveedorProfile.cs: FOUND
- PortalNegocioWS/Mappings/Profiles/AdministracionProfile.cs: FOUND
- PortalNegocioWS/Mappings/Profiles/NotificacionProfile.cs: FOUND
- PortalNegocioWS/Mappings/MappingProfile.cs: CONFIRMED DELETED
- Commit 535523c: FOUND
- Commit a6f75fa: FOUND
