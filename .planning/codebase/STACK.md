# Technology Stack

**Analysis Date:** 2026-04-14

## Languages

**Primary:**
- C# (.NET 9.0) - All four projects: API, Business, Data, Model layers

**Secondary:**
- SQL (Oracle PL/SQL) - Raw queries via `ExecuteQuery<T>()` in `Negocio.Business/Utilidades/Utilidades.cs` and `Negocio.Business/Utilidades/General.cs`

## Runtime

**Environment:**
- .NET 9.0 (net9.0 target framework across all projects)

**Package Manager:**
- NuGet with Central Package Management (`Directory.Packages.props`)
- Lockfile: Not present (no packages.lock.json detected)

## Frameworks

**Core:**
- ASP.NET Core 9.0 (Web SDK: `Microsoft.NET.Sdk.Web`) - REST API host in `PortalNegocioWS/`
- Devart LinqConnect ORM (`Devart.Data.Oracle.Linq` 4.9.2033, `Devart.Data.Linq` 5.0.0, `Devart.Data.Oracle` 10.0.0) - Proprietary Oracle LINQ ORM; data context auto-generated from `Negocio.Data/DataContext.lqml`

**Testing:**
- No test framework detected. No test projects exist in the solution.

**Build/Dev:**
- MSBuild / dotnet CLI
- Swashbuckle.AspNetCore 9.0.4 - Swagger/OpenAPI docs (dev-only, enabled via `app.UseSwagger()` in Development env)

## Key Dependencies

**Critical:**
- `Devart.Data.Oracle` 10.0.0 - Oracle database driver (requires Devart license key; key is embedded in the connection string in `appsettings.json`)
- `Devart.Data.Oracle.Linq` 4.9.2033 - LINQ-to-SQL for Oracle; DataContext generated from `Negocio.Data/DataContext.lqml`
- `Microsoft.AspNetCore.Authentication.JwtBearer` 9.0.9 - JWT Bearer auth, configured in `PortalNegocioWS/Installers/AuthenticationInstaller.cs`
- `AutoMapper` 14.0.0 - Object-to-object mapping; 80+ mappings defined in `PortalNegocioWS/Mappings/MappingProfile.cs`
- `Cronos` 0.11.1 - Cron expression parsing for background jobs in `PortalNegocioWS/Services/CronJobService.cs`
- `Stubble.Core` 1.10.8 - Mustache-style template rendering for email notification bodies in `Negocio.Business/Notificacion/Notificacion.cs`
- `NPOI` 2.7.4 - Excel file generation in `Negocio.Business/ArchivoExcel/`
- `Newtonsoft.Json` 13.0.3 - JSON serialization (used over System.Text.Json), configured via `AddNewtonsoftJson` in `PortalNegocioWS/Program.cs`
- `Serilog.AspNetCore` 9.0.0 + `Serilog.Sinks.Oracle` 1.1.1 - Structured logging to console, file (`Logs/log.txt`), and optionally Oracle DB

**Infrastructure:**
- `Microsoft.Extensions.Caching.StackExchangeRedis` 8.0.10 - Redis client package present but fully disabled (all code commented out in `PortalNegocioWS/Installers/CacheInstaller.cs`)
- `Microsoft.AspNetCore.ResponseCompression` 2.3.0 - GZip + Brotli compression for `application/json` responses, configured in `PortalNegocioWS/Installers/CompressInstaller.cs`
- `Microsoft.IdentityModel.Tokens` 8.14.0 + `System.IdentityModel.Tokens.Jwt` 8.14.0 - JWT token creation and validation
- `EntityFramework` 6.5.1 - Classic EF referenced by `Negocio.Business` (likely a transitive or legacy remnant; ORM is LinqConnect, not EF)
- `Microsoft.EntityFrameworkCore.Relational` 9.0.9 - EFCore relational abstractions (also appears to be a remnant alongside LinqConnect)

## Configuration

**Environment:**
- `PortalNegocioWS/appsettings.json` — Production config: Oracle connection string, JWT secret key, cron schedules, CORS URLs, storage type and paths
- `PortalNegocioWS/appsettings.Development.json` — Dev overrides: local file storage path (`D:\proyectos\PortalNegocios\files`), frontend/backend URLs
- Key settings: `JWT:SecretKey`, `Settings:DiasVenceClave`, `Settings:CronEnviarInvitacion`, `Settings:CronEnviarActualizacionDatos`, `Storage:Type`
- SMTP settings (server, port, credentials, SSL) stored in Oracle DB table `POGE_CONSTANTE` (constants: `serv_mail`, `port_mail`, `usr_mail`, `pwd_mail`, `send_mail`, `ssl_mail`), not in config files
- `EncryptedKey` in `appsettings.json` used as salt for SHA-512 password hashing

**Build:**
- `Directory.Packages.props` — Central NuGet version management for all projects
- `PortalNegocioWS/PortalNegocioWS.csproj` — API project; x64 platform target in Debug
- `Negocio.Data/DataContext.lqml` — LinqConnect designer file; generates `DataContext.Designer.cs` (do not edit manually)

## Platform Requirements

**Development:**
- .NET 9.0 SDK
- Oracle database access (server: `172.25.3.201`, SID: `SIA`)
- Devart dotConnect for Oracle license (key embedded in connection string)
- Windows assumed (dev storage path uses Windows backslash paths)

**Production:**
- Self-hosted on local network (production URL: `http://172.25.2.39:15032`)
- Oracle database server at `172.25.3.201`
- Local filesystem for file storage (S3 and Remote FTP options exist but are disabled)
- HTTPS not enforced (`RequireHttpsMetadata = false`, `app.UseHttpsRedirection()` is commented out)

---

*Stack analysis: 2026-04-14*
