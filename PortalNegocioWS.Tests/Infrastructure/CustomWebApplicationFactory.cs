using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Negocio.Data;
using System.Text;

namespace PortalNegocioWS.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Deterministic test signing key — NEVER use in production appsettings
    public static readonly SymmetricSecurityKey TestSigningKey =
        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            "TestSecretKey_MustBe32CharactersOrLonger!"));

    private Action<IServiceCollection>? _extraServices;

    public CustomWebApplicationFactory WithExtraServices(Action<IServiceCollection> configure)
    {
        _extraServices = configure;
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove real Oracle singleton — prevents connection attempts at startup
            // BusinessInstaller registers: services.AddSingleton<IDataContextFactory, DataContextFactory>()
            var factoryDesc = services.SingleOrDefault(
                d => d.ServiceType == typeof(IDataContextFactory));
            if (factoryDesc != null) services.Remove(factoryDesc);
            var mockFactory = new Mock<IDataContextFactory>();
            services.AddSingleton(mockFactory.Object);

            // Override JWT to use test signing key.
            // PostConfigure runs AFTER AuthenticationInstaller.Configure — guaranteed last-wins.
            // DO NOT use Configure<JwtBearerOptions> — it would be overwritten by the installer.
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = TestSigningKey,
                        ValidateIssuer = true,
                        ValidIssuer = "PortalNegociosAPI",
                        ValidateAudience = true,
                        ValidAudience = "PortalNegociosApp",
                        ValidateLifetime = false
                    };
                });

            _extraServices?.Invoke(services);
        });
    }
}
