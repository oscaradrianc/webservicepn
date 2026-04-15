# CONVENTIONS.md — Code Style and Patterns

## Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `SolicitudService`, `ProveedorController` |
| Interfaces | `I` prefix + PascalCase | `ISolicitudService`, `IProveedorBusiness` |
| Private fields | `_camelCase` | `_solicitudBusiness`, `_mapper` |
| Methods | PascalCase | `GetSolicitudes()`, `CrearCotizacion()` |
| Parameters | camelCase | `solicitudId`, `proveedorModel` |
| DTOs/Models | PascalCase + suffix | `SolicitudModel`, `ProveedorRequest` |
| Domain names | Spanish (matches DB) | `Solicitud`, `Cotizacion`, `Proveedor` |

## Response Wrapper

All endpoints return `Response<T>`:

```csharp
return Ok(new Response<SolicitudModel>
{
    Data = result,
    ResponseStatus = new ResponseStatus
    {
        Status = "Success",
        Message = "Operación exitosa"
    }
});
```

Error case:
```csharp
return Ok(new Response<SolicitudModel>
{
    ResponseStatus = new ResponseStatus
    {
        Status = "Error",
        Message = ex.Message
    }
});
```

## Error Handling

- Controllers wrap business calls in `try/catch`, return `Response<T>` with `Status = "Error"`
- Business layer may throw exceptions up to the controller
- No global exception middleware — each controller handles its own errors
- `ILogger<T>` injected in controllers for logging exceptions

## Data Access Pattern

Business services create `PORTALNEGOCIODataContext` directly (no repository):

```csharp
using var db = new PORTALNEGOCIODataContext(_connectionString);
var result = db.POGE_SOLICITUD
    .Where(s => s.POGE_ID == id)
    .Select(s => _mapper.Map<SolicitudModel>(s))
    .FirstOrDefault();
```

Writes:
```csharp
db.POGE_SOLICITUD.InsertOnSubmit(entity);
db.SubmitChanges();
```

Batch operations:
```csharp
using var tx = db.Connection.BeginTransaction();
db.Transaction = tx;
// ... multiple operations
tx.Commit();
```

## Mapping

Two approaches coexist:

**AutoMapper** (preferred for complex objects):
```csharp
var model = _mapper.Map<SolicitudModel>(entity);
```

**`GetModelObject` extension method** (used in some business classes):
```csharp
var model = entity.GetModelObject<SolicitudModel>();
```

Mappings are centralized in `MappingProfile.cs` — add new mappings there.

## Code Organization

- `#region` blocks used to group related methods in large service classes
- Business layer classes can be 500–1000+ lines; use regions to navigate
- Async method signatures (`Task<T>`) used even when body is synchronous LINQ

## Dependency Injection

Registered via installer classes (`IInstaller`):

```csharp
public class BusinessInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ISolicitudBusiness, SolicitudBusiness>();
        // ...
    }
}
```

Business services: `Scoped` lifetime.
AutoMapper: `Singleton` lifetime.
Background jobs: `AddHostedService`.

## File/Folder Conventions

- Controllers in `PortalNegocioWS/Controllers/`
- Business interfaces + impls together in `Negocio.Business/` (flat or by domain)
- Models in `Negocio.Model/{Domain}/`
- Background jobs in `PortalNegocioWS/Services/`
