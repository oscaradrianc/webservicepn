using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortalNegocioWS.Handlers;

namespace PortalNegocioWS.Installers
{
    public class ErrorHandlingInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = ctx =>
                {
                    ctx.ProblemDetails.Extensions["traceId"] =
                        ctx.HttpContext.TraceIdentifier;
                };
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ctx =>
                    new UnprocessableEntityObjectResult(
                        new ValidationProblemDetails(ctx.ModelState));
            });
        }
    }
}
