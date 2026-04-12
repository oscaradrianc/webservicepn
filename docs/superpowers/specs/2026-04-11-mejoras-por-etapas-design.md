# Plan de Mejoras por Etapas — Portal Negocios Web Service

**Fecha:** 2026-04-11  
**Stack:** ASP.NET Core 9.0 · Devart LinqConnect (Oracle) · Negocio.Business / Negocio.Data / Negocio.Model / PortalNegocioWS  
**Estrategia:** Capas concéntricas — cada etapa es desplegable sin romper el contrato de la API (URLs y JSON request/response se mantienen igual).  
**Prioridad:** Seguridad → Mantenibilidad → Modernización .NET 9

---

## Etapa 1: Seguridad (Crítica)

### 1.1 — Inyección SQL en `GetConstante`

**Archivo:** `Negocio.Business/Utilidades/General.cs:187`  
**Problema:** Se usa `string.Format` para construir la query SQL directamente. Permite SQL injection.

```csharp
// ACTUAL (vulnerable)
cx.ExecuteQuery<string>(string.Format(
    "SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", constante))

// SOLUCIÓN — parámetro posicional de LinqConnect (no string.Format)
cx.ExecuteQuery<string>(
    "SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA={0}", constante)
```

> LinqConnect soporta parámetros posicionales en `ExecuteQuery<T>` de la misma forma que EF, pasando el valor como argumento adicional. El driver lo envía como bind variable a Oracle, no como concatenación.

---

### 1.2 — Secretos en `appsettings.json`

**Archivo:** `PortalNegocioWS/appsettings.json`  
**Problema:** Connection string con credenciales Oracle, JWT SecretKey y EncryptedKey están en texto plano y versionados en git.

**Solución:**
- Usar `dotnet user-secrets` para desarrollo local.
- En producción, leer desde variables de entorno (`CONNECTIONSTRINGS__PORTALNEGOCIOS`, `JWT__SECRETKEY`, `ENCRYPTEDKEY`).
- ASP.NET Core ya soporta override de `appsettings.json` con environment variables por convención.
- Dejar en `appsettings.json` solo placeholders vacíos o valores no sensibles.
- Agregar regla en `.gitignore` o usar `appsettings.Production.json` no versionado.

---

### 1.3 — Excepción silenciosa en `SendMail`

**Archivo:** `Negocio.Business/Utilidades/General.cs:373`  
**Problema:** `catch (Exception ex) { }` — si el envío de correo falla, el error desaparece sin rastro.

**Solución:** Inyectar `ILogger<UtilidadesBusiness>` y loguear el error. No relanzar (para no interrumpir el flujo), pero sí registrar.

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error enviando correo. Destinatarios: {Destinatarios}", string.Join(", ", listaCorreos));
}
```

---

### 1.4 — JWT sin validación de Issuer/Audience

**Archivo:** `PortalNegocioWS/Installers/AuthenticationInstaller.cs:24-25`  
**Problema:** `ValidateIssuer = false` y `ValidateAudience = false`. Un token firmado con la misma clave desde cualquier origen es aceptado.

**Solución:** Configurar en `appsettings.json`:
```json
"JWT": {
  "SecretKey": "...",
  "Issuer": "portalnegocios-api",
  "Audience": "portalnegocios-frontend"
}
```
Y activar la validación en `AuthenticationInstaller`.

---

### 1.5 — `new Thread()` para notificaciones post-commit

**Archivo:** `Negocio.Business/Solicitud/SolicitudCompra.cs:99-116`  
**Problema:** Se crea un `Thread` manual para enviar notificaciones. No hay manejo de errores, no respeta el ciclo de vida del request, puede causar race conditions con el contexto de BD que ya se cerró.

**Solución:** Usar `Task.Run` con `try/catch` y logging, o encolar en un `Channel<T>` consumido por un `BackgroundService`. Para esta etapa: migrar a `Task.Run` con manejo de excepción como mínimo.

```csharp
_ = Task.Run(async () =>
{
    try { _notificacion.GenerarNotificacion("autorizagerencia", request); }
    catch (Exception ex) { _logger.LogError(ex, "Error enviando notificación gerencia"); }
});
```

---

### 1.6 — Validación de modelos incompleta

**Problema:** Los controllers validan solo `if (request == null)` manualmente, pero no usan las data annotations del modelo. Campos requeridos (ej. `Username`, `Password` en `LoginRequest`) pueden llegar vacíos sin ser rechazados.

**Solución:** 
- Agregar atributos `[Required]`, `[StringLength]` a los DTOs de request críticos (`LoginRequest`, `SolicitudCompra`, `ChangePasswordRequest`).
- `[ApiController]` ya devuelve 400 automáticamente con el detalle de validación sin código adicional.

---

## Etapa 2: Mantenibilidad

### 2.1 — DataContext sin inyección de dependencias

**Problema:** Todos los métodos de negocio hacen `new PORTALNEGOCIODataContext()` directamente. Imposible testear, difícil de auditar conexiones.

**Solución:** Crear una interfaz `IDataContextFactory` y su implementación, registrada como `Scoped` en DI:

```csharp
public interface IDataContextFactory
{
    PORTALNEGOCIODataContext Create();
}

public class DataContextFactory : IDataContextFactory
{
    public PORTALNEGOCIODataContext Create() => new PORTALNEGOCIODataContext();
}
```

Los servicios de negocio reciben `IDataContextFactory` en el constructor y reemplazan `new PORTALNEGOCIODataContext()` por `_factory.Create()`. No se modifica el `DataContext.Designer.cs` generado.

---

### 2.2 — `UtilidadesBusiness` es un God Object

**Archivo:** `Negocio.Business/Utilidades/General.cs` (524 líneas)  
**Problema:** Mezcla responsabilidades: email, criptografía, consultas de BD, utilidades de string, decodificación de archivos.

**Solución:** Extraer `IEmailService` con su implementación `SmtpEmailService`:
- Mueve `SendMail`, `GetConstante` (para config de mail), `ConvertirMensaje` a `IEmailService`.
- `IUtilidades` queda con: helpers de datos (GetSecuencia, IsDecimal, DecodificarArchivo, ObtenerBlob, GetStringEncriptado, GetRandomKey).
- Las consultas de catálogo (`ObtenerMunicipios`, `ObtenerPais`, etc.) se mueven a `IUtilidades` o a sus servicios de negocio correspondientes.

---

### 2.3 — Async ficticio con `Task.Run` sobre LINQ síncrono

**Problema:** Métodos como `ObtenerClaseValor`, `ObtenerActividadEconomica`, `ObtenerMunicipio` usan `Task.Run(() => linqQuery)`. LinqConnect no tiene API verdaderamente async; esto solo mueve el trabajo a un thread del pool sin beneficio real de I/O async.

**Solución:** Hacer estos métodos síncronos en la interfaz y en la implementación. Actualizar las firmas en `IUtilidades` e `ISolicitudCompra`. Los controllers que los llamen serán síncronos también (correcto para LinqConnect).

---

### 2.4 — Código comentado y archivos muertos

**Eliminar:**
- `Program.cs`: clase `Program` original comentada (líneas 97–138).
- `StartupCopia.cs` (excluida del build en `.csproj` pero sigue en disco).
- `CacheInstaller.cs`: cuerpo completamente comentado. Si Redis no está en uso, simplificar a clase vacía o eliminar el installer.
- Consulta SQL comentada en `Login.cs` (líneas 141–200+).
- Método comentado `GetConstante(string, PORTALNEGOCIODataContext)` en `General.cs`.

---

### 2.5 — Inconsistencia en respuestas de controllers

**Problema:** 
- `LoginController`: devuelve `Response<Usuario>` y `ResponseStatus` directamente (no `IActionResult`).
- `SolicitudController`: mezcla `Ok()`, `Content(HttpStatusCode.BadRequest.ToString(), result)`.
- `CotizacionController`: algunos métodos devuelven `Response<T>` directamente, otros `IActionResult`.

**Solución:** Estandarizar todos a `IActionResult`:
```csharp
// Éxito
return Ok(new Response<T> { Status = new ResponseStatus { Status = "OK" }, Data = data });

// Error controlado
return BadRequest(new Response<T> { Status = new ResponseStatus { Status = "ERROR", Message = msg } });
```

---

## Etapa 3: Modernización .NET 9

### 3.1 — Swashbuckle → OpenAPI nativo .NET 9

**Problema:** `Swashbuckle.AspNetCore 9.0.4` está en camino de deprecación en .NET 9. Microsoft provee OpenAPI nativo con `Microsoft.AspNetCore.OpenApi`.

**Solución:**
- Remover `Swashbuckle.AspNetCore` de `Directory.Packages.props` y del `.csproj`.
- Agregar `builder.Services.AddOpenApi()` y `app.MapOpenApi()`.
- Para UI de exploración, usar Scalar (recomendado) o mantener SwaggerUI apuntando al endpoint nativo.

---

### 3.2 — Serilog no está activo

**Problema:** `Serilog.AspNetCore` está instalado y configurado en `appsettings.json`, pero en `Program.cs` falta `builder.Host.UseSerilog()`. El logging usa el proveedor por defecto de .NET.

**Solución:**
```csharp
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```
Esto activa el sink de File y Console definidos en `appsettings.json`.

---

### 3.3 — Compresión de respuestas no activada

**Problema:** `CompressInstaller` registra `ResponseCompression` pero en `Program.cs` falta `app.UseResponseCompression()` antes de `app.MapControllers()`.

**Solución:** Agregar la línea faltante en `Program.cs`. Sin cambios en el installer.

---

### 3.4 — Health Checks

**Solución:** Agregar endpoint `/health` con check de conexión Oracle:
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<OracleHealthCheck>("oracle");

app.MapHealthChecks("/health");
```
Implementar `OracleHealthCheck : IHealthCheck` que abra y cierre una conexión al DataContext.

---

### 3.5 — Dependencias innecesarias o en conflicto

**Problema:**
- `EntityFramework 6.5.1` (EF6 clásico) coexiste con `Microsoft.EntityFrameworkCore.Relational 9.0.9` y con LinqConnect. Solo se usa LinqConnect realmente.
- `Microsoft.AspNetCore.Mvc.Abstractions 2.2.0` es de ASP.NET Core 2.2, innecesario con .NET 9.
- `Microsoft.AspNetCore.Mvc.Core 2.2.5` ídem.

**Solución:**
- Remover `EntityFramework 6.5.1` de `Negocio.Business.csproj` si no hay uso real (verificar imports).
- Remover `Microsoft.AspNetCore.Mvc.Abstractions` y `Microsoft.AspNetCore.Mvc.Core` 2.2.x de `Directory.Packages.props`.
- Verificar que `Oracle.EntityFrameworkCore 9.23.90` también sea innecesario dado que se usa LinqConnect.

---

## Resumen de archivos afectados por etapa

| Etapa | Archivos principales |
|-------|---------------------|
| 1 | `General.cs`, `appsettings.json`, `AuthenticationInstaller.cs`, `SolicitudCompra.cs`, `LoginRequest.cs`, `ChangePasswordRequest.cs` |
| 2 | `General.cs`, `IUtilidades.cs`, `Program.cs`, todos los controllers, `BusinessInstaller.cs`, `DataContext` (solo DI) |
| 3 | `Program.cs`, `SwaggerInstaller.cs`, `Directory.Packages.props`, nuevo `OracleHealthCheck.cs` |

## Contrato de API

Los siguientes elementos NO cambian en ninguna etapa:
- Rutas de todos los controllers (`api/[controller]/[action]`)
- Estructura JSON de request y response
- Esquema de autenticación JWT Bearer
- Nombres de namespaces visibles desde el exterior
