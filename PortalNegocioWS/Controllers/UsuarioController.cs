using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Data;
using Negocio.Model;
using Newtonsoft.Json;

namespace SWNegocio.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {

        private readonly IUsuario _usuarioBusiness;
        private readonly ILogin _loginBusiness;
        private readonly IConfiguration _configuration;
        private readonly IUtilidades _utilidades;
        private readonly IMapper _mapper;
        private readonly ILogger<UsuarioController> _logger;
        private readonly INotificacion _notificacion;
        public UsuarioController(IUsuario usuario, ILogin login, IConfiguration configuration, IUtilidades utilidades, IMapper mapper, ILogger<UsuarioController> logger, INotificacion notificacion)
        {
            _usuarioBusiness = usuario;
            _loginBusiness = login;
            _configuration = configuration;
            _utilidades = utilidades;
            _mapper = mapper;
            _logger = logger;
            _notificacion = notificacion;
        }

        [HttpGet]
        //[Route("Get")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Get()
        {
            var lst_usuarios = await _usuarioBusiness.GetUsuario();

            if (lst_usuarios == null)
            {
                return NotFound();
            }

            return Ok(lst_usuarios);
        }

        [HttpGet]
        [Route("GetId")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUsuario(decimal id)
        {
            var lst_usuarios = _usuarioBusiness.GetUsuario(id);

            if (lst_usuarios == null)
            {
                return NotFound();
            }

            return Ok(lst_usuarios);
        }


        [HttpPut("{id}")]
        //[Route("Update")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUsuario(decimal id, [FromBody] Usuario usuario)
        {
            ResponseStatus r;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != usuario.IdUsuario)
            {
                return BadRequest();
            }

            try
            {
                var usua = _mapper.Map<POGEUSUARIO>(usuario);
                await _usuarioBusiness.UpdateUsuario(id, usua);
                r = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch (KeyNotFoundException dx)
            {
                _logger.LogError($"PN - Error al actualizar el usuario: { dx.Message }");
                return NotFound(dx);
            }
            catch(Exception e)
            {
                _logger.LogError($"PN - Error al actualizar el usuario: { JsonConvert.SerializeObject(usuario) } :: { e.Message }");
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        /*[Route("Insert")]*/
        public async Task<IActionResult> InsertUsuario([FromBody]Usuario usuario)
        {            
            if (!ModelState.IsValid)
            {
                _logger.LogError($"PN - Error al insertar usuario modelo no es valido: { JsonConvert.SerializeObject(usuario) } ");
                return BadRequest(ModelState);
            }

            try
            {
                string claveAleatoria = _utilidades.GetRandomKey();
                string claveEncriptada = _utilidades.Encriptar(claveAleatoria, _configuration.GetSection("EncryptedKey").Value);

                usuario.Clave = claveEncriptada;
                usuario.CambiarClave = Configuracion.ValorSI;
                usuario.VenceClave = Configuracion.ValorSI;
                usuario.FechaVence = DateTime.Now.AddDays(10);

                var usua = _mapper.Map<POGEUSUARIO>(usuario);

                var resp = await _usuarioBusiness.InsertUsuario(usua);

                if (resp.Status == Configuracion.StatusOk)
                {
                    resp.Message = "Usuario creado exitosamente!";

                    //Asigno la clave aleatoria sin encriptar para enviarla por correo al usuario
                    usuario.Clave = claveAleatoria;

                    //////////////////Envia Correo indicando nuevo registro de usuario//////////////////////////
                    Thread t = new Thread(() =>
                    {
                        _notificacion.GenerarNotificacion("nuevousuario", usuario);
                    });

                    t.Start();
                    t.IsBackground = true;
                }

                return Ok(resp);
            }
            catch (Exception e)
            {
                _logger.LogError($"PN - Error al insertar usuario: { e.Message }");
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
       //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteUsuario(decimal id)
        {
            Usuario usuario = new Usuario();
            try
            {
                usuario = _usuarioBusiness.DeleteUsuario(id);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(usuario);
        }

                
        [Route("cambiarclave")]
        [HttpPost]
        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ResponseStatus CambiarClaveUsuario(CambioClave request)
        {
            /*
            ResponseStatus resp = new ResponseStatus();

            if (!ModelState.IsValid)
            {
                resp.Status = "ERROR";
                resp.Message = "Modelo Invalido";
                return resp; // -- BadRequest(ModelState);
            }

            resp = _usuarioBusiness.CambiarClaveUsuario(request);

            resp.Status = "OK";
            resp.Message = "";
            return resp;*/

            ResponseStatus resp; // = new ResponseStatus();

            if (request == null)
            {
                resp = new ResponseStatus { Status = Configuracion.StatusError, Message = HttpStatusCode.BadRequest.ToString() };
            }
            else
            {
                ChangePasswordRequest newRequest = new ChangePasswordRequest { Username = request.Usuario, Password = request.ClaveAnterior, NewPassword = request.NuevaClave };
                resp = _loginBusiness.ChangePassword(newRequest, _configuration);
            }

            return resp;
        }


        [Route("resetclave")]
        [HttpPost]
        public async Task<IActionResult> ResetClave([FromBody]int idUsuario)
        {
            ResponseStatus resp = new ResponseStatus();

            try
            {
                int diasVenceClave = Convert.ToInt32(_configuration.GetSection("Settings").GetSection("DiasVenceClave").Value);

                resp = await _usuarioBusiness.ResetClave(idUsuario, diasVenceClave);

                if (resp.Status == Configuracion.StatusOk)
                {


                    Usuario usu = _usuarioBusiness.GetUsuario(idUsuario);
                    usu.Clave = resp.Message;
                    //////////////////Envia Correo indicando nuevo registro de usuario//////////////////////////
                    Thread t = new Thread(() =>
                    {
                        _notificacion.GenerarNotificacion("resetpassword", usu);
                    });

                    t.Start();
                    t.IsBackground = true;

                    resp.Message = string.Empty;
                }

                return Ok(resp);
            }
            catch (Exception e)
            {
                _logger.LogError($"PN - Error al insertar usuario: { e.Message }");
                return BadRequest(e.Message);
            }

        }
    }
}