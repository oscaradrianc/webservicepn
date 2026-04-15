# External Integrations

**Analysis Date:** 2026-04-14

## APIs & External Services

**Email (SMTP):**
- System.Net.Mail `SmtpClient` - Sends transactional emails for procurement events (new user, password reset, supplier registration, purchase solicitation invitations, quotation confirmations, adjudication notifications)
  - SDK/Client: `System.Net.Mail` (built-in .NET BCL)
  - Configuration: SMTP settings stored in Oracle DB table `POGE_CONSTANTE`; constants read at runtime via `GetConstante()` in `Negocio.Business/Utilidades/General.cs`
  - Constants keys: `serv_mail` (host), `port_mail` (port), `usr_mail` (username), `pwd_mail` (password), `ssl_mail` (enable SSL), `send_mail` (from address)
  - Auth: Credentials stored in database, not in config files
  - Email template rendering: Mustache-style via `Stubble.Core` (`StubbleBuilder`) in `Negocio.Business/Notificacion/Notificacion.cs`
  - Events that trigger email: 12+ named events (nuevousuario, resetpassword, registroproveedor, confregistroprov, autorizagerencia, autorizacompras, registropregunta, registrorespuesta, confirmacioncotizaci, registrocotizacion, publicacioninvitacion, adjudicado, desierto, actualizaciondatos)

## Data Storage

**Databases:**
- Oracle Database
  - Connection: `connectionStrings:PORTALNEGOCIODataContextConnectionString` in `appsettings.json`
  - Server: `172.25.3.201` (Direct mode, SID: `SIA`)
  - Schema user: `PORTAL_NEGOCIOS`
  - Client/ORM: Devart dotConnect for Oracle (`Devart.Data.Oracle` 10.0.0 + `Devart.Data.Oracle.Linq` 4.9.2033)
  - Data context: `PORTALNEGOCIODataContext` — auto-generated from `Negocio.Data/DataContext.lqml`; do not edit `Negocio.Data/DataContext.Designer.cs` manually
  - Direct mode: `Direct=True` bypasses Oracle client installation
  - License: Devart license key embedded directly in the connection string

**File Storage:**
- Local Filesystem (active)
  - Implementation: `Negocio.Business/Utilidades/LocalStorageService.cs`
  - Interface: `Negocio.Business/Utilidades/IStorageService.cs`
  - Base path (dev): `D:\proyectos\PortalNegocios\files` (in `appsettings.Development.json`)
  - Base path (prod): empty string in `appsettings.json` — must be configured
  - Registered as singleton in `PortalNegocioWS/Program.cs` via `Storage:Type = "Local"`

- Amazon S3 (disabled/stub)
  - Implementation class exists: `Negocio.Business/Utilidades/S3StorageService.cs`
  - All methods throw `NotImplementedException()`; S3 client code is commented out
  - Config placeholders exist in `appsettings.json` under `Storage:S3` (BucketName, Region, AccessKey, SecretKey)
  - DI wiring commented out in `PortalNegocioWS/Program.cs`

- Remote FTP/HTTP (disabled/partial)
  - Implementation class: `Negocio.Business/Utilidades/RemoteStorageService.cs`
  - Class does not implement `IStorageService` interface (interface inheritance removed)
  - DI wiring commented out in `PortalNegocioWS/Program.cs`
  - Config placeholders exist in `appsettings.json` under `Storage:Remote`

**Caching:**
- Redis (disabled)
  - Package present: `Microsoft.Extensions.Caching.StackExchangeRedis` 8.0.10
  - Configured in `appsettings.json` under `RedisCacheSettings` (ConnectionString: `localhost:6379`, Enabled: true)
  - All registration code is commented out in `PortalNegocioWS/Installers/CacheInstaller.cs`
  - `RedisManager/` project exists in repo root but is not referenced by any active project

## Authentication & Identity

**Auth Provider:**
- Custom JWT (no external identity provider)
  - Implementation: `PortalNegocioWS/Installers/AuthenticationInstaller.cs` + `PortalNegocioWS/Controllers/LoginController.cs`
  - Algorithm: HMAC-SHA256 symmetric signing
  - Secret: `JWT:SecretKey` in `appsettings.json`
  - Token validation: Issuer and Audience not validated (`ValidateIssuer = false`, `ValidateAudience = false`)
  - HTTPS enforcement: Disabled (`RequireHttpsMetadata = false`)
  - Role data: Resolved from Oracle tables `POGEROL` and `POGEOPCIONXROL` at login time

**Password Hashing:**
- SHA-512 with a salt key (`EncryptedKey` in `appsettings.json`, value: `PNEEP2021`)
- Implementation: `Negocio.Business/Utilidades/General.cs` → `GetStringEncriptado()` and `CreateSHA512()`
- Double-hash pattern: `SHA512(SHA512(salt + password))`

## Monitoring & Observability

**Error Tracking:**
- None (no external error tracking service such as Sentry or Application Insights)

**Logs:**
- Serilog (`Serilog.AspNetCore` 9.0.0)
  - Minimum level: Error (production)
  - Sinks active: Console + File (`Logs/log.txt` in `PortalNegocioWS/`)
  - Sink available but commented out: Oracle DB sink (`Serilog.Sinks.Oracle` 1.1.1) — configuration exists in commented-out `Program.cs` block
  - Enrichers configured: MachineName, ThreadId

**Oracle Diagnostic Monitor:**
- `OracleMonitor` from Devart is activated in `PortalNegocioWS/Program.cs` (`myMonitor.IsActive = true`) — enables Devart-level query diagnostics

## CI/CD & Deployment

**Hosting:**
- Self-hosted on internal network; production at `http://172.25.2.39:15032`
- Frontend expected at `http://172.25.2.39:15033`
- CORS restricted to configured frontend/backend URLs (policy: `"OrigenLocal"` in `PortalNegocioWS/Installers/CorsInstaller.cs`)

**CI Pipeline:**
- None detected. No `.github/`, `.gitlab-ci.yml`, `azure-pipelines.yml`, or similar found.

## Environment Configuration

**Required settings (appsettings.json):**
- `connectionStrings:PORTALNEGOCIODataContextConnectionString` — Oracle connection string with embedded Devart license
- `JWT:SecretKey` — JWT signing key
- `EncryptedKey` — SHA-512 password salt
- `Settings:URLFrontend` / `Settings:URLBackend` — CORS allowed origins
- `Settings:DiasVenceClave` — Password expiry days (default: 90)
- `Settings:CronEnviarInvitacion` — Cron schedule for invitation email job (default: `*/15 * * * *`)
- `Settings:CronEnviarActualizacionDatos` — Cron schedule for data update notification job
- `Storage:Type` — Storage backend selection (`"Local"`, `"S3"`, or `"Remote"`)
- `Storage:Local:BasePath` — Root path for local file storage

**SMTP settings (Oracle DB table `POGE_CONSTANTE`):**
- `serv_mail`, `port_mail`, `usr_mail`, `pwd_mail`, `send_mail`, `ssl_mail` — Must be populated in the database

**Secrets location:**
- Oracle connection string (with password + Devart license) stored in `appsettings.json` (not in environment variables or secret manager)
- JWT secret stored in `appsettings.json`
- SMTP credentials stored in Oracle DB

## Webhooks & Callbacks

**Incoming:**
- None detected. No webhook receiver endpoints found.

**Outgoing:**
- None detected. All external communication is outbound SMTP email only.

## Background Jobs

**CronJobService base class:** `PortalNegocioWS/Services/CronJobService.cs` — implements `IHostedService`, uses `Cronos` for schedule parsing

**Active jobs registered in `PortalNegocioWS/Program.cs`:**
- `ActualizarEstadoSolicitudJob` — daily at midnight (`0 0 * * *`): updates purchase solicitation statuses
- `EnviarNotificacionInvitacionJob` — every 15 minutes (`*/15 * * * *`, configurable): sends pending invitation emails to suppliers
- `NotificacionActualizacionDatosJob` — yearly on Jan 1 (`0 0 1 1 *`): sends supplier data update reminders (file exists but not registered in `Program.cs`)

---

*Integration audit: 2026-04-14*
