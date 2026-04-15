---
phase: 01
plan: 01-01
title: "Agregar UseAuthentication + auditar y proteger getadjuntos"
subsystem: Authentication & Authorization
tags: [security, jwt, middleware, authorization]
date_completed: 2026-04-15
duration_minutes: 12
status: complete
requirements_met: [SEC-01, SEC-05]
---

# Phase 01 Plan 01: Security Triage — JWT Middleware & Endpoint Protection Summary

**One-liner:** Activated JWT authentication middleware in the request pipeline and protected the `getadjuntos` endpoint from unauthorized access to procurement documents.

## Changes Made

### Task 1: Add app.UseAuthentication() to Program.cs

**File Modified:** `PortalNegocioWS/Program.cs`

**Line Changed:** 89-94

**Before:**
```csharp
app.UseCors("OrigenLocal");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Mapea los controladores de la API
```

**After:**
```csharp
app.UseCors("OrigenLocal");

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); // Mapea los controladores de la API
```

**Critical Detail:** The order is mandatory. `UseAuthentication()` must execute before `UseAuthorization()` for JWT token validation to occur before authorization checks.

### Task 2: Remove [AllowAnonymous] from getadjuntos Endpoint

**File Modified:** `PortalNegocioWS/Controllers/SolicitudController.cs`

**Line Changed:** 209-211

**Before:**
```csharp
[HttpGet]
[AllowAnonymous]
[Route("getadjuntos")]
public Response<SolicitudCompra> GetAdjuntosSolicitud(int id)
```

**After:**
```csharp
[HttpGet]
[Route("getadjuntos")]
public Response<SolicitudCompra> GetAdjuntosSolicitud(int id)
```

**Inheritance Mechanism:** The `SolicitudController` class has `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` at the class level (line 20). Removing `[AllowAnonymous]` from `GetAdjuntosSolicitud` causes the method to inherit this class-level authorization requirement, making JWT authentication mandatory.

**Public Endpoint Preserved:** The `ListInvitaciones` endpoint at line 153 retains its `[AllowAnonymous]` attribute by design — supplier invitations remain publicly accessible as required.

## Acceptance Criteria Met

✓ `grep -n "UseAuthentication"` returns line 92 in Program.cs  
✓ `grep -n "UseAuthorization"` returns line 93 in Program.cs (line number > 92)  
✓ Build succeeded with zero errors, 15 warnings (all pre-existing)  
✓ Exactly one `[AllowAnonymous]` remains in SolicitudController.cs (at line 153, for listinvitacion)  
✓ `getadjuntos` method no longer shows `[AllowAnonymous]` in grep context

## Build Verification

```
dotnet build PortalNegocioWS/PortalNegocioWS.csproj

Result: Compilación correcta (Build succeeded)
Errors: 0
Warnings: 15 (all pre-existing, unrelated to this change)
Duration: 00:00:16.57
```

## Security Impact

**Before:** Any unauthenticated client could request `/api/solicitud/getadjuntos/{id}` and receive procurement documents (terms, attachments, specifications).

**After:** All requests to `getadjuntos` must include a valid JWT Bearer token. The middleware chain:
1. Client sends request with `Authorization: Bearer <token>`
2. `UseAuthentication()` validates the JWT signature and expiry
3. `UseAuthorization()` checks the `[Authorize]` attribute on the endpoint
4. Invalid/missing tokens receive HTTP 401 Unauthorized
5. Valid tokens allow access to procurement documents

**Threat Mitigations:**
- **T-01-01-01 (Spoofing):** `app.UseAuthentication()` validates token signatures before handlers process requests
- **T-01-01-02 (Information Disclosure):** Removing `[AllowAnonymous]` restricts document access to authenticated users
- **T-01-01-03 (Elevation of Privilege):** Without `UseAuthentication()`, all `[Authorize]` attributes in the codebase are ineffective — this fix closes the gap

## Commits

| Hash | Message |
|------|---------|
| 99e6f48 | feat(01-security-triage): activate JWT middleware and protect getadjuntos endpoint |

## Deviations from Plan

None — plan executed exactly as specified.

## Self-Check

✓ File `/d/proyectos/PortalNegociosGithub/webservicepn/PortalNegocioWS/Program.cs` exists and contains `app.UseAuthentication();` at line 92  
✓ File `/d/proyectos/PortalNegociosGithub/webservicepn/PortalNegocioWS/Controllers/SolicitudController.cs` exists and no longer has `[AllowAnonymous]` on getadjuntos method  
✓ Commit 99e6f48 exists in git log  
✓ Build passes with zero errors  

## Next Steps

This plan satisfies requirements SEC-01 and SEC-05 from the Phase 1 security audit. The next plans in this phase will:
- Phase 01 Plan 02: Audit and standardize HTTP response codes across all endpoints
- Phase 01 Plan 03: Implement structured logging for authentication and authorization events
- Phase 01 Plan 04: Validate input parameters in request DTOs
