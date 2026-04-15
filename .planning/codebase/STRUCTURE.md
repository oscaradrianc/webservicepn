# STRUCTURE.md — Directory Layout and Organization

## Solution Structure

```
webservicepn/
├── PortalNegocioWS.sln              # Solution file
├── Directory.Packages.props         # Centralized NuGet package versions
├── PortalNegocioWS/                 # API entry point project
├── Negocio.Business/                # Business logic layer
├── Negocio.Model/                   # DTOs and request/response models
└── Negocio.Data/                    # Auto-generated ORM data context
```

## PortalNegocioWS/ (API Layer)

```
PortalNegocioWS/
├── Program.cs                       # App startup, IInstaller discovery, middleware pipeline
├── StartupCopia.cs                  # Legacy startup (unused, kept for reference)
├── appsettings.json                 # Production config (Oracle conn, JWT, cron, storage)
├── appsettings.Development.json     # Dev overrides (local paths, URLs)
├── Controllers/                     # HTTP endpoints (one controller per domain)
│   ├── LoginController.cs           # Auth: JWT token issuance, password change
│   ├── SolicitudController.cs       # Purchase solicitations CRUD
│   ├── CotizacionController.cs      # Quotations management
│   ├── ProveedorController.cs       # Supplier management
│   ├── CatalogoController.cs        # Catalog items
│   ├── ConsultaController.cs        # Read-only query endpoints
│   ├── UsuarioController.cs         # User management
│   ├── RolController.cs             # Role management
│   ├── OpcionController.cs          # Menu options
│   ├── OpcionesRolController.cs     # Role-option assignments
│   ├── NotificacionController.cs    # Notifications
│   ├── NotificacionUsuarioController.cs
│   ├── NoticiasController.cs        # News/announcements
│   ├── AutorizadorGerenciaController.cs  # Management approvals
│   ├── ConstanteController.cs       # System constants
│   ├── ParametroGeneralController.cs    # General parameters
│   ├── PreguntasController.cs       # Security questions
│   ├── FormatoController.cs         # Document formats
│   └── UtilidadesController.cs      # Utility endpoints
├── Installers/                      # DI registration (IInstaller pattern)
│   ├── IInstaller.cs                # Marker interface
│   ├── AuthenticationInstaller.cs   # JWT Bearer setup
│   ├── BusinessInstaller.cs         # Scoped business service registrations
│   ├── AutoMapperInstaller.cs       # AutoMapper singleton
│   ├── CorsInstaller.cs             # CORS policy
│   ├── SwaggerInstaller.cs          # Swagger/OpenAPI
│   ├── CacheInstaller.cs            # Redis (disabled/commented)
│   └── CompressInstaller.cs         # Response compression
├── Mappings/
│   └── MappingProfile.cs            # AutoMapper profile (80+ entity↔model mappings)
├── Services/                        # Background hosted services
│   ├── CronJobService.cs            # Base class for cron jobs
│   ├── IScopedService.cs            # Interface for scoped job logic
│   ├── ActualizarEstadoSolicitudJob.cs    # Updates solicitation statuses
│   ├── EnviarNotificacionInvitacionJob.cs # Sends invitation emails
│   └── NotificacionActualizacionDatosJob.cs # Data sync notifications
└── Logs/                            # File-based logs (log{date}.txt)
```

## Negocio.Business/ (Business Logic Layer)

```
Negocio.Business/
├── Negocio.Business.csproj
├── {Domain}/                        # One folder per domain
│   ├── I{Domain}.cs                 # Interface
│   └── {Domain}.cs                  # Implementation
│
├── Solicitud/                       # Purchase solicitation logic
├── Cotizacion/                      # Quotation workflow
├── Proveedor/                       # Supplier management
├── Usuario/                         # User accounts
├── Login/                           # Authentication logic
├── Rol/                             # Roles and permissions
├── Opcion/                          # Menu options
├── Catalogo/                        # Catalog management
├── Consultas/                       # Complex read queries
├── Notificacion/                    # Push notifications
├── NotificacionUsuario/             # User notification preferences
├── Noticias/                        # News/announcements
├── AutorizadorGerencia/             # Management approval workflow
├── Constante/                       # System constants
├── ParametroGeneral/                # General parameters
├── Preguntas/                       # Security questions
├── ArchivoExcel/                    # Excel file generation
└── Utilidades/                      # Shared utilities
```

## Negocio.Model/ (DTOs)

```
Negocio.Model/
├── Negocio.Model.csproj
├── ChangePasswordRequest.cs         # Standalone request (at root)
├── {Domain}/                        # Mirrors Business domain folders
│   ├── {Domain}Model.cs             # Response/read DTO
│   └── {Domain}Request.cs           # Request/write DTO (when separate)
│
├── Login/
├── Solicitud/
├── Cotizacion/
├── Proveedor/
├── Usuario/
├── Catalogo/
├── Consultas/
├── Notificaciones/
├── Noticias/
├── AutorizadorGerencia/
├── Constante/
├── Configuracion/
├── Documento/
├── Especialidad/
├── Accionista/
└── ArchivoExcel/
```

## Negocio.Data/ (Data Layer — Auto-generated)

```
Negocio.Data/
├── Negocio.Data.csproj
├── DataContext.lqml                 # LinqConnect schema definition (source of truth)
├── DataContext.Designer.cs          # AUTO-GENERATED — DO NOT EDIT
├── DataContext.edps                 # Devart designer settings
├── DataContext.Diagram1.view        # Visual diagram
├── DataContextFactory.cs            # IDataContextFactory implementation
├── IDataContextFactory.cs           # Factory interface
└── app.config                       # Legacy config remnant
```

## Key File Locations

| Purpose | File |
|---------|------|
| App entry point | `PortalNegocioWS/Program.cs` |
| JWT config | `PortalNegocioWS/appsettings.json` → `JWT:SecretKey` |
| Oracle connection | `PortalNegocioWS/appsettings.json` → `ConnectionStrings:PortalNegocio` |
| DI registration | `PortalNegocioWS/Installers/BusinessInstaller.cs` |
| AutoMapper mappings | `PortalNegocioWS/Mappings/MappingProfile.cs` |
| NuGet versions | `Directory.Packages.props` (root) |
| DB schema | `Negocio.Data/DataContext.lqml` |
| Generated DB context | `Negocio.Data/DataContext.Designer.cs` |

## Adding New Features

**New domain:**
1. Create `Negocio.Model/{Domain}/{Domain}Model.cs`
2. Create `Negocio.Business/{Domain}/I{Domain}.cs` + `{Domain}.cs`
3. Register in `BusinessInstaller.cs`: `services.AddScoped<I{Domain}, {Domain}>()`
4. Create `PortalNegocioWS/Controllers/{Domain}Controller.cs`
5. Add mappings to `MappingProfile.cs`

**New background job:**
1. Create `PortalNegocioWS/Services/{JobName}Job.cs` extending `CronJobService`
2. Register via `services.AddHostedService<{JobName}Job>()` in `Program.cs`
3. Add cron schedule to `appsettings.json`

## Naming Conventions

| Artifact | Pattern | Example |
|----------|---------|---------|
| Projects | `Negocio.{Layer}` | `Negocio.Business` |
| Namespaces | Match project + folder | `Negocio.Business.Solicitud` |
| Controllers | `{Domain}Controller` | `SolicitudController` |
| Business impl | `{Domain}` | `SolicitudBusiness` |
| Business interface | `I{Domain}` | `ISolicitudBusiness` |
| DTOs | `{Domain}Model` | `SolicitudModel` |
| DB entities | `POGE_{TABLE}` | `POGE_SOLICITUD` |
| DB columns | `POGE_{COLUMN}` | `POGE_ID`, `POGE_ESTADO` |
