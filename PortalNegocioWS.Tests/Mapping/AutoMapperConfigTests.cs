using AutoMapper;
using PortalNegocioWS.Installers;
using Xunit;

namespace PortalNegocioWS.Tests.Mapping;

public class AutoMapperConfigTests
{
    [Fact]
    public void AutoMapper_AllProfiles_ConfigurationIsValid()
    {
        var config = new MapperConfiguration(c =>
        {
            c.AddMaps(typeof(AutoMapperInstaller).Assembly);
        });

        config.AssertConfigurationIsValid();
    }
}
