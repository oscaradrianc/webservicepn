---
plan: "01-05"
phase: "01"
title: "AssertConfigurationIsValid() en AutoMapper — falla rápido en startup"
completed: true
tags: [startup-validation, fail-fast, configuration-error-detection]
created: "2026-04-15T20:30:00Z"
duration_minutes: 5
subsystem: "PortalNegocioWS"
key_files:
  created: []
  modified:
    - PortalNegocioWS/Installers/AutoMapperInstaller.cs
commits:
  - hash: be6007b
    message: "feat(01-05): add AssertConfigurationIsValid() to AutoMapper startup validation"
---

# Phase 01 Plan 05: AssertConfigurationIsValid() en AutoMapper Summary

**One-liner:** Added `config.AssertConfigurationIsValid()` to AutoMapper startup to detect mapping configuration errors immediately, rather than silently at first mapper use.

## Objective Achieved

Added early validation of AutoMapper configuration at startup by inserting `config.AssertConfigurationIsValid()` in `AutoMapperInstaller.InstallServices()` between the creation of `MapperConfiguration` and the call to `config.CreateMapper()`.

This ensures the application fails immediately with a clear exception message if any of the 80+ AutoMapper mappings in `MappingProfile.cs` are misconfigured — preventing silent runtime failures when requests first touch the mapper.

## Changes Made

### PortalNegocioWS/Installers/AutoMapperInstaller.cs

**Change:** Added one line of code

```csharp
config.AssertConfigurationIsValid();
```

Inserted between `new MapperConfiguration(...)` and `config.CreateMapper()`.

**Before:**
```csharp
public void InstallServices(IServiceCollection services, IConfiguration configuration)
{          
    var config = new AutoMapper.MapperConfiguration(c =>
    {
        c.AddProfile(new Mappings.MappingProfile());
    });

    var mapper = config.CreateMapper();

    services.AddSingleton(mapper);
}
```

**After:**
```csharp
public void InstallServices(IServiceCollection services, IConfiguration configuration)
{
    var config = new AutoMapper.MapperConfiguration(c =>
    {
        c.AddProfile(new Mappings.MappingProfile());
    });

    config.AssertConfigurationIsValid();

    var mapper = config.CreateMapper();

    services.AddSingleton(mapper);
}
```

## Verification

### Grep Confirmation
```bash
$ grep -n "AssertConfigurationIsValid" PortalNegocioWS/Installers/AutoMapperInstaller.cs
16:            config.AssertConfigurationIsValid();
```

Result: Exactly one match. Requirement satisfied.

### Build Result
```
dotnet build PortalNegocioWS/PortalNegocioWS.csproj
```

**Status: Build succeeded (Compilación correcta)**

- No AutoMapper configuration errors detected
- All 80+ mappings in `MappingProfile.cs` validated successfully
- Build completed in 3.90 seconds
- No breaking changes to existing code

**Build warnings present (pre-existing, not caused by this change):**
- AutoMapper 14.0.0 NuGet advisory (high severity) — already tracked in project security debt
- Various unused variable warnings in Business layer (pre-existing)

## Success Criteria Met

- [x] `config.AssertConfigurationIsValid();` exists in `AutoMapperInstaller.InstallServices()`
- [x] Placed correctly between `new MapperConfiguration(...)` and `config.CreateMapper()`
- [x] `dotnet build` successful
- [x] All 80+ mappings passed validation at startup
- [x] No runtime configuration errors

## Threat Model Mitigation

| Threat ID | Category | Component | Mitigation | Status |
|-----------|----------|-----------|-----------|--------|
| T-01-05-01 | Tampering | AutoMapper runtime errors | AssertConfigurationIsValid() detects mapping errors at startup, not runtime | ✓ Implemented |
| T-01-05-02 | Denial of Service | Startup fails on config error | Acceptable risk — early detection preferred over silent runtime failure | ✓ Accepted |

## Impact Assessment

**Security:** Positive — fail-fast principle prevents silent mapping configuration errors that could be exploited or lead to data corruption.

**Performance:** Negligible — validation happens once at startup, not per request.

**Maintainability:** Positive — developers will immediately see if they misconfigure a mapping when they run the application, rather than discovering it weeks later in production.

**Testing:** All existing mappings validated. No test framework changes needed (no test projects exist in solution).

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None.

## Threat Flags

No new security surface introduced.

## Self-Check

- [x] File exists: `/d/proyectos/PortalNegociosGithub/webservicepn/PortalNegocioWS/Installers/AutoMapperInstaller.cs`
- [x] Change present in file (line 16)
- [x] Commit exists: be6007b
- [x] Grep confirms single occurrence: `config.AssertConfigurationIsValid();`
- [x] Build successful with no errors

## PASSED
