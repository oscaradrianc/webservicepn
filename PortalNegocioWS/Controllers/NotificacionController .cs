
using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Model;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace SWNegocio.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class NotificacionController : ControllerBase
    {

        private readonly INotificacion _notificacionBusiness;

        public NotificacionController(INotificacion notificacion)
        {
            _notificacionBusiness = notificacion;
        }

        [HttpGet]
        //[Route("Get")]
        public IActionResult Get()
        {
            var lst_notificacion = _notificacionBusiness.GetNotificacion();

            if (lst_notificacion == null)
            {
                return NotFound();
            }

            return Ok(lst_notificacion);
        }

        [HttpGet]
        [Route("GetId")]
        public IActionResult GetNotificacion(decimal id)
        {
            var lst_notificacion = _notificacionBusiness.GetNotificacion(id);

            if (lst_notificacion == null)
            {
                return NotFound();
            }

            return Ok(lst_notificacion);
        }


        [HttpPut("{id}")]
        //[Route("Update")]
        public async Task<IActionResult> UpdateNotificacion(decimal id, Notificacion notificacion)
        {
            ResponseStatus resp = new ResponseStatus();
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != notificacion.IdNotificacion)
            {
                return BadRequest();
            }

            try
            {
                resp = await _notificacionBusiness.UpdateNotificacion(id, notificacion);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(resp);
        }

        [HttpPost]
        [Route("Insert")]
        public IActionResult InsertNotificacion(Notificacion notificacion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _notificacionBusiness.InsertNotificacion(notificacion);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("Delete")]
        public IActionResult DeleteNotificacion(decimal id)
        {
            Notificacion notificacion = new Notificacion();
            try
            {
                notificacion = _notificacionBusiness.DeleteNotificacion(id);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(notificacion);
        }

    }
}