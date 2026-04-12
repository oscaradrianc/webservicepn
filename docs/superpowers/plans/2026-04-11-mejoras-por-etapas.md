# Mejoras por Etapas — Portal Negocios WS

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Aplicar mejoras incrementales de seguridad, mantenibilidad y modernización .NET 9 sin romper el contrato de la API (rutas, JSON request/response).

**Architecture:** Capas concéntricas — cada etapa es desplegable independientemente. Etapa 1 corrige vulnerabilidades con cambios quirúrgicos. Etapa 2 refactoriza la capa de negocio internamente. Etapa 3 moderniza la infraestructura del host.

**Tech Stack:** ASP.NET Core 9.0 · Devart LinqConnect (Devart.Data.Oracle.Linq 4.9.2033 / Devart.Data.Linq 5.0.0) · AutoMapper 14 · Serilog 9 · JWT Bearer 9 · Cronos 0.11.1

> **Nota sobre tests:** El proyecto no tiene proyecto de tests y LinqConnect no expone interfaces mockeables. La verificación se hace con `dotnet build` + prueba manual del endpoint afectado. Cada tarea indica cómo verificar.

---

## Mapa de archivos

| Archivo | Acción | Etapa |
|---------|--------|-------|
| `Negocio.Business/Utilidades/General.cs` | Modificar | 1.1, 1.3, 2.2, 2.3 |
| `Negocio.Business/Utilidades/IUtilidades.cs` | Modificar | 2.2, 2.3 |
| `Negocio.Business/Utilidades/IEmailService.cs` | Crear | 2.2 |
| `Negocio.Business/Utilidades/SmtpEmailService.cs` | Crear | 2.2 |
| `PortalNegocioWS/appsettings.json` | Modificar | 1.2 |
| `PortalNegocioWS/appsettings.Development.json` | Modificar | 1.2 |
| `PortalNegocioWS/Installers/AuthenticationInstaller.cs` | Modificar | 1.4 |
| `Negocio.Business/Solicitud/SolicitudCompra.cs` | Modificar | 1.5 |
| `Negocio.Model/Login/LoginRequest.cs` | Modificar | 1.6 |
| `Negocio.Model/ChangePasswordRequest.cs` | Modificar | 1.6 |
| `Negocio.Data/IDataContextFactory.cs` | Crear | 2.1 |
| `Negocio.Data/DataContextFactory.cs` | Crear | 2.1 |
| `PortalNegocioWS/Installers/BusinessInstaller.cs` | Modificar | 2.1, 2.2 |
| `PortalNegocioWS/Program.cs` | Modificar | 2.4, 3.2, 3.3, 3.4 |
| `PortalNegocioWS/Installers/SwaggerInstaller.cs` | Modificar | 3.1 |
| `PortalNegocioWS/HealthChecks/OracleHealthCheck.cs` | Crear | 3.4 |
| `Directory.Packages.props` | Modificar | 3.5 |
| `Negocio.Business/Negocio.Business.csproj` | Modificar | 3.5 |
| Todos los controllers | Modificar | 2.5 |

---

## ETAPA 1: Seguridad

---

### Task 1: Corregir SQL Injection en `GetConstante`

**Spec:** 1.1  
**Files:**
- Modify: `Negocio.Business/Utilidades/General.cs` (método `GetConstante`, aprox. línea 187)

- [ ] **Paso 1: Localizar el método**

  Abrir `Negocio.Business/Utilidades/General.cs` y encontrar:
  ```csharp
  return cx.ExecuteQuery<string>(string.Format("SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", constante)).FirstOrDefault();
  ```

- [ ] **Paso 2: Reemplazar con parámetro posicional**

  Reemplazar esa línea con:
  ```csharp
  return cx.ExecuteQuery<string>(
      "SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA={0}",
      constante
  ).FirstOrDefault();
  ```

  > LinqConnect's `ExecuteQuery<T>` acepta parámetros adicionales después del string de query. Los pasa como bind variables a Oracle — equivalente a `OracleParameter`, nunca concatena texto.

- [ ] **Paso 3: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 4: Commit**

  ```bash
  git add Negocio.Business/Utilidades/General.cs
  git commit -m "fix: corregir SQL injection en GetConstante usando parámetro posicional LinqConnect"
  ```

---

### Task 2: Remover secretos de `appsettings.json`

**Spec:** 1.2  
**Files:**
- Modify: `PortalNegocioWS/appsettings.json`
- Modify: `PortalNegocioWS/appsettings.Development.json`

- [ ] **Paso 1: Verificar si existe `appsettings.Development.json`**

  ```bash
  ls PortalNegocioWS/appsettings*.json
  ```

- [ ] **Paso 2: Mover secretos a `appsettings.Development.json`**

  Copiar los valores reales de `appsettings.json` a `appsettings.Development.json` (crear si no existe). El archivo de desarrollo debe tener:
  ```json
  {
    "JWT": {
      "SecretKey": "<valor-actual-del-appsettings>",
      "Issuer": "portalnegocios-api",
      "Audience": "portalnegocios-frontend"
    },
    "EncryptedKey": "<valor-actual>",
    "connectionStrings": {
      "PORTALNEGOCIODataContextConnectionString": "<connection-string-completa-actual>"
    }
  }
  ```

- [ ] **Paso 3: Limpiar `appsettings.json` de secretos**

  En `appsettings.json`, reemplazar los valores sensibles con placeholders vacíos:
  ```json
  {
    "JWT": {
      "SecretKey": "",
      "Issuer": "portalnegocios-api",
      "Audience": "portalnegocios-frontend"
    },
    "EncryptedKey": "",
    "connectionStrings": {
      "PORTALNEGOCIODataContextConnectionString": ""
    }
  }
  ```
  Mantener todas las demás secciones (`Serilog`, `Settings`, `Storage`, `RedisCacheSettings`, `AllowedHosts`) intactas.

- [ ] **Paso 4: Agregar `appsettings.Development.json` a `.gitignore`**

  Abrir (o crear) `.gitignore` en la raíz del repositorio y agregar al final:
  ```
  # Archivos con secretos locales
  PortalNegocioWS/appsettings.Development.json
  ```

- [ ] **Paso 5: Verificar que la app levanta en desarrollo**

  ```bash
  dotnet run --project PortalNegocioWS/PortalNegocioWS.csproj --environment Development
  ```
  Esperado: la app inicia sin excepciones de configuración. Si hay error de "SecretKey vacío", el `AuthenticationInstaller` lo detectará — la solución de JWT issuer/audience (Task 4) incluye validación de valor vacío.

- [ ] **Paso 6: Commit (solo appsettings.json y .gitignore, NO Development)**

  ```bash
  git add PortalNegocioWS/appsettings.json .gitignore
  git commit -m "fix: remover secretos de appsettings.json, mover a Development (no versionado)"
  ```

---

### Task 3: Loguear excepciones silenciosas en `SendMail`

**Spec:** 1.3  
**Files:**
- Modify: `Negocio.Business/Utilidades/General.cs`
- Modify: `Negocio.Business/Utilidades/IUtilidades.cs`

- [ ] **Paso 1: Agregar `ILogger` al constructor de `UtilidadesBusiness`**

  En `General.cs`, localizar la clase `UtilidadesBusiness` (no tiene constructor explícito actualmente). Agregar:
  ```csharp
  using Microsoft.Extensions.Logging;

  public class UtilidadesBusiness : IUtilidades
  {
      private readonly ILogger<UtilidadesBusiness> _logger;

      public UtilidadesBusiness(ILogger<UtilidadesBusiness> logger)
      {
          _logger = logger;
      }
      // ... resto de métodos sin cambios
  ```

- [ ] **Paso 2: Reemplazar el catch vacío en `SendMail`**

  Localizar en `General.cs`:
  ```csharp
  catch (Exception ex)
  {
      
  }
  ```
  Reemplazar con:
  ```csharp
  catch (Exception ex)
  {
      _logger.LogError(ex, "Error enviando correo. Destinatarios: {Destinatarios}", 
          string.Join(", ", listaCorreos));
  }
  ```

- [ ] **Paso 3: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

  Si hay error de resolución de `ILogger<UtilidadesBusiness>`: `UtilidadesBusiness` ya está registrado como `Scoped` en `BusinessInstaller`, el framework inyecta `ILogger<T>` automáticamente.

- [ ] **Paso 4: Commit**

  ```bash
  git add Negocio.Business/Utilidades/General.cs
  git commit -m "fix: loguear error en SendMail en lugar de tragarlo silenciosamente"
  ```

---

### Task 4: Activar validación de Issuer/Audience en JWT

**Spec:** 1.4  
**Files:**
- Modify: `PortalNegocioWS/Installers/AuthenticationInstaller.cs`
- Modify: `PortalNegocioWS/appsettings.json` (ya modificado en Task 2 — los campos `Issuer` y `Audience` ya están)

- [ ] **Paso 1: Actualizar `AuthenticationInstaller.cs`**

  Reemplazar el contenido completo del método `InstallServices`:
  ```csharp
  public void InstallServices(IServiceCollection services, IConfiguration configuration)
  {
      var secretKey = configuration.GetValue<string>("JWT:SecretKey");
      if (string.IsNullOrWhiteSpace(secretKey))
          throw new InvalidOperationException("JWT:SecretKey no está configurado. Verificar appsettings o variables de entorno.");

      var key = Encoding.ASCII.GetBytes(secretKey);

      services.AddAuthentication(x =>
      {
          x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      }).AddJwtBearer(x =>
      {
          x.RequireHttpsMetadata = false;
          x.SaveToken = true;
          x.TokenValidationParameters = new TokenValidationParameters
          {
              ValidateIssuerSigningKey = true,
              IssuerSigningKey = new SymmetricSecurityKey(key),
              ValidateIssuer = true,
              ValidIssuer = configuration.GetValue<string>("JWT:Issuer"),
              ValidateAudience = true,
              ValidAudience = configuration.GetValue<string>("JWT:Audience")
          };
      });
  }
  ```

- [ ] **Paso 2: Verificar que el `LoginBusiness` emite el Issuer/Audience al generar el token**

  Buscar `TokenGenerator.GenerateTokenJwt` en el proyecto:
  ```bash
  grep -r "GenerateTokenJwt" Negocio.Business/ --include="*.cs"
  ```
  Abrir el archivo encontrado y verificar que al crear `JwtSecurityToken` incluye `issuer` y `audience`. Si no los tiene, actualizarlos para que lean de `IConfiguration`.

  > El `LoginController` pasa `_configuration` al `LoginBusiness.Authenticate`. Esa configuración ya tiene `JWT:Issuer` y `JWT:Audience` disponibles.

- [ ] **Paso 3: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 4: Verificar manualmente**

  Iniciar la app y hacer POST a `api/login/authenticate` con credenciales válidas. El token retornado debe ser válido para endpoints protegidos. Si hay 401, verificar que `TokenGenerator` emite `Issuer`/`Audience` iguales a los de `appsettings.Development.json`.

- [ ] **Paso 5: Commit**

  ```bash
  git add PortalNegocioWS/Installers/AuthenticationInstaller.cs
  git commit -m "fix: activar validación de Issuer y Audience en JWT"
  ```

---

### Task 5: Reemplazar `new Thread()` con `Task.Run` para notificaciones

**Spec:** 1.5  
**Files:**
- Modify: `Negocio.Business/Solicitud/SolicitudCompra.cs` (líneas 99–116)

- [ ] **Paso 1: Agregar `ILogger` a `SolicitudBusiness`**

  En `SolicitudCompra.cs`, modificar el constructor:
  ```csharp
  private readonly INotificacion _notificacion;
  private readonly IUtilidades _utilidades;
  private readonly IStorageService _storageService;
  private readonly ILogger<SolicitudBusiness> _logger;

  public SolicitudBusiness(INotificacion notificacion, IUtilidades utilidades, 
      IStorageService storageService, ILogger<SolicitudBusiness> logger)
  {
      _notificacion = notificacion;
      _utilidades = utilidades;
      _storageService = storageService;
      _logger = logger;
  }
  ```

- [ ] **Paso 2: Reemplazar el bloque de `new Thread()` en `RegistrarSolicitud`**

  Localizar (líneas ~99–116):
  ```csharp
  if(request.NumeroSAIA == null)
  {
      Thread t = new Thread(() =>
      {
          _notificacion.GenerarNotificacion("autorizagerencia", request);
      });
      t.Start();
      t.IsBackground = true;
  }
  else
  {
      Thread t = new Thread(() =>
      {
          _notificacion.GenerarNotificacion("autorizacompras", request);
      });
      t.Start();
      t.IsBackground = true;
  }
  ```

  Reemplazar con:
  ```csharp
  var tipoNotificacion = request.NumeroSAIA == null ? "autorizagerencia" : "autorizacompras";
  _ = Task.Run(() =>
  {
      try
      {
          _notificacion.GenerarNotificacion(tipoNotificacion, request);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error enviando notificación {Tipo} para solicitud {Codigo}", 
              tipoNotificacion, request.CodigoSolicitud);
      }
  });
  ```

- [ ] **Paso 3: Verificar que no hay más `new Thread` en el archivo**

  ```bash
  grep -n "new Thread" Negocio.Business/Solicitud/SolicitudCompra.cs
  ```
  Esperado: sin resultados.

- [ ] **Paso 4: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 5: Commit**

  ```bash
  git add Negocio.Business/Solicitud/SolicitudCompra.cs
  git commit -m "fix: reemplazar new Thread() con Task.Run con manejo de excepción para notificaciones"
  ```

---

### Task 6: Agregar Data Annotations a modelos de request críticos

**Spec:** 1.6  
**Files:**
- Modify: `Negocio.Model/Login/LoginRequest.cs`
- Modify: `Negocio.Model/ChangePasswordRequest.cs`

- [ ] **Paso 1: Leer los archivos actuales**

  ```bash
  cat Negocio.Model/Login/LoginRequest.cs
  cat Negocio.Model/ChangePasswordRequest.cs
  ```

- [ ] **Paso 2: Actualizar `LoginRequest.cs`**

  Reemplazar contenido manteniendo namespace existente, agregar annotations:
  ```csharp
  using System.ComponentModel.DataAnnotations;

  namespace Negocio.Model
  {
      public class LoginRequest
      {
          [Required(ErrorMessage = "El identificador de usuario es requerido")]
          public string Username { get; set; }

          [Required(ErrorMessage = "La contraseña es requerida")]
          public string Password { get; set; }

          [Required(ErrorMessage = "El origen es requerido")]
          public string Origen { get; set; }
      }
  }
  ```

- [ ] **Paso 3: Actualizar `ChangePasswordRequest.cs`**

  Leer el archivo primero para conocer sus propiedades actuales, luego agregar `[Required]` a cada propiedad existente que sea mandatoria. Ejemplo (ajustar según propiedades reales):
  ```csharp
  using System.ComponentModel.DataAnnotations;

  namespace Negocio.Model
  {
      public class ChangePasswordRequest
      {
          [Required]
          public string Username { get; set; }

          [Required]
          public string ClaveActual { get; set; }

          [Required]
          [MinLength(6, ErrorMessage = "La nueva clave debe tener al menos 6 caracteres")]
          public string ClaveNueva { get; set; }
      }
  }
  ```

- [ ] **Paso 4: Verificar que `[ApiController]` está en los controllers de Login**

  ```bash
  grep -n "ApiController" PortalNegocioWS/Controllers/LoginController.cs
  ```
  Esperado: línea con `[ApiController]`. Si no existe, agregar el atributo a la clase.

- [ ] **Paso 5: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 6: Verificar manualmente**

  Hacer POST a `api/login/authenticate` con body vacío `{}`. Esperado: HTTP 400 con detalle de validación automático de `[ApiController]`.

- [ ] **Paso 7: Commit**

  ```bash
  git add Negocio.Model/Login/LoginRequest.cs Negocio.Model/ChangePasswordRequest.cs
  git commit -m "fix: agregar data annotations a modelos de request críticos para validación automática"
  ```

---

## ETAPA 2: Mantenibilidad

---

### Task 7: Crear `IDataContextFactory` para eliminar `new DataContext()` directo

**Spec:** 2.1  
**Files:**
- Create: `Negocio.Data/IDataContextFactory.cs`
- Create: `Negocio.Data/DataContextFactory.cs`
- Modify: `PortalNegocioWS/Installers/BusinessInstaller.cs`

- [ ] **Paso 1: Crear `IDataContextFactory.cs`**

  ```csharp
  // Negocio.Data/IDataContextFactory.cs
  namespace Negocio.Data
  {
      public interface IDataContextFactory
      {
          PORTALNEGOCIODataContext Create();
      }
  }
  ```

- [ ] **Paso 2: Crear `DataContextFactory.cs`**

  ```csharp
  // Negocio.Data/DataContextFactory.cs
  namespace Negocio.Data
  {
      public class DataContextFactory : IDataContextFactory
      {
          public PORTALNEGOCIODataContext Create() => new PORTALNEGOCIODataContext();
      }
  }
  ```

- [ ] **Paso 3: Registrar en `BusinessInstaller.cs`**

  Agregar al inicio del método `InstallServices`:
  ```csharp
  services.AddScoped<IDataContextFactory, DataContextFactory>();
  ```

  Añadir using si hace falta:
  ```csharp
  using Negocio.Data;
  ```

- [ ] **Paso 4: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 5: Commit**

  ```bash
  git add Negocio.Data/IDataContextFactory.cs Negocio.Data/DataContextFactory.cs PortalNegocioWS/Installers/BusinessInstaller.cs
  git commit -m "refactor: agregar IDataContextFactory para centralizar creación del DataContext"
  ```

  > **Nota:** La migración de todos los `new PORTALNEGOCIODataContext()` a `_factory.Create()` es un refactor de largo aliento. Este task solo crea la infraestructura. Se puede migrar servicio por servicio en PRs separados.

---

### Task 8: Extraer `IEmailService` de `UtilidadesBusiness`

**Spec:** 2.2  
**Files:**
- Create: `Negocio.Business/Utilidades/IEmailService.cs`
- Create: `Negocio.Business/Utilidades/SmtpEmailService.cs`
- Modify: `Negocio.Business/Utilidades/General.cs`
- Modify: `PortalNegocioWS/Installers/BusinessInstaller.cs`

- [ ] **Paso 1: Crear `IEmailService.cs`**

  ```csharp
  // Negocio.Business/Utilidades/IEmailService.cs
  using System.Collections.Generic;

  namespace Negocio.Business.Utilidades
  {
      public interface IEmailService
      {
          void SendMail(List<string> listaCorreos, string asunto, string mensaje, bool bcc = false);
          string ConvertirMensaje(string mensaje, string parametros);
      }
  }
  ```

- [ ] **Paso 2: Crear `SmtpEmailService.cs`**

  Copiar los métodos `SendMail` y `ConvertirMensaje` desde `General.cs`:
  ```csharp
  // Negocio.Business/Utilidades/SmtpEmailService.cs
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Generic;
  using System.Net.Mail;

  namespace Negocio.Business.Utilidades
  {
      public class SmtpEmailService : IEmailService
      {
          private readonly IUtilidades _utilidades;
          private readonly ILogger<SmtpEmailService> _logger;

          public SmtpEmailService(IUtilidades utilidades, ILogger<SmtpEmailService> logger)
          {
              _utilidades = utilidades;
              _logger = logger;
          }

          public void SendMail(List<string> listaCorreos, string asunto, string mensaje, bool bcc = false)
          {
              try
              {
                  MailMessage mail = new MailMessage();
                  string servidorMail = _utilidades.GetConstante("serv_mail");
                  string sslMail = _utilidades.GetConstante("ssl_mail");
                  string pwdMail = _utilidades.GetConstante("pwd_mail");
                  string usrMail = _utilidades.GetConstante("usr_mail");
                  string sendMail = _utilidades.GetConstante("send_mail");
                  int portMail = Convert.ToInt32(_utilidades.GetConstante("port_mail"));

                  SmtpClient SmtpServer = new SmtpClient(servidorMail);
                  mail.From = new MailAddress(sendMail);

                  if (bcc)
                      listaCorreos.ForEach(correo => mail.Bcc.Add(correo));
                  else
                      listaCorreos.ForEach(correo => mail.To.Add(correo));

                  mail.Subject = asunto;
                  mail.Body = mensaje;
                  mail.IsBodyHtml = true;
                  SmtpServer.Port = portMail;
                  SmtpServer.Credentials = new System.Net.NetworkCredential(usrMail, pwdMail);
                  SmtpServer.EnableSsl = Convert.ToBoolean(sslMail);
                  SmtpServer.Send(mail);
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error enviando correo. Destinatarios: {Destinatarios}",
                      string.Join(", ", listaCorreos));
              }
          }

          public string ConvertirMensaje(string mensaje, string parametros)
          {
              string result = mensaje;
              string[] param = parametros.Split('|');
              foreach (var item in param)
              {
                  if (!item.Equals(string.Empty))
                  {
                      string[] variable = item.Split('~');
                      result = result.Replace("{$" + variable[0].ToUpper() + "}", variable[1]);
                  }
              }
              return result;
          }
      }
  }
  ```

- [ ] **Paso 3: Remover `SendMail` y `ConvertirMensaje` de `General.cs`**

  En `Negocio.Business/Utilidades/General.cs`, eliminar los métodos `SendMail` y `ConvertirMensaje` completos (ya están en `SmtpEmailService`). También eliminar el `ILogger` agregado en Task 3 de `UtilidadesBusiness` — el logger de `SendMail` ya no vivirá ahí sino en `SmtpEmailService`.

- [ ] **Paso 4: Remover `SendMail` y `ConvertirMensaje` de `IUtilidades.cs`**

  En `IUtilidades.cs`, eliminar las firmas:
  ```csharp
  void SendMail(List<string> listaCorreos, string asunto, string mensaje, bool bcc = false);
  string ConvertirMensaje(string mensaje, string parametros);
  ```

- [ ] **Paso 5: Buscar todos los usos de `SendMail` y `ConvertirMensaje` en el proyecto**

  ```bash
  grep -rn "\.SendMail\|\.ConvertirMensaje\|_utilidades\.SendMail\|_utilidades\.ConvertirMensaje" Negocio.Business/ --include="*.cs"
  ```
  
  Para cada archivo encontrado, agregar `IEmailService` en el constructor del servicio correspondiente y cambiar las llamadas de `_utilidades.SendMail` → `_emailService.SendMail` y `_utilidades.ConvertirMensaje` → `_emailService.ConvertirMensaje`.

- [ ] **Paso 6: Registrar `IEmailService` en `BusinessInstaller.cs`**

  Agregar:
  ```csharp
  services.AddScoped<IEmailService, SmtpEmailService>();
  ```
  
  Añadir using:
  ```csharp
  using Negocio.Business.Utilidades;
  ```

- [ ] **Paso 7: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`. Si hay errores de compilación sobre métodos no encontrados, significa que quedan usos de `_utilidades.SendMail` sin migrar — ubicarlos con `grep` y corregirlos.

- [ ] **Paso 8: Commit**

  ```bash
  git add Negocio.Business/Utilidades/IEmailService.cs \
          Negocio.Business/Utilidades/SmtpEmailService.cs \
          Negocio.Business/Utilidades/General.cs \
          Negocio.Business/Utilidades/IUtilidades.cs \
          PortalNegocioWS/Installers/BusinessInstaller.cs
  git add -u  # captura otros archivos modificados en paso 5
  git commit -m "refactor: extraer IEmailService de UtilidadesBusiness para separar responsabilidades"
  ```

---

### Task 9: Eliminar async ficticio (`Task.Run` sobre LINQ síncrono)

**Spec:** 2.3  
**Files:**
- Modify: `Negocio.Business/Utilidades/General.cs`
- Modify: `Negocio.Business/Utilidades/IUtilidades.cs`

- [ ] **Paso 1: Identificar todos los métodos con `Task.Run` sobre LINQ en `IUtilidades`**

  ```bash
  grep -n "Task.Run\|async Task" Negocio.Business/Utilidades/General.cs
  grep -n "Task\b" Negocio.Business/Utilidades/IUtilidades.cs
  ```

- [ ] **Paso 2: Cambiar `ObtenerClaseValor` a síncrono**

  En `IUtilidades.cs`, cambiar:
  ```csharp
  Task<List<ClaseValor>> ObtenerClaseValor(int idClase);
  ```
  Por:
  ```csharp
  List<ClaseValor> ObtenerClaseValor(int idClase);
  ```

  En `General.cs`, cambiar la implementación:
  ```csharp
  // ANTES
  public async Task<List<ClaseValor>> ObtenerClaseValor(int idClase)
  {
      using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
      {
          var lta = (...).ToList();
          return await Task.Run(() => lta);
      }
  }

  // DESPUÉS
  public List<ClaseValor> ObtenerClaseValor(int idClase)
  {
      using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
      {
          return (from c in cx.POGECLASEs
                  join v in cx.POGECLASEVALORs on c.CLASCLASE equals v.CLASCLASE
                  where c.CLASCLASE == idClase && v.CLVAESTADO == "A"
                  orderby v.CLVAVALOR
                  select new ClaseValor 
                  { 
                      Clase = (int)c.CLASCLASE, 
                      IdClaseValor = (int)v.CLVACLASEVALOR, 
                      CodigoValor = (int)v.CLVACODIGOVALOR, 
                      Valor = v.CLVAVALOR, 
                      Estado = v.CLVAESTADO, 
                      Descripcion = v.CLVADESCRIPCION 
                  }).ToList();
      }
  }
  ```

- [ ] **Paso 3: Repetir para `ObtenerActividadEconomica(string codigoCIIU)` y `ObtenerMunicipio(int idMunicipio)`**

  Aplicar el mismo patrón: eliminar `async`, eliminar `Task.Run`, retornar directamente el resultado de la query LINQ. Actualizar firmas en `IUtilidades.cs`.

- [ ] **Paso 4: Buscar callers de estos métodos y actualizar `await`**

  ```bash
  grep -rn "await.*ObtenerClaseValor\|await.*ObtenerActividadEconomica\|await.*ObtenerMunicipio" \
    Negocio.Business/ PortalNegocioWS/ --include="*.cs"
  ```
  
  Para cada caller, eliminar el `await` y ajustar si el método caller era `async` solo por esta llamada (puede hacerse síncrono también si no tiene otras operaciones async reales).

- [ ] **Paso 5: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 6: Commit**

  ```bash
  git add -u
  git commit -m "refactor: eliminar Task.Run ficticio sobre LINQ síncrono de LinqConnect"
  ```

---

### Task 10: Eliminar código muerto y comentado

**Spec:** 2.4  
**Files:**
- Modify: `PortalNegocioWS/Program.cs`
- Modify: `PortalNegocioWS/Installers/CacheInstaller.cs`
- Modify: `Negocio.Business/Login/Login.cs`
- Modify: `Negocio.Business/Utilidades/General.cs`
- Delete: `PortalNegocioWS/StartupCopia.cs` (si existe en disco)

- [ ] **Paso 1: Limpiar `Program.cs`**

  Eliminar las líneas 97–138 de `Program.cs` (el bloque `/* public class Program { ... } */` comentado).

- [ ] **Paso 2: Simplificar `CacheInstaller.cs`**

  Reemplazar el contenido de `CacheInstaller.cs` con una clase mínima (el installer vacío no hace nada pero mantiene el patrón):
  ```csharp
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;

  namespace PortalNegocioWS.Installers
  {
      // Redis está deshabilitado. Descomentar cuando se active.
      public class CacheInstaller : IInstaller
      {
          public void InstallServices(IServiceCollection services, IConfiguration configuration) { }
      }
  }
  ```

- [ ] **Paso 3: Limpiar `Login.cs`**

  Eliminar el bloque de comentario grande (líneas ~141–200+) que contiene la consulta SQL original con `cmd.ExecuteReader()`.

- [ ] **Paso 4: Limpiar `General.cs`**

  Eliminar el método comentado `GetConstante(string nombreConstante, PORTALNEGOCIODataContext ctx)`.  
  Eliminar el método comentado `DecodificarArchivo` duplicado (al final del archivo).

- [ ] **Paso 5: Eliminar `StartupCopia.cs` si existe**

  ```bash
  ls PortalNegocioWS/StartupCopia.cs 2>/dev/null && echo "existe" || echo "no existe"
  ```
  Si existe: eliminarlo del disco y del `.csproj` si aún está referenciado (aunque el `.csproj` ya lo excluye con `<Compile Remove>`).

- [ ] **Paso 6: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 7: Commit**

  ```bash
  git add -u
  git commit -m "refactor: eliminar código muerto, comentado y archivos obsoletos"
  ```

---

### Task 11: Estandarizar respuestas de controllers a `IActionResult`

**Spec:** 2.5  
**Files:**
- Modify: `PortalNegocioWS/Controllers/LoginController.cs`
- Modify: `PortalNegocioWS/Controllers/SolicitudController.cs`
- Modify: `PortalNegocioWS/Controllers/CotizacionController.cs`

> **Contrato mantenido:** Los clientes reciben el mismo JSON. Solo cambia el mecanismo interno (el status HTTP explícito se envía correctamente en lugar de 200 siempre).

- [ ] **Paso 1: Actualizar `LoginController.cs`**

  Cambiar los métodos para devolver `IActionResult`:
  ```csharp
  [HttpPost]
  [EnableCors]
  [Route("authenticate")]
  public IActionResult Authenticate(LoginRequest login)
  {
      var resp = _loginBusiness.Authenticate(login, _configuration);
      return Ok(resp);
  }

  [HttpPost]
  [EnableCors]
  [Route("changepassword")]
  public IActionResult ChangePassword(ChangePasswordRequest credentials)
  {
      var resp = _loginBusiness.ChangePassword(credentials, _configuration);
      if (resp.Status == Configuracion.StatusError)
          return BadRequest(resp);
      return Ok(resp);
  }

  [HttpPost]
  [EnableCors]
  [Route("resetpassword")]
  public IActionResult ResetPassword(ResetPassRequest request)
  {
      var resp = _loginBusiness.ResetPassword(request, _configuration);
      if (resp.Status == Configuracion.StatusError)
          return BadRequest(resp);
      return Ok(resp);
  }
  ```

  > **Nota:** La validación `if (login == null)` se elimina porque `[ApiController]` + `[Required]` (Task 6) ya rechaza el request antes de llegar al action.

- [ ] **Paso 2: Corregir `SolicitudController.cs` — método `ActualizarSolicitud`**

  Localizar:
  ```csharp
  return Content(HttpStatusCode.BadRequest.ToString(), result);
  ```
  Reemplazar con:
  ```csharp
  return BadRequest(new ResponseStatus { Status = Configuracion.StatusError, Message = result });
  ```
  Aplicar el mismo cambio en `RegistrarSolicitud` y cualquier otro endpoint del controller que use `Content(HttpStatusCode...)`.

- [ ] **Paso 3: Verificar controllers restantes**

  ```bash
  grep -rn "Content(HttpStatusCode\|return new Response\b" PortalNegocioWS/Controllers/ --include="*.cs"
  ```
  Para cada resultado, reemplazar con la forma `Ok(...)` / `BadRequest(...)` equivalente.

- [ ] **Paso 4: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 5: Commit**

  ```bash
  git add PortalNegocioWS/Controllers/
  git commit -m "refactor: estandarizar respuestas de controllers a IActionResult con Ok/BadRequest"
  ```

---

## ETAPA 3: Modernización .NET 9

---

### Task 12: Activar Serilog correctamente

**Spec:** 3.2  
**Files:**
- Modify: `PortalNegocioWS/Program.cs`

- [ ] **Paso 1: Agregar `UseSerilog` antes de `builder.Build()`**

  En `Program.cs`, después de `var builder = WebApplication.CreateBuilder(args);` y antes del bloque de installers, agregar:
  ```csharp
  builder.Host.UseSerilog((context, config) =>
      config.ReadFrom.Configuration(context.Configuration));
  ```

  Agregar el using si falta:
  ```csharp
  using Serilog;
  ```

- [ ] **Paso 2: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 3: Verificar que los logs llegan al archivo**

  Iniciar la app y hacer cualquier request. Verificar que se crea/actualiza `PortalNegocioWS/Logs/log.txt` con entradas estructuradas.

- [ ] **Paso 4: Commit**

  ```bash
  git add PortalNegocioWS/Program.cs
  git commit -m "feat: activar Serilog con configuración desde appsettings"
  ```

---

### Task 13: Activar compresión de respuestas

**Spec:** 3.3  
**Files:**
- Modify: `PortalNegocioWS/Program.cs`

- [ ] **Paso 1: Agregar `UseResponseCompression` en el pipeline**

  En `Program.cs`, localizar la sección de middleware (después de `var app = builder.Build()`). Agregar antes de `app.MapControllers()`:
  ```csharp
  app.UseResponseCompression();
  ```

  El orden correcto del pipeline debe quedar:
  ```csharp
  app.UseResponseCompression(); // PRIMERO — antes de que se genere la respuesta
  app.UseCors("OrigenLocal");
  app.UseAuthorization();
  app.MapControllers();
  ```

- [ ] **Paso 2: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```

- [ ] **Paso 3: Commit**

  ```bash
  git add PortalNegocioWS/Program.cs
  git commit -m "feat: activar middleware de compresión de respuestas HTTP"
  ```

---

### Task 14: Reemplazar Swashbuckle por OpenAPI nativo .NET 9

**Spec:** 3.1  
**Files:**
- Modify: `Directory.Packages.props`
- Modify: `PortalNegocioWS/PortalNegocioWS.csproj`
- Modify: `PortalNegocioWS/Installers/SwaggerInstaller.cs`
- Modify: `PortalNegocioWS/Program.cs`

- [ ] **Paso 1: Agregar paquete OpenAPI nativo en `Directory.Packages.props`**

  Agregar en el `<ItemGroup>`:
  ```xml
  <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="9.0.4" />
  <PackageVersion Include="Scalar.AspNetCore" Version="2.5.4" />
  ```

- [ ] **Paso 2: Actualizar `PortalNegocioWS.csproj`**

  Reemplazar:
  ```xml
  <PackageReference Include="Swashbuckle.AspNetCore" />
  ```
  Por:
  ```xml
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
  <PackageReference Include="Scalar.AspNetCore" />
  ```

- [ ] **Paso 3: Actualizar `SwaggerInstaller.cs`**

  Reemplazar el contenido completo:
  ```csharp
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;

  namespace PortalNegocioWS.Installers
  {
      public class SwaggerInstaller : IInstaller
      {
          public void InstallServices(IServiceCollection services, IConfiguration configuration)
          {
              services.AddOpenApi();
          }
      }
  }
  ```

- [ ] **Paso 4: Actualizar `Program.cs` — reemplazar Swagger UI por Scalar**

  Localizar el bloque:
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }
  ```
  Reemplazar con:
  ```csharp
  if (app.Environment.IsDevelopment())
  {
      app.MapOpenApi();
      app.MapScalarApiReference(); // UI en /scalar/v1
  }
  ```
  
  Agregar using:
  ```csharp
  using Scalar.AspNetCore;
  ```

- [ ] **Paso 5: Remover Swashbuckle de `Directory.Packages.props`**

  Eliminar la línea:
  ```xml
  <PackageVersion Include="Swashbuckle.AspNetCore" Version="9.0.4" />
  ```

- [ ] **Paso 6: Restaurar paquetes y verificar build**

  ```bash
  dotnet restore PortalNegocioWS.sln
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`

- [ ] **Paso 7: Verificar UI en desarrollo**

  Iniciar la app y navegar a `http://localhost:<puerto>/scalar/v1`. Esperado: UI de Scalar mostrando todos los endpoints.

- [ ] **Paso 8: Commit**

  ```bash
  git add Directory.Packages.props \
          PortalNegocioWS/PortalNegocioWS.csproj \
          PortalNegocioWS/Installers/SwaggerInstaller.cs \
          PortalNegocioWS/Program.cs
  git commit -m "feat: reemplazar Swashbuckle por OpenAPI nativo .NET 9 con Scalar UI"
  ```

---

### Task 15: Agregar Health Check para Oracle

**Spec:** 3.4  
**Files:**
- Create: `PortalNegocioWS/HealthChecks/OracleHealthCheck.cs`
- Modify: `PortalNegocioWS/Program.cs`

- [ ] **Paso 1: Crear `OracleHealthCheck.cs`**

  ```csharp
  // PortalNegocioWS/HealthChecks/OracleHealthCheck.cs
  using Microsoft.Extensions.Diagnostics.HealthChecks;
  using Negocio.Data;
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  namespace PortalNegocioWS.HealthChecks
  {
      public class OracleHealthCheck : IHealthCheck
      {
          public Task<HealthCheckResult> CheckHealthAsync(
              HealthCheckContext context,
              CancellationToken cancellationToken = default)
          {
              try
              {
                  using var cx = new PORTALNEGOCIODataContext();
                  cx.Connection.Open();
                  cx.Connection.Close();
                  return Task.FromResult(HealthCheckResult.Healthy("Conexión Oracle OK"));
              }
              catch (Exception ex)
              {
                  return Task.FromResult(HealthCheckResult.Unhealthy("Error de conexión Oracle", ex));
              }
          }
      }
  }
  ```

- [ ] **Paso 2: Registrar y mapear en `Program.cs`**

  Después de los installers (antes de `var app = builder.Build()`), agregar:
  ```csharp
  builder.Services.AddHealthChecks()
      .AddCheck<OracleHealthCheck>("oracle");
  ```
  
  Agregar using:
  ```csharp
  using PortalNegocioWS.HealthChecks;
  ```

  Después de `var app = builder.Build()`, agregar antes de `app.Run()`:
  ```csharp
  app.MapHealthChecks("/health");
  ```

- [ ] **Paso 3: Verificar build**

  ```bash
  dotnet build PortalNegocioWS.sln
  ```

- [ ] **Paso 4: Verificar endpoint**

  Iniciar la app y hacer GET a `http://localhost:<puerto>/health`.  
  Esperado: HTTP 200 con body `Healthy`.  
  Si Oracle no es accesible: HTTP 503 con body `Unhealthy`.

- [ ] **Paso 5: Commit**

  ```bash
  git add PortalNegocioWS/HealthChecks/OracleHealthCheck.cs PortalNegocioWS/Program.cs
  git commit -m "feat: agregar health check endpoint /health con verificación de conexión Oracle"
  ```

---

### Task 16: Limpiar dependencias innecesarias

**Spec:** 3.5  
**Files:**
- Modify: `Directory.Packages.props`
- Modify: `Negocio.Business/Negocio.Business.csproj`

- [ ] **Paso 1: Verificar uso real de `EntityFramework 6.5.1` y `Oracle.EntityFrameworkCore`**

  ```bash
  grep -rn "using System.Data.Entity\b" Negocio.Business/ --include="*.cs"
  grep -rn "using Oracle.EntityFrameworkCore\|OracleDbContext\|DbContext" Negocio.Business/ --include="*.cs"
  ```

- [ ] **Paso 2: Si los resultados son solo imports no usados, remover de `Negocio.Business.csproj`**

  Eliminar de `Negocio.Business.csproj`:
  ```xml
  <PackageReference Include="EntityFramework" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" />
  ```
  
  > `using System.Data.Entity` aparece en `SolicitudCompra.cs` pero solo se usa `DbEntityEntry` para EF6. Si LinqConnect ya provee esa funcionalidad, el import puede ser eliminado. Verificar que no cause error de compilación.

- [ ] **Paso 3: Remover paquetes ASP.NET Core 2.2 de `Directory.Packages.props`**

  Eliminar las líneas:
  ```xml
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Abstractions" Version="2.2.0" />
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Core" Version="2.2.5" />
  ```

- [ ] **Paso 4: Remover `Oracle.EntityFrameworkCore` si no se usa**

  ```bash
  grep -rn "Oracle.EntityFrameworkCore" . --include="*.csproj" --include="*.cs"
  ```
  Si solo está en `Directory.Packages.props` y no en ningún `.csproj`, eliminar:
  ```xml
  <PackageVersion Include="Oracle.EntityFrameworkCore" Version="9.23.90" />
  ```

- [ ] **Paso 5: Restaurar y verificar build**

  ```bash
  dotnet restore PortalNegocioWS.sln
  dotnet build PortalNegocioWS.sln
  ```
  Esperado: `Build succeeded. 0 Error(s)`. Si hay errores de tipo no encontrado, el paquete sí se usa — restaurarlo y buscar alternativa.

- [ ] **Paso 6: Commit**

  ```bash
  git add Directory.Packages.props Negocio.Business/Negocio.Business.csproj
  git commit -m "chore: remover dependencias innecesarias (EF6, ASP.NET Core 2.2, Oracle EFCore sin uso)"
  ```

---

## Self-Review

### Cobertura del spec

| Spec | Task | Estado |
|------|------|--------|
| 1.1 SQL Injection | Task 1 | ✅ |
| 1.2 Secretos en appsettings | Task 2 | ✅ |
| 1.3 SendMail silencioso | Task 3 | ✅ |
| 1.4 JWT Issuer/Audience | Task 4 | ✅ |
| 1.5 new Thread() | Task 5 | ✅ |
| 1.6 Model validation | Task 6 | ✅ |
| 2.1 IDataContextFactory | Task 7 | ✅ |
| 2.2 Extraer IEmailService | Task 8 | ✅ |
| 2.3 Async ficticio | Task 9 | ✅ |
| 2.4 Código muerto | Task 10 | ✅ |
| 2.5 Respuestas controllers | Task 11 | ✅ |
| 3.1 OpenAPI nativo | Task 14 | ✅ |
| 3.2 Serilog activo | Task 12 | ✅ |
| 3.3 Compresión | Task 13 | ✅ |
| 3.4 Health checks | Task 15 | ✅ |
| 3.5 Dependencias | Task 16 | ✅ |

### Notas de consistencia

- `IEmailService` definido en Task 8 es usado en la misma Task 8 (SmtpEmailService). Los servicios que llaman `_utilidades.SendMail` se migran en el mismo task.
- `IDataContextFactory` (Task 7) se crea como infraestructura base. La migración de todos los `new PORTALNEGOCIODataContext()` existentes es intencionalemente gradual — cada PR de feature posterior puede adoptar el factory.
- El `ILogger<UtilidadesBusiness>` agregado en Task 3 es removido en Task 8 cuando `SendMail` se mueve a `SmtpEmailService`. `UtilidadesBusiness` pierde la necesidad del logger.
- Scalar versión `2.5.4` es compatible con `Microsoft.AspNetCore.OpenApi 9.x`.
