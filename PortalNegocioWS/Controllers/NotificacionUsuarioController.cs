using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PortalNegocioWS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    
    public class NotificacionUsuarioController : ControllerBase
    {
        private readonly INotificacionUsuario _notificacionUsuarioBusiness;

        public NotificacionUsuarioController(INotificacionUsuario notificacionUsuario)
        {
            _notificacionUsuarioBusiness = notificacionUsuario;
        }

        // GET: api/<NotificacionUsuarioController>
        [HttpGet]
        public IActionResult GetAll()
        {
            var lst_notiusuario = _notificacionUsuarioBusiness.ObtenerNotificacionesxUsuario();

            if (lst_notiusuario == null)
            {
                return NotFound();
            }

            return Ok(lst_notiusuario);
        }

        // GET api/<NotificacionUsuarioController>/5
        [HttpGet("{id}")]
        public IActionResult GetUsuarioxIdNotificacion(int id)
        {
            var lst_notiusuario = _notificacionUsuarioBusiness.ObtenerUsuarioxIdNotificacion(id);

            if (lst_notiusuario == null)
            {
                return NotFound();
            }

            return Ok(lst_notiusuario);
        }

        // POST api/<NotificacionUsuarioController>
        [HttpPost]
        //[Route("Insert")]
        public IActionResult InsertarNotificacionUsuario(Negocio.Model.NotificacionUsuario notificacionUsuario)
        {
            ResponseStatus response = new ResponseStatus();

            if (!ModelState.IsValid)
            {
                response.Status = Configuracion.StatusError;
                response.Message = Configuracion.MsjModeloInvalido;
                return Ok(response);
            }

            try
            {
                response = _notificacionUsuarioBusiness.InsertNotificacionUsuario(notificacionUsuario);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(response);
        }

        // PUT api/<NotificacionUsuarioController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<NotificacionUsuarioController>/5
        
        [HttpDelete("{id1:int}/{id2:int}")]
        //[Route("Delete")]
        public IActionResult DeleteNotificacionUsuario(int id1, int id2)
        {
            
            ResponseStatus response = new ResponseStatus();

            if (!ModelState.IsValid)
            {
                response.Status = Configuracion.StatusError;
                response.Message = Configuracion.MsjModeloInvalido;
                return Ok(response);
            }

            try
            {
                response = _notificacionUsuarioBusiness.EliminarNotificacionUsuario(id1, id2);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(response);
        }
    }
}
