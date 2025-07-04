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


var builder = WebApplication.CreateBuilder(args);

// Obtener todos los instaladores
var installers = Assembly.GetExecutingAssembly().ExportedTypes
    .Where(x => typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
    .Select(Activator.CreateInstance)
    .Cast<IInstaller>()
    .ToList();

// Registrar servicios a través de los instaladores
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

OracleMonitor myMonitor = new OracleMonitor();
myMonitor.IsActive = true;


var app = builder.Build();

// Configuración de middleware (equivalente a `Configure` en `Startup.cs`)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("OrigenLocal");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Mapea los controladores de la API

app.Run();

/*public class Program
{
    public static void Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        Log.Logger = new LoggerConfiguration()
            //.ReadFrom.Configuration(configuration)
            .MinimumLevel.Error()
            .WriteTo.File(@"Logs/log.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:t4}] {Message:j}{NewLine}")
            /*.WriteTo.Oracle(cfg => 
                            cfg.WithSettings(configuration.GetConnectionString("PORTALNEGOCIODataContextConnectionString"))
                            .UseBurstBatch()
                            .CreateSink())*/
/*      .CreateLogger();

  try
  {
      Log.Debug ("Inicia el WS PN");
      CreateHostBuilder(args).Build().Run();
  }

  catch (Exception ex)
  {
      Log.Fatal($"Error iniciando el WS PN: { ex.Message }");
  }
  finally
  {
      Log.CloseAndFlush();
  }
}

public static IHostBuilder CreateHostBuilder(string[] args) =>
  Host.CreateDefaultBuilder(args)
      .UseSerilog()
      .ConfigureWebHostDefaults(webBuilder =>
      {
          webBuilder.UseStartup<Startup>();
      });
}*/
