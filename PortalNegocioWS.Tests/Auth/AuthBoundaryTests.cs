using System.Net;
using System.Net.Http.Headers;
using PortalNegocioWS.Tests.Infrastructure;
using Xunit;

namespace PortalNegocioWS.Tests.Auth;

public class AuthBoundaryTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthBoundaryTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Unauthorized_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/solicitud/list?tipo=1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Authorized_WithToken_DoesNotReturn401()
    {
        var client = _factory.CreateClient();
        var token = JwtTokenHelper.GenerateToken();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/solicitud/list?tipo=1");
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
