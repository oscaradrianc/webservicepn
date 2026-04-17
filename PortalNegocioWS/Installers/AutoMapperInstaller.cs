using Microsoft.Extensions.DependencyInjection;


namespace PortalNegocioWS.Installers
{
    public class AutoMapperInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var config = new AutoMapper.MapperConfiguration(c =>
            {
                c.AddMaps(typeof(AutoMapperInstaller).Assembly);
            });

            config.AssertConfigurationIsValid();

            var mapper = config.CreateMapper();

            services.AddSingleton(mapper);
        }
    }
}
