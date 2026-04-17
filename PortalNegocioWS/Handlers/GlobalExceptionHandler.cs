using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PortalNegocioWS.Exceptions;

namespace PortalNegocioWS.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

            var (statusCode, title, detail) = exception switch
            {
                BusinessException ex => (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),
                NotFoundException ex => (StatusCodes.Status404NotFound, "Not Found", ex.Message),
                UnauthorizedException ex => (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
            };

            httpContext.Response.StatusCode = statusCode;

            await _problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail
                }
            });

            return true;
        }
    }
}
