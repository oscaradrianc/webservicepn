using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Negocio.Business;
using Negocio.Model;
using System.Net;
using System.Threading;

namespace SWNegocio.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogin _loginBusiness;

        public LoginController(IConfiguration configuration, ILogin login)
        {
            _configuration = configuration;
            _loginBusiness = login;
        }


        [HttpPost]
        [EnableCors]
        [Route("authenticate")]
        public Response<Usuario> Authenticate(LoginRequest login)
        {
            Response<Usuario> resp = new Response<Usuario>();


            if (login == null)
            {
                resp.Status = new ResponseStatus { Status = Configuracion.StatusError, Message = HttpStatusCode.BadRequest.ToString() };
                resp.Data = null;
                
            }
            else
            {

                resp = _loginBusiness.Authenticate(login, _configuration);
            }

            return resp;

        }



        [HttpPost]
        [EnableCors]
        [Route("changepassword")]
        public ResponseStatus ChangePassword(ChangePasswordRequest credentials)
        {
            ResponseStatus resp = new ResponseStatus();

            if (credentials == null)
            {
                resp = new ResponseStatus { Status = Configuracion.StatusError, Message = HttpStatusCode.BadRequest.ToString() };                
            }
            else
            {

                resp = _loginBusiness.ChangePassword (credentials, _configuration);
            }

            return resp;

        }

        [HttpPost]
        [EnableCors]
        [Route("resetpassword")]
        public ResponseStatus ResetPassword(ResetPassRequest request)
        {
            ResponseStatus resp = new ResponseStatus();

            if (request == null)
            {
                resp = new ResponseStatus { Status = Configuracion.StatusError, Message = HttpStatusCode.BadRequest.ToString() };
            }
            else
            {

                resp = _loginBusiness.ResetPassword (request, _configuration);
            }

            return resp;

        }

    }
}