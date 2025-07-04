using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devart.Data.Oracle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using PortalNegocioWS.Installers;
using Serilog;
using PortalNegocioWS.Services;

namespace PortalNegocioWS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           
            var installers =  typeof(Startup).Assembly.ExportedTypes.Where(x => 
                typeof(IInstaller).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            installers.ForEach(installer => installer.InstallServices(services, Configuration));
            //services.AddControllers().AddNewtonsoftJson();

            services.AddControllers().AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

          
            services.AddScoped<IScopedService, ScopedService>();

            services.AddCronJob<ActualizarEstadoSolicitudJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"0 0 * * *";
            });

            services.AddCronJob<EnviarNotificacionInvitacionJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = Configuration.GetSection("Settings").GetSection("CronEnviarInvitacion").Value;
            });

            /*
            services.AddCronJob<NotificacionActualizacionDatosJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                //c.CronExpression = Configuration.GetSection("Settings").GetSection("CronEnviarActualizacionDatos").Value;
                c.CronExpression = @"0 0 1 4 *";
            });*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseRouting();
            
            //Swagger
            app.UseSwagger();
            app.UseSwaggerUI(config => {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            app.UseCors("OrigenLocal");            
            //Authentication
            app.UseAuthorization();
            app.UseAuthentication();
           
            //CompressResponse
            app.UseResponseCompression();
            //app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
