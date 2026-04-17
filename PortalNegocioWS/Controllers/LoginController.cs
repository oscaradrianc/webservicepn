using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Negocio.Business;
using Negocio.Model;
using PortalNegocioWS.Controllers;
using PortalNegocioWS.Exceptions;

namespace SWNegocio.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : ApiControllerBase
    {
        private readonly ILogin _loginBusiness;

        public LoginController(ILogin login)
        {
            _loginBusiness = login;
        }

        [HttpPost]
        [EnableCors]
        [Route("authenticate")]
        public IActionResult Authenticate(LoginRequest login)
        {
            var resp = _loginBusiness.Authenticate(login);

            if (resp.Data?.ResultadoLogin == -2 || resp.Data?.ResultadoLogin == -1)
                return Unauthorized();

            return Ok(resp);
        }

        [HttpPost]
        [EnableCors]
        [Route("changepassword")]
        public IActionResult ChangePassword(ChangePasswordRequest credentials)
        {
            var resp = _loginBusiness.ChangePassword(credentials);

            if (resp.Status == Configuracion.StatusError)
                throw new BusinessException(resp.Message);

            return Ok(resp);
        }

        [HttpPost]
        [EnableCors]
        [Route("resetpassword")]
        public IActionResult ResetPassword(ResetPassRequest request)
        {
            var resp = _loginBusiness.ResetPassword(request);

            if (resp.Status == Configuracion.StatusError)
                throw new BusinessException(resp.Message);

            return Ok(resp);
        }
    }
}