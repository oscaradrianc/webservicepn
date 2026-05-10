using Mapster;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Negocio.Business;
using Negocio.Model;
using Negocio.Data;

namespace PortalNegocioWS.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OpcionesRolController : ApiControllerBase
    {
        private readonly IRol _rol;
        private readonly ILogger<ParametroGeneralController> _logger;

        public OpcionesRolController(IRol rol, ILogger<ParametroGeneralController> logger)
        {
            _rol = rol;
            _logger = logger;
        }

        // GET: api/<OpcionesRolController>
        [HttpGet]
        public async Task<IActionResult> ObtenerOpcionesRol()
        {
            Response<List<OpcionxRol>> r = new Response<List<OpcionxRol>>();

            try
            {
                var opcionRol = await _rol.GetOpcionRol();
                r.Data = opcionRol.Adapt<List<OpcionxRol>>();
                r.Status = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        // GET api/<OpcionesRolController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerOpcionRol(int id)
        {
            Response<List<OpcionxRol>> r = new Response<List<OpcionxRol>>();

            try
            {
                var opcionRol = await _rol.GetOpcionRol(id);
                r.Data = opcionRol.Adapt<List<OpcionxRol>>();
                r.Status = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        // POST api/<OpcionesRolController>
        [HttpPost]
        public async Task<IActionResult> InsertarOpcionXRol([FromBody] OpcionxRol opcionRol)
        {
            try
            {
                ResponseStatus res = await _rol.InsertOpcionRol(opcionRol.Adapt<POGEOPCIONXROL>());

                return Ok(res);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        // PUT api/<OpcionesRolController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OpcionesRolController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarOpcionXRol(int id)
        {
            try
            {
                ResponseStatus res = await _rol.DeleteOpcionRol(id);
                return Ok(res);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
