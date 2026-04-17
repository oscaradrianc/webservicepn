using System.Net;
using System.Net.Http.Json;
using Moq;
using Negocio.Business;
using Negocio.Model;
using PortalNegocioWS.Tests.Infrastructure;
using Xunit;

namespace PortalNegocioWS.Tests.Auth;

public class LoginFlowTests
{
    [Fact]
    public async Task ValidCredentials_Returns200()
    {
        var mockLogin = new Mock<ILogin>();
        mockLogin.Setup(x => x.Authenticate(It.IsAny<LoginRequest>()))
            .Returns(new Response<Usuario>
            {
                Data = new Usuario { ResultadoLogin = 1 }
            });

        using var factory = new CustomWebApplicationFactory().WithExtraServices(services =>
        {
            var desc = services.SingleOrDefault(d => d.ServiceType == typeof(ILogin));
            if (desc != null) services.Remove(desc);
            services.AddScoped(_ => mockLogin.Object);
        });

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/login/authenticate",
            new LoginRequest { Username = "test", Password = "test", Origen = "I" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidCredentials_Returns401()
    {
        var mockLogin = new Mock<ILogin>();
        mockLogin.Setup(x => x.Authenticate(It.IsAny<LoginRequest>()))
            .Returns(new Response<Usuario>
            {
                Data = new Usuario { ResultadoLogin = -1 }
            });

        using var factory = new CustomWebApplicationFactory().WithExtraServices(services =>
        {
            var desc = services.SingleOrDefault(d => d.ServiceType == typeof(ILogin));
            if (desc != null) services.Remove(desc);
            services.AddScoped(_ => mockLogin.Object);
        });

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/login/authenticate",
            new LoginRequest { Username = "test", Password = "wrong", Origen = "I" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
