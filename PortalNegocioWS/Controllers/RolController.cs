using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Model;
using PortalNegocioWS.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Negocio.Data;

namespace SWNegocio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RolController : ApiControllerBase
    {
        private readonly IRol _rolBusiness;
        private readonly ILogger<RolController> _logger;

        public RolController(IRol rol, ILogger<RolController> logger)
        {
            _rolBusiness = rol;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalogo()
        {
            Response<List<Rol>> r = new Response<List<Rol>>();

            try
            {
                var lst_rol = await _rolBusiness.GetRol();
                r.Data = lst_rol.Adapt<List<Rol>>();
                r.Status = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al obtener catálogo de roles");
                throw;
            }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetRol(decimal id)
        {
            var lst_rol = _rolBusiness.GetRol(id);

            if (lst_rol == null)
            {
                return NotFound();
            }

            return Ok(lst_rol);
        }


        [HttpPut]
        public IActionResult UpdateRol(decimal id, Rol rol)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != rol.Id)
            {
                return BadRequest();
            }

            try
            {
                _rolBusiness.UpdateRol(id, rol);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Insertrol(Rol rol)
        {
            ResponseStatus resp = new ResponseStatus();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                resp = await _rolBusiness.InsertRol(rol.Adapt<POGEROL>());
            }
            catch
            {
                return BadRequest();
            }

            return Ok(resp);
        }

        [HttpDelete]
        public IActionResult DeleteRol(decimal id)
        {
            Rol rol = new Rol();
            try
            {
                rol = _rolBusiness.DeleteRol(id);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(rol);
        }


    }
}
