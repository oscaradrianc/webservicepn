---
phase: "02"
phase_name: "observability-foundation"
status: "issues_found"
depth: standard
files_reviewed: 6
findings:
  critical: 4
  warning: 6
  info: 7
  total: 17
reviewed_at: "2026-04-15"
---

# Code Review — Phase 02: Observability Foundation

## Critical

### CR-01: Hardcoded production database credentials in appsettings.json

**File:** `PortalNegocioWS/appsettings.json:44`
**Issue:** Oracle connection string contains plaintext username (`PORTAL_NEGOCIOS`), password (`portal2021`), and server IP (`172.25.3.201`) committed to source control. Anyone with repo access has direct database access.
**Fix:** Move to environment variable (`CONNECTIONSTRINGS__PORTALNEGOCIOS...`). Leave placeholder empty in appsettings.json.

---

### CR-02: Hardcoded JWT secret key in appsettings.json

**File:** `PortalNegocioWS/appsettings.json:39`
**Issue:** `JWT:SecretKey` is a hardcoded 64-char string in source control. Any party with repo access can forge valid JWT tokens and bypass all authentication.
**Fix:** Inject via `JWT__SecretKey` environment variable. Set empty string in appsettings.json.

---

### CR-03: Hardcoded encryption key in appsettings.json

**File:** `PortalNegocioWS/appsettings.json:37`
**Issue:** `"EncryptedKey": "PNEEP2021"` committed to source control. Used to salt password hashes — knowing this enables offline pre-computation against any known password.
**Fix:** Move to `ENCRYPTEDKEY` environment variable.

---

### CR-04: Invalid JSON — inline comment causes startup parse exception

**File:** `PortalNegocioWS/appsettings.json:48`
**Issue:** `"Type": "Local", // Cambia entre Local, S3, o Remote` contains a JS-style comment. JSON does not support comments — `System.Text.Json` will throw a parse exception and the app will fail to start.
**Fix:** Remove the inline comment from the JSON value.

---

## Warning

### WR-01: try/catch wraps only app.Run() — startup exceptions bypass Log.Fatal

**File:** `PortalNegocioWS/Program.cs:109-120`
**Issue:** If an exception is thrown during `builder.Build()`, installer execution, or middleware setup, it propagates uncaught past the `Log.Fatal` call. The bootstrap logger will be active but the structured fatal message is never written.
**Fix:** Expand the try block to wrap the entire startup sequence including `WebApplication.CreateBuilder`.

---

### WR-02: SendMail silently swallows exceptions — callers receive no error signal

**File:** `Negocio.Business/Utilidades/General.cs:373-378`
**Issue:** `catch` block logs and returns normally. Background jobs that send invitation emails will not know delivery failed. Silent failure is the only outcome.
**Fix:** Add `throw;` after `Log.Error(...)` to preserve caller awareness, or return a `bool` success indicator. If fire-and-forget is intentional, document this contract.

---

### WR-03: ObtenerBlob throws NullReferenceException when blob not found

**File:** `Negocio.Business/Utilidades/General.cs:407-412`
**Issue:** `FirstOrDefault()` returns `null` when no blob matches. `Convert.ToBase64String(buffer)` on the next line throws `NullReferenceException`.
**Fix:** Add null check: `if (buffer == null) return string.Empty;`

---

### WR-04: CreateSHA512 double-hashes input — hash-of-hash, not hash of original

**File:** `Negocio.Business/Utilidades/General.cs:281-286`
**Issue:** `alg.ComputeHash(strData)` is called once, then `alg.ComputeHash(hashValue)` is called on the result. External systems computing SHA-512 of the same input will get a different value. Almost certainly a refactoring bug from the prior `SHA512Managed` block (which hashed only once).
**Fix:** Remove the second `ComputeHash` call. **Warning:** If this double-hash is already in production with stored hashes, changing it invalidates all passwords — verify against live data before fixing.

---

### WR-05: GetSecuencia uses string interpolation in raw SQL — SQL injection risk

**File:** `Negocio.Business/Utilidades/General.cs:296`
**Issue:** `string.Format("SELECT {0}.NEXTVAL FROM DUAL", nombreSecuencia)` injects the sequence name directly into a raw SQL query. If any caller passes a user-controlled value, this is a SQL injection vector.
**Fix:** Validate `nombreSecuencia` against an allowlist of known sequence names before interpolating.

---

### WR-06: ConvertirMensaje throws IndexOutOfRangeException on malformed input

**File:** `Negocio.Business/Utilidades/General.cs:397`
**Issue:** `item.Split('~')` produces a 1-element array when `~` is absent. `variable[1]` access throws `IndexOutOfRangeException` unconditionally.
**Fix:** `if (variable.Length < 2) continue;` before accessing `variable[1]`.

---

## Info

### IN-01: Bootstrap logger has no MinimumLevel — defaults to Verbose

**File:** `PortalNegocioWS/Program.cs:20-22`
**Fix:** Add `.MinimumLevel.Warning()` to the bootstrap logger configuration.

---

### IN-02: Serilog.Sinks.Oracle referenced but never configured

**File:** `PortalNegocioWS/PortalNegocioWS.csproj:21`
**Issue:** Package adds binary weight and an unused Oracle sink dependency. No Oracle sink in `appsettings.json`.
**Fix:** Remove `<PackageReference Include="Serilog.Sinks.Oracle" />` until the sink is actually configured.

---

### IN-03: Microsoft.AspNetCore.Mvc.Abstractions/Core pinned to 2.2 on net9.0 target

**File:** `Directory.Packages.props:25-26`
**Issue:** Version `2.2.0`/`2.2.5` while project targets `net9.0`. These types are included in the .NET 9 shared framework — explicit NuGet pins can cause type-identity conflicts at runtime.
**Fix:** Remove both entries from `Directory.Packages.props`.

---

### IN-04: Microsoft.AspNetCore.ResponseCompression pinned to 2.3.0 — same mismatch as IN-03

**File:** `Directory.Packages.props:31`
**Fix:** Remove entry; provided by the `net9.0` framework.

---

### IN-05: RedisCacheSettings.Enabled: true with Redis disabled

**File:** `PortalNegocioWS/appsettings.json:24-27`
**Issue:** `"Enabled": true` but Redis is commented out in `CacheInstaller.cs`. Misleading config state.
**Fix:** Set `"Enabled": false` to match runtime behavior.

---

### IN-06: Commented-out dead code blocks in General.cs

**File:** `Negocio.Business/Utilidades/General.cs:206-208, 263, 272-276, 318-322, 502-508`
**Issue:** Multiple large commented-out blocks including duplicate `DecodificarArchivo` implementation and obsolete raw-SQL `GetConstante`.
**Fix:** Delete. Git history preserves them if needed.

---

### IN-07: Storage:Local:BasePath is empty string in production config

**File:** `PortalNegocioWS/appsettings.json:50`
**Issue:** Resolves all file paths relative to process working directory — fragile in production deployments.
**Fix:** Set explicit absolute path, or document that this must be overridden via environment variable before deploy.
