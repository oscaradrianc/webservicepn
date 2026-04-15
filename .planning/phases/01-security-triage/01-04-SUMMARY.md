---
phase: 01
plan: 01-04
title: "Configurar ValidateIssuer y ValidateAudience en JWT"
subsystem: Authentication
tags: [jwt, security, issuer-validation, audience-validation]
duration: "5 minutes"
completed_date: 2026-04-15
dependencies:
  requires: []
  provides: [JWT-ISSUER-AUDIENCE-VALIDATION]
  affects: [all-api-endpoints]
tech_stack:
  patterns: [config-driven-security, jwt-bearer-validation]
  added: []
key_files:
  created: []
  modified:
    - PortalNegocioWS/Installers/AuthenticationInstaller.cs
    - PortalNegocioWS/appsettings.json
    - PortalNegocioWS/appsettings.Development.json
decisions:
  - id: JWT-ISSUER-VALUE
    decision: "Used 'PortalNegociosAPI' as JWT:Issuer value for all environments"
    rationale: "Consistent identifier for API origin, same in prod and dev per plan D-05"
  - id: JWT-AUDIENCE-VALUE
    decision: "Used 'PortalNegociosApp' as JWT:Audience value for all environments"
    rationale: "Consistent audience identifier, same in prod and dev per plan D-06, D-07"
---

## Summary

Successfully activated JWT issuer and audience validation in the ASP.NET Core authentication layer. This plan closes threat T-01-04-01 and T-01-04-02 by enabling token origin validation (spoofing mitigation). Tokens from external services or with incorrect audience values are now rejected with 401 Unauthorized.

## What Changed

### Task 1: Added JWT:Issuer and JWT:Audience to Configuration

**File: PortalNegocioWS/appsettings.json**

Updated JWT section to include issuer and audience claims:

```json
"JWT": {
  "SecretKey": "uM3LdotPoO9ijpOI5iY8qUgfQPcXZLfJw7334kYvqOcchuSsgOafNmLpcuBPDry",
  "Issuer": "PortalNegociosAPI",
  "Audience": "PortalNegociosApp"
}
```

**File: PortalNegocioWS/appsettings.Development.json**

Added JWT section override for development environment:

```json
"JWT": {
  "Issuer": "PortalNegociosAPI",
  "Audience": "PortalNegociosApp"
}
```

### Task 2: Enabled ValidateIssuer and ValidateAudience in AuthenticationInstaller

**File: PortalNegocioWS/Installers/AuthenticationInstaller.cs**

Updated TokenValidationParameters block:

```csharp
x.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = true,
    ValidIssuer = configuration.GetValue<string>("JWT:Issuer"),
    ValidateAudience = true,
    ValidAudience = configuration.GetValue<string>("JWT:Audience")
};
```

Changes made:
- `ValidateIssuer`: changed from `false` to `true`
- Added `ValidIssuer` property reading from config key `JWT:Issuer`
- `ValidateAudience`: changed from `false` to `true`
- Added `ValidAudience` property reading from config key `JWT:Audience`

## Verification Results

All acceptance criteria passed:

```bash
grep -n "Issuer" PortalNegocioWS/appsettings.json
# Output: "Issuer": "PortalNegociosAPI" ✓

grep -n "Audience" PortalNegocioWS/appsettings.json
# Output: "Audience": "PortalNegociosApp" ✓

grep -n "Issuer" PortalNegocioWS/appsettings.Development.json
# Output: "Issuer": "PortalNegociosAPI" ✓

grep -n "Audience" PortalNegocioWS/appsettings.Development.json
# Output: "Audience": "PortalNegociosApp" ✓

grep -n "ValidateIssuer = true" PortalNegocioWS/Installers/AuthenticationInstaller.cs
# Output: ValidateIssuer = true ✓

grep -n "ValidateAudience = true" PortalNegocioWS/Installers/AuthenticationInstaller.cs
# Output: ValidateAudience = true ✓

grep -n "ValidIssuer = configuration.GetValue" PortalNegocioWS/Installers/AuthenticationInstaller.cs
# Output: ValidIssuer = configuration.GetValue<string>("JWT:Issuer") ✓

grep -n "ValidAudience = configuration.GetValue" PortalNegocioWS/Installers/AuthenticationInstaller.cs
# Output: ValidAudience = configuration.GetValue<string>("JWT:Audience") ✓

grep -n "ValidateIssuer = false\|ValidateAudience = false" PortalNegocioWS/Installers/AuthenticationInstaller.cs
# Output: (no results - old false values removed) ✓

dotnet build PortalNegocioWS/PortalNegocioWS.csproj
# Output: Build succeeded. 0 Errors. (Pre-existing warnings only) ✓
```

## Security Impact

This plan activates two critical security controls:

1. **Issuer Validation (T-01-04-01 Mitigation)**: Tokens from other services claiming different issuer values are now rejected, preventing spoofing attacks via token substitution.

2. **Audience Validation (T-01-04-02 Mitigation)**: Tokens with incorrect audience claims are rejected, ensuring tokens are only valid for the intended application.

Both validations follow the principle of least privilege — only tokens signed by the correct issuer AND intended for the correct audience are accepted.

## Deviations from Plan

None - plan executed exactly as written.

## Self-Check: PASSED

- File created: .planning/phases/01-security-triage/01-04-SUMMARY.md ✓
- All modified files exist ✓
- Commit hash 204d04e verified in git log ✓
- Build succeeds with no errors ✓
