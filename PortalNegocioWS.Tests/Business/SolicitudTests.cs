using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Moq;
using Negocio.Business;
using Negocio.Model;
using PortalNegocioWS.Tests.Infrastructure;
using Xunit;

namespace PortalNegocioWS.Tests.Business;

public class SolicitudTests
{
    [Fact]
    public async Task AutorizarSolicitud_WhenBusinessReturnsOK_Returns200()
    {
        var mockSolicitud = new Mock<ISolicitudCompra>();
        mockSolicitud.Setup(s => s.AutorizarSolicitud(It.IsAny<Autorizacion>())).Returns("OK");

        using var factory = new CustomWebApplicationFactory().WithExtraServices(services =>
        {
            var desc = services.SingleOrDefault(d => d.ServiceType == typeof(ISolicitudCompra));
            if (desc != null) services.Remove(desc);
            services.AddScoped(_ => mockSolicitud.Object);
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken());

        var response = await client.PostAsJsonAsync("/api/Solicitud/Autorizar", new Autorizacion
        {
            CodigoSolicitud = 1,
            EstadoAutorizacion = "A",
            IdUsuario = 1,
            TipoAutorizacion = "G"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarSolicitud_WhenBusinessReturnsOK_Returns200()
    {
        var mockSolicitud = new Mock<ISolicitudCompra>();
        mockSolicitud.Setup(s => s.RegistrarSolicitud(It.IsAny<SolicitudCompra>())).ReturnsAsync("OK");

        using var factory = new CustomWebApplicationFactory().WithExtraServices(services =>
        {
            var desc = services.SingleOrDefault(d => d.ServiceType == typeof(ISolicitudCompra));
            if (desc != null) services.Remove(desc);
            services.AddScoped(_ => mockSolicitud.Object);
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTokenHelper.GenerateToken());

        var response = await client.PostAsJsonAsync("/api/Solicitud/registrar", new SolicitudCompra
        {
            Descripcion = "Test",
            TipoSolicitud = "1"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
