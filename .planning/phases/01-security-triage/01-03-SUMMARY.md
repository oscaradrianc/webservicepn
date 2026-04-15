---
phase: 01
plan: "01-03"
title: "Parametrizar SQL raw en General.cs y Utilidades.cs (SQL injection)"
date_completed: "2026-04-15"
status: completed
requirements: [SEC-03]
---

# Phase 01 Plan 03: SQL Injection Mitigation in GetConstante Summary

**Objective:** Eliminate two SQL injection vulnerabilities via string interpolation in `GetConstante` methods by replacing raw ExecuteQuery calls with parameterized LINQ queries.

## Tasks Completed

### Task 01: Replace ExecuteQuery in General.cs GetConstante

**Status:** Completed

**File Modified:** `Negocio.Business/Utilidades/General.cs` (Line 180-195)

**Change:**
- From: `cx.ExecuteQuery<string>(string.Format("SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", constante))`
- To: `cx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == constante).Select(x => x.CONSVALOR).FirstOrDefault()`

**Verification:**
- String.Format with CONS_REFERENCIA eliminated
- ExecuteQuery replaced with LINQ query
- POGE_CONSTANTE property correctly identified as `POGECONSTANTEs` in DataContext
- Column properties: `CONSREFERENCIA` (input), `CONSVALOR` (output)
- Method signature unchanged — callers require no modification

### Task 02: Replace ExecuteQuery in Utilidades.cs GetConstante

**Status:** Completed

**File Modified:** `Negocio.Business/Utilidades/Utilidades.cs` (Line 68-70)

**Change:**
- From: `ctx.ExecuteQuery<string>(string.Format("SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", nombreConstante))`
- To: `ctx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == nombreConstante).Select(x => x.CONSVALOR).FirstOrDefault()`

**Verification:**
- String.Format with CONS_REFERENCIA eliminated
- ExecuteQuery replaced with LINQ query using same property names
- Static method signature unchanged: `GetConstante(string, PORTALNEGOCIODataContext)`
- Callers in SendMail and other methods continue to work without modification

## Build Results

**Negocio.Business Build:**
```
Compilación correcta.
0 Errores, 11 Advertencia(s)
```

**PortalNegocioWS Build:**
```
0 Errores, 3 Advertencia(s)
```

All warnings are pre-existing (unused variables, async/await patterns, missing field assignments) — not related to SQL injection mitigations.

## Verification Summary

```bash
# No string.Format with CONS_REFERENCIA in either file
grep -rn "string.Format.*CONS_REFERENCIA" Negocio.Business/Utilidades/
# Result: No matches ✓

# LINQ queries present in both GetConstante methods
grep -n "POGECONSTANTEs.Where" Negocio.Business/Utilidades/*.cs
# Result: 2 matches (one per file) ✓

# Successful build
dotnet build PortalNegocioWS/PortalNegocioWS.csproj
# Result: Success ✓
```

## Security Impact

**Threat IDs Mitigated:**
- **T-01-03-01:** Tampering via SQL injection in General.cs GetConstante → Mitigated
- **T-01-03-02:** Tampering via SQL injection in Utilidades.cs GetConstante → Mitigated

**Mitigation Strategy:** Devart LinqConnect ORM generates parameterized SQL internally when using LINQ queries. String interpolation is completely eliminated. The method signatures remain unchanged, ensuring no breaking changes for existing callers.

**Note on T-01-03-03:** Information Disclosure via Oracle error messages remains as-is. Error handling is addressed in Phase 2 with structured logging (Serilog).

## DataContext Properties Used

| Property Name | Type | Table | Column |
|---|---|---|---|
| `POGECONSTANTEs` | `Devart.Data.Linq.Table<POGECONSTANTE>` | PORTAL_NEGOCIOS.POGE_CONSTANTE | — |
| `CONSREFERENCIA` | `string` | — | CONS_REFERENCIA |
| `CONSVALOR` | `string` | — | CONS_VALOR |

Verified from: `Negocio.Data/DataContext.Designer.cs` lines 543–550, 15683–15746.

## Commit History

| Hash | Message |
|---|---|
| `58e30d8` | fix(01-security-triage): parametrizar SQL en GetConstante (SQL injection) |

## Deviations from Plan

None — plan executed exactly as written.

## Success Criteria

- [x] `GetConstante` in General.cs no longer uses `string.Format` or `ExecuteQuery`
- [x] `GetConstante` in Utilidades.cs no longer uses `string.Format` or `ExecuteQuery`
- [x] Both methods use parameterized LINQ queries over `POGECONSTANTEs`
- [x] Method signatures unchanged — callers require no modification
- [x] Build successful (0 errors, pre-existing warnings only)
- [x] Requirement SEC-03 satisfied

## Files Modified

- `Negocio.Business/Utilidades/General.cs`
- `Negocio.Business/Utilidades/Utilidades.cs`

## Known Issues

None.

---

**Completed by:** Claude Code  
**Date:** 2026-04-15  
**Duration:** ~5 minutes
