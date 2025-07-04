using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace PortalNegocioWS.Installers
{
    public class CorsInstaller : IInstaller
    {   
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "OrigenLocal",
                                  builder =>                                  
                                  {
                                      builder.WithOrigins(configuration.GetSection("Settings").GetSection("URLFrontend").Value, configuration.GetSection("Settings").GetSection("URLBackend").Value)
                                      .AllowAnyHeader()
                                      .AllowAnyMethod();
                                  });
            });

            services.AddControllers();
        }
    }
}
