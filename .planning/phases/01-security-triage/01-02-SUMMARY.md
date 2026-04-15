---
phase: "01"
plan: "01-02"
title: "Fix CambiarClaveUsuario (texto plano) + corregir throw e en Usuario.cs y Controllers"
subsystem: "Authentication & Error Handling"
tags: ["security", "cryptography", "error-handling", "password", "bug-fix"]
type: "execute"
requirements: ["SEC-02", "HYG-07"]
status: "complete"
dependencies:
  requires: ["01-01"]
  provides: ["Hashed password storage in password change", "Stack trace preservation in error handling"]
  affects: ["Authentication flow", "Exception logging", "API error responses"]
tech_stack:
  patterns: ["IUtilidades.GetStringEncriptado", "SHA-512 double hash", "Proper throw in catch blocks"]
  libraries: []
key_files:
  created: []
  modified:
    - "Negocio.Business/Usuario/Usuario.cs"
    - "Negocio.Business/ArchivoExcel/ArchivoExcel.cs"
decisions:
  - "Applied Rule 2 (auto-fix missing critical functionality) to fix throw ex in ArchivoExcelBusiness that was not explicitly in scope but follows the same pattern as the main task"
execution_time: "7 minutes"
---

## Summary

Successfully fixed password hashing in the CambiarClaveUsuario method and corrected all exception re-throwing patterns that were destroying stack traces.

## Task 1: Fix CambiarClaveUsuario Password Hashing

**Objective:** Replace plain text password storage with SHA-512 hash in the password change method.

### Changes Made

File: `Negocio.Business/Usuario/Usuario.cs`

**Line 272 - Before:**
```csharp
query.USUACLAVE = request.NuevaClave;
```

**Line 272 - After:**
```csharp
query.USUACLAVE = _utilidades.GetStringEncriptado(request.NuevaClave, _configuration.GetSection("EncryptedKey").Value);
```

**Lines 284-287 - Before:**
```csharp
catch (Exception e)
{
    throw e;
}
```

**Lines 284-287 - After:**
```csharp
catch
{
    throw;
}
```

### Security Impact

- **T-01-02-01 (Information Disclosure):** Mitigated - Plain text passwords are no longer stored in POGEUSUARIO.USUA_CLAVE
- **T-01-02-02 (Tampering):** Ensured - Uses identical SHA-512 hashing pattern as Authenticate() method, guaranteeing consistency
- **T-01-02-03 (Stack Trace Loss):** Fixed - throw statement now preserves full stack trace

### Hashing Pattern Consistency

The implementation uses the established pattern from `IUtilidades.GetStringEncriptado()`:
1. Encodes password as Unicode bytes
2. Prepends EncryptedKey as prefix
3. Applies SHA-512 double-hash via `CreateSHA512()`
4. Returns Base64-encoded result

This matches exactly how `Authenticate()` verifies passwords during login (line 270), ensuring password created with `CambiarClaveUsuario` can be verified successfully.

## Task 2: Sweep Controllers - Exception Re-throwing

**Objective:** Verify no controllers use `throw [variable];` pattern in catch blocks.

### Findings

Verification showed that **no controllers currently have `throw [variable];` patterns**. All 19 controller files properly handle exceptions with:
- `return StatusCode(500, ...)`
- `return NotFound(...)`
- `return BadRequest(...)`
- Other HTTP response mechanisms

All controllers already follow the correct pattern of not re-throwing exceptions with a named variable.

### Additional Fix: ArchivoExcelBusiness (Deviation - Rule 2)

During verification, discovered that `Negocio.Business/ArchivoExcel/ArchivoExcel.cs` had a `throw ex;` pattern that destroys stack traces, identical to the issue we fixed in Usuario.cs.

File: `Negocio.Business/ArchivoExcel/ArchivoExcel.cs`

**Lines 96-101 - Before:**
```csharp
catch (Exception ex)
{
    transaction.Rollback();
    cx.Connection.Close();
    throw ex;
}
```

**Lines 96-101 - After:**
```csharp
catch
{
    transaction.Rollback();
    cx.Connection.Close();
    throw;
}
```

This is a Rule 2 auto-fix: missing critical functionality (stack trace preservation) that directly relates to the task's security/debugging goals. The pattern matches exactly what we're fixing in Usuario.cs.

## Verification Results

All acceptance criteria verified:

```bash
✓ No plain text password assignment in CambiarClaveUsuario
✓ GetStringEncriptado called for new password hashing
✓ No "throw e" patterns in Negocio.Business/
✓ No "throw [variable];" patterns in PortalNegocioWS/Controllers/
✓ Full solution builds successfully: 0 Errors, 11 Warnings (pre-existing)
```

### Build Output
```
Compilación correcta.
0 Errores
Tiempo transcurrido 00:00:03.05
```

## Commits

| Commit | Message | Files |
|--------|---------|-------|
| a9f1d48 | fix(01-security-triage): hash new password in CambiarClaveUsuario + fix throw e | Usuario/Usuario.cs |
| 9c42f65 | fix(01-security-triage): fix throw ex to throw in ArchivoExcelBusiness | ArchivoExcel/ArchivoExcel.cs |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical Functionality] Fixed throw ex in ArchivoExcelBusiness**
- **Found during:** Verification sweep for throw patterns
- **Issue:** `throw ex;` in RegistrarArchivo catch block destroys stack trace, same security/debugging issue as Task 1
- **Fix:** Changed to `throw;` to preserve stack trace
- **Files modified:** Negocio.Business/ArchivoExcel/ArchivoExcel.cs
- **Commit:** 9c42f65

## Success Criteria Verification

| Criteria | Result |
|----------|--------|
| `query.USUACLAVE` receives hashed value, not plain text | ✓ Uses `GetStringEncriptado()` |
| Hash pattern matches `GetStringEncriptado` | ✓ Identical to login verification |
| `throw e` replaced with `throw;` in CambiarClaveUsuario | ✓ Line 286 |
| All controllers use proper exception handling | ✓ No `throw [variable];` found |
| `grep -rn 'throw [a-zA-Z]\+;' PortalNegocioWS/Controllers/` returns 0 | ✓ 0 matches |
| Solution builds successfully | ✓ 0 Errors |

## Security Impact Summary

- **Passwords:** Now stored as SHA-512 hashes in CambiarClaveUsuario (matching Authenticate verification)
- **Error Handling:** Stack traces no longer lost during exception re-throwing in business layer
- **Logging:** Complete stack traces available for error analysis and debugging
- **Authentication:** Password change flow now cryptographically consistent with login authentication

---

*Plan completed 2026-04-15 — 2 files modified, 2 commits, all criteria met*
