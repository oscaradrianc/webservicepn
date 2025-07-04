using Microsoft.AspNetCore.Mvc;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Negocio.Business;
using AutoMapper;
using Negocio.Data;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PortalNegocioWS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ParametroGeneralController : ControllerBase
    {
        private readonly IParametroGeneral _parametroGeneral;
        private readonly IMapper _mapper;
        private readonly ILogger<ParametroGeneralController> _logger;

        public ParametroGeneralController(IParametroGeneral parametroGeneral, IMapper mapper, ILogger<ParametroGeneralController> logger)
        {
            _parametroGeneral = parametroGeneral;
            _mapper = mapper;
            _logger = logger;
            
        }

        // GET: api/<ParametroGeneralController>
        [HttpGet]
        public async Task<IActionResult> GetClases()
        {
            Response<List<Clases>> r = new Response<List<Clases>>();

            try
            {
                var clases = await _parametroGeneral.ObtenerClases();
                r.Data = _mapper.Map<List<Clases>>(clases);
                r.Status = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch(Exception e)
            {
               
                return StatusCode(500, e.Message);
            }            
        }

        // GET api/<ParametroGeneralController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClaseValor(int id)
        {
            Response<List<ClaseValor>> r = new Response<List<ClaseValor>>();

            try
            {
                var claseValor = await _parametroGeneral.ObtenerClaseValorPorClase(id);
                r.Data = _mapper.Map<List<ClaseValor>>(claseValor);
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
        public async Task<IActionResult> AgregarClaseValor([FromBody]ClaseValor claseValor)
        {
            try
            {
                var resp = await _parametroGeneral.InsertarClaseValor(_mapper.Map<POGECLASEVALOR>(claseValor));                 
                    
                return Ok(resp);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error al insertar clase valor: { JsonConvert.SerializeObject(claseValor) } :: { e.Message } ");
                return StatusCode(500, e.Message);
            }
        }

        // PUT api/<ParametroGeneralController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarClaveValor(int id, [FromBody]ClaseValor claseValor)
        {
            try
            {
                if(claseValor == null)
                {
                    _logger.LogError("Objecto Clase Valor esta vacio");
                    return BadRequest("Objecto Clase Valor esta vacio");
                }

                if(!ModelState.IsValid)
                {
                    _logger.LogError ("El objecto Clase Valor no es valido");
                    return BadRequest("El objecto Clase Valor no es valido");
                }

                await _parametroGeneral.ActualizarClaseValor(id, _mapper.Map<POGECLASEVALOR>(claseValor));
                var resp = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(resp);
            }
            catch(Exception e)
            {
                _logger.LogError($"Error al actualizar clase valor { JsonConvert.SerializeObject(claseValor) } :: { e.Message }");
                return StatusCode(500, e.Message);
            }
        }

        // DELETE api/<ParametroGeneralController>/5
        /*[HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
