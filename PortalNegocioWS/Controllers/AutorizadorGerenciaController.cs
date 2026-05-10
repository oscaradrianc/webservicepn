using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Data;
using Negocio.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PortalNegocioWS.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AutorizadorGerenciaController : ApiControllerBase
    {
        private readonly IAutorizadorGerencia _autorizadorGerencia;
        private readonly ILogger<ParametroGeneralController> _logger;

        public AutorizadorGerenciaController(IAutorizadorGerencia autorizadorGerencia, ILogger<ParametroGeneralController> logger)
        {
            _autorizadorGerencia = autorizadorGerencia;
            _logger = logger;
        }

        // GET: api/<AutorizadorGerenciaController>
        [HttpGet]
        public async Task<IActionResult> GetAutorizadores()
        {
            Response<List<AutorizadorGerencia>> r = new Response<List<AutorizadorGerencia>>();

            try
            {
                var autorizadoresGerencia = await _autorizadorGerencia.ObtenerAutorizadores();
                r.Data = autorizadoresGerencia.Adapt<List<AutorizadorGerencia>>();
                r.Status = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch (Exception e)
            {

                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAutorizadores(int id)
        {
            Response<List<AutorizadorGerencia>> r = new Response<List<AutorizadorGerencia>>();

            try
            {
                var claseValor = await _autorizadorGerencia.ObtenerAutorizadores(id);
                r.Data = claseValor.Adapt<List<AutorizadorGerencia>>();
                r.Status = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        // POST api/<ParametroGeneralController>
        [HttpPost]
        public async Task<IActionResult> AgregarClaseValor([FromBody]AutorizadorGerencia autorizador)
        {
            try
            {
                var resp = await _autorizadorGerencia.InsertarAutorizadorGerencia(autorizador.Adapt<POGEAUTORIZADORGERENCIA>());

                return Ok(resp);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error al insertar clase valor: { JsonConvert.SerializeObject(autorizador) } :: { e.Message } ");
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("{id1:int}/{id2:int}")]
        public async Task<IActionResult> DeleteAutorizacion(int id1, int id2)
        {
            ResponseStatus res = new ResponseStatus();
            try
            {
                res = await _autorizadorGerencia.EliminarAutorizadorGerencia(id1, id2);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(res);
        }
    }
}
