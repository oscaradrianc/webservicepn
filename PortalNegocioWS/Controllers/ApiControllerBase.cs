using Microsoft.AspNetCore.Mvc;

namespace PortalNegocioWS.Controllers
{
    /// <summary>
    /// Base class for all API controllers (implements API-03).
    /// Inheriting controllers get [ApiController] (auto ModelState validation) automatically.
    ///
    /// INTENTIONAL DESIGN (per locked decisions D-02 and D-03, override documented in 04-CONTEXT.md):
    /// This class does NOT include helper methods such as BusinessError(), NotFound(), or
    /// Unauthorized(). Per D-02, error handling is centralized in GlobalExceptionHandler.
    /// Per D-03, controllers signal errors by throwing typed exceptions directly:
    ///   - throw new BusinessException(message)   → HTTP 400 ProblemDetails
    ///   - throw new NotFoundException(message)   → HTTP 404 ProblemDetails
    ///   - throw new UnauthorizedException(...)   → HTTP 401 ProblemDetails
    /// Wrapper helper methods would be an intermediate layer with no benefit over direct throws.
    /// This marker class IS the complete implementation of API-03.
    /// </summary>
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
    }
}
