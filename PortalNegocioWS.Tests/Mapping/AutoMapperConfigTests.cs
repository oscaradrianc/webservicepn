using Mapster;
using PortalNegocioWS.Installers;
using Xunit;

namespace PortalNegocioWS.Tests.Mapping;

public class MapsterConfigTests
{
    [Fact]
    public void MapsterConfig_Compile_DoesNotThrow()
    {
        // Arrange: build a fresh TypeAdapterConfig and scan all IRegister classes
        // from the PortalNegocioWS assembly — mirrors what MapsterInstaller does at startup.
        var config = new TypeAdapterConfig();
        config.Scan(typeof(MapsterInstaller).Assembly);

        // Act & Assert: Compile() validates all registered mappings.
        // If a property mapping is missing or misconfigured, this throws — catching
        // the same errors that IsDevelopment() guards catch at runtime in Development.
        var exception = Record.Exception(() => config.Compile());
        Assert.Null(exception);
    }
}
