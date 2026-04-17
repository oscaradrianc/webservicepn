using System;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using PortalNegocioWS.Services;
using PortalNegocioWS.Installers;
using System.Reflection;
using System.Linq;
using Devart.Data.Oracle;
using Negocio.Business.Utilidades;
using System.Net;
using Negocio.Business;


// Bootstrap logger: captura errores durante el startup del host (Oracle conn fail, config load fail)
// Use non-reloadable logger in test runs to avoid "logger already frozen" across multiple factory instances.
bool isTest = AppDomain.CurrentDomain.GetAssemblies()
    .Any(a => a.FullName?.StartsWith("Microsoft.AspNetCore.Mvc.Testing") == true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Obtener todos los instaladores
var installers = Assembly.GetExecutingAssembly().ExportedTypes
    .Where(x => typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
    .Select(Activator.CreateInstance)
    .Cast<IInstaller>()
    .ToList();

// Registrar servicios a trav�s de los instaladores
installers.ForEach(installer => installer.InstallServices(builder.Services, builder.Configuration));

/////
builder.Services.AddSingleton<IStorageService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var storageType = config["Storage:Type"];

    return storageType switch
    {
        "Local" => new LocalStorageService(config["Storage:Local:BasePath"]),
        /*"S3" => new S3StorageService(
                    new AmazonS3Client(
                        config["Storage:S3:AccessKey"],
                        config["Storage:S3:SecretKey"],
                        Amazon.RegionEndpoint.GetBySystemName(config["Storage:S3:Region"])),
                    config["Storage:S3:BucketName"]),
        "Remote" => new RemoteStorageService(
                        config["Storage:Remote:ServerBasePath"],
                        new NetworkCredential(config["Storage:Remote:Username"], config["Storage:Remote:Password"])
        ),*/
        _ => throw new Exception("Tipo de almacenamiento no configurado.")
    };
});

/////


builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
}); ;

builder.Services.AddScoped<IScopedService, ScopedService>();

builder.Services.AddCronJob<ActualizarEstadoSolicitudJob>(c =>
{
    c.TimeZoneInfo = TimeZoneInfo.Local;
    c.CronExpression = @"0 0 * * *";
});

builder.Services.AddCronJob<EnviarNotificacionInvitacionJob>(c =>
{
    c.TimeZoneInfo = TimeZoneInfo.Local;
    c.CronExpression = builder.Configuration.GetSection("Settings").GetSection("CronEnviarInvitacion").Value;
});

builder.Services.AddCronJob<NotificacionActualizacionDatosJob>(c =>
{
    c.TimeZoneInfo = TimeZoneInfo.Local;
    c.CronExpression = builder.Configuration.GetSection("Settings").GetSection("CronEnviarActualizacionDatos").Value;
});

// Replace bootstrap logger with config-driven logger from appsettings.json
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    OracleMonitor myMonitor = new OracleMonitor();
    myMonitor.IsActive = true;
}

app.UseSerilogRequestLogging(); // Registra m�todo, ruta, status code y duraci�n de cada request HTTP
app.UseExceptionHandler();

// Configuraci�n de middleware (equivalente a `Configure` en `Startup.cs`)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("OrigenLocal");

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); // Mapea los controladores de la API

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory<Program> visibility from PortalNegocioWS.Tests
public partial class Program { }
