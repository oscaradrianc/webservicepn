using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PortalNegocioWS.Installers
{
    public class MapsterInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            // Scan IRegister classes in this assembly into GlobalSettings so that
            // .Adapt<T>() extension methods (which use GlobalSettings by default) pick up
            // all custom property mappings (ALLCAPS -> PascalCase).
            // Per locked decision D-05: use GlobalSettings directly, NOT a new instance.
            TypeAdapterConfig.GlobalSettings.Scan(typeof(MapsterInstaller).Assembly);

            // Register the GlobalSettings singleton so Program.cs can resolve it for Compile().
            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        }
    }
}
