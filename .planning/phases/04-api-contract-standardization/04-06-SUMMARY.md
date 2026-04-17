# Plan 04-06: Add DataAnnotations to Critical Request Models

**Status:** Completed
**Completed:** 2026-04-17
**Wave:** 2
**Depends On:** 04-01

## Objective

Add DataAnnotations ([Required], [MaxLength], [EmailAddress], [Range]) to the most critical request models. Since all 19 controllers already have [ApiController], adding [Required] to a model immediately causes automatic HTTP 422 ValidationProblemDetails responses for requests missing that field — no controller code changes needed.

## Execution Summary

### Task 1: Add DataAnnotations to Auth/Login Models (Priority 1)

**Files Modified:**
- `Negocio.Model/Login/LoginRequest.cs`
- `Negocio.Model/ChangePasswordRequest.cs`
- `Negocio.Model/Login/ResetPassRequest.cs`
- `Negocio.Model/Usuario/CambioClave.cs`

**Changes:**
- **LoginRequest**: Added [Required] on Username, Password, Origen
- **ChangePasswordRequest**: Added [Required] on Username, Password, NewPassword, Origen
- **ResetPassRequest**: Added [Required] + [EmailAddress] on Email, [Required] on Username
- **CambioClave**: Added [Required] on Usuario, ClaveAnterior, NuevaClave

**Rationale:** These are public-facing authentication endpoints where validation is critical. All fields are always required in both create and authentication contexts.

**Verification:**
- `grep -c "[Required" Negocio.Model/Login/LoginRequest.cs` returns 3
- `grep "[Required" Negocio.Model/Login/ResetPassRequest.cs` returns matches
- `grep "[EmailAddress]" Negocio.Model/Login/ResetPassRequest.cs` returns match
- `grep -c "[Required" Negocio.Model/Usuario/CambioClave.cs` returns 3
- `dotnet build` passes with 0 errors

### Task 2: Add DataAnnotations to Business Models (Priority 2 + 3)

**Files Modified:**
- `Negocio.Model/Solicitud/SolicitudCompra.cs`
- `Negocio.Model/Solicitud/Autorizacion.cs`
- `Negocio.Model/Cotizacion/Cotizacion.cs`

**Changes:**
- **SolicitudCompra**: Added [Required] on Descripcion and TipoSolicitud. NO [Range] on int fields (TipoContratacion, Area) because this is a dual-use model used in both create and update operations where int fields may be 0 in partial-update payloads.
- **Autorizacion**: Added [Range(1, int.MaxValue)] on CodigoSolicitud and IdUsuario; [Required] on EstadoAutorizacion and TipoAutorizacion. Left EstadoActual and Observacion unannotated.
- **Cotizacion**: Added [Range(1, int.MaxValue)] on CodigoProveedor, CodigoSolicitud, CodigoUsuario with comments documenting the safety decision. Verified that Cotizacion is ONLY used in create operations (RegistrarCotizacion) - no update methods found in ICotizacion.

**Business Layer Analysis:**
- Analyzed `ISolicitudCompra.ActualizarSolicitud` to confirm SolicitudCompra is used in updates - int fields receive values from the request and could be 0, so [Range] would break partial updates
- Analyzed `ICotizacion` to confirm no update methods exist for Cotizacion - only `RegistrarCotizacion` which expects non-zero FK values
- Analyzed `Autorizacion` usage in authorization flows - always requires both CodigoSolicitud and IdUsuario to be non-zero

**Verification:**
- `grep "[Required" Negocio.Model/Solicitud/SolicitudCompra.cs` returns matches
- `grep "Descripcion" Negocio.Model/Solicitud/SolicitudCompra.cs` shows [Required] above it
- `grep "TipoSolicitud" Negocio.Model/Solicitud/SolicitudCompra.cs` shows [Required] above it
- `grep "[Range" Negocio.Model/Solicitud/SolicitudCompra.cs` returns 0 matches (no Range on dual-use int fields)
- `grep -c "[Required" Negocio.Model/Solicitud/Autorizacion.cs` returns 2
- `grep -c "[Range" Negocio.Model/Solicitud/Autorizacion.cs` returns 2
- `grep -c "[Range" Negocio.Model/Cotizacion/Cotizacion.cs` returns 3
- `dotnet build` passes with 0 errors

## Threat Model Compliance

| Threat ID | Category | Component | Disposition | Mitigation |
|-----------|----------|-----------|-------------|------------|
| T-04-15 | Tampering | LoginRequest [Required] on Origen | Mitigated | Origen distinguishes internal ("I") from supplier ("P") users. [Required] ensures the field is present. |
| T-04-16 | Tampering | SolicitudCompra [Required] - dual-use caution | Mitigated | Only string fields annotated. Int fields left unannotated after business layer analysis confirmed update paths. |
| T-04-17 | Denial of Service | [EmailAddress] on ResetPassRequest.Email | Accepted | Validates format only, no network calls. Beneficial - not a DoS risk. |

## Key Decisions

1. **SolicitudCompra Int Fields**: Did NOT add [Range] to TipoContratacion, Area, or other int fields. These are used in `ActualizarSolicitud` where partial updates may send 0 values. Adding [Range] would break existing update payloads.

2. **Cotizacion FK Fields**: Added [Range(1, int.MaxValue)] to CodigoProveedor, CodigoSolicitud, CodigoUsuario. Verified that Cotizacion is ONLY used in `RegistrarCotizacion` (create operation) - no update methods exist in `ICotizacion`. The FK fields are used in WHERE clauses and INSERTs, so 0 would cause logical errors.

3. **Autorizacion Fields**: Added [Range] to CodigoSolicitud and IdUsuario, [Required] to EstadoAutorizacion and TipoAutorizacion. Authorization always requires both the request ID and user ID to be non-zero, and the authorization state and type are mandatory strings.

## Impact on API Contracts

All modified models now trigger automatic HTTP 422 ValidationProblemDetails responses when validation fails, thanks to the existing [ApiController] attribute on all controllers. No controller code changes were required.

### New Validation Errors

Clients will now receive 422 responses for:

**Authentication Endpoints:**
- LoginRequest: Missing Username, Password, or Origen
- ChangePasswordRequest: Missing Username, Password, NewPassword, or Origen
- ResetPassRequest: Missing Username or Email, or invalid Email format
- CambioClave: Missing Usuario, ClaveAnterior, or NuevaClave

**Business Endpoints:**
- SolicitudCompra: Missing Descripcion or TipoSolicitud
- Autorizacion: CodigoSolicitud ≤ 0, IdUsuario ≤ 0, missing EstadoAutorizacion, or missing TipoAutorizacion
- Cotizacion: CodigoProveedor ≤ 0, CodigoSolicitud ≤ 0, or CodigoUsuario ≤ 0

## Artifacts Created

1. **Updated Model Files** (7 files):
   - Negocio.Model/Login/LoginRequest.cs
   - Negocio.Model/ChangePasswordRequest.cs
   - Negocio.Model/Login/ResetPassRequest.cs
   - Negocio.Model/Usuario/CambioClave.cs
   - Negocio.Model/Solicitud/SolicitudCompra.cs
   - Negocio.Model/Solicitud/Autorizacion.cs
   - Negocio.Model/Cotizacion/Cotizacion.cs

2. **Commits** (2 atomic commits):
   - `feat(04-06): add DataAnnotations to auth/login models`
   - `feat(04-06): add DataAnnotations to business models`

## Success Criteria Met

- ✅ `dotnet build` passes with 0 errors
- ✅ LoginRequest, ChangePasswordRequest, ResetPassRequest, CambioClave: all auth-critical fields have [Required]
- ✅ ResetPassRequest.Email has [EmailAddress]
- ✅ SolicitudCompra: [Required] on Descripcion and TipoSolicitud only (no [Range] on int fields)
- ✅ Autorizacion: [Range(1,int.MaxValue)] on CodigoSolicitud and IdUsuario; [Required] on EstadoAutorizacion and TipoAutorizacion
- ✅ Cotizacion: int FK fields annotated with [Range] (confirmed safe via business layer analysis)
- ✅ Each task committed individually
- ✅ SUMMARY.md created in plan directory

## Next Steps

No immediate next steps - this plan is complete. The next plans in Wave 2 (04-07, 04-08) will continue API contract standardization work on other models and endpoints.
