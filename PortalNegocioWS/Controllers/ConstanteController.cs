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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PortalNegocioWS.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class ConstanteController : ControllerBase
    {
        private readonly IConstante _constanteBusiness;
        private readonly IConfiguration _configuration;
        private readonly IUtilidades _utilidades;
        private readonly IMapper _mapper;
        private readonly ILogger<ConstanteController> _logger;

        public ConstanteController(IConstante constante, IConfiguration configuration, IUtilidades utilidades, IMapper mapper, ILogger<ConstanteController> logger)
        {
            _constanteBusiness = constante;
            _configuration = configuration;
            _utilidades = utilidades;
            _mapper = mapper;
            _logger = logger;
        }


        // GET: api/<ConstanteController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lst_constante = await _constanteBusiness.GetConstante();

            if (lst_constante == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<List<Constante>>(lst_constante));
        }

        // GET api/<ConstanteController>/5
        [HttpGet("{id}")]
        public IActionResult GetUsuario(int id)
        {
            var constante = _constanteBusiness.GetConstante(id);

            if (constante == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<POGECONSTANTE>(constante));
        }

        // POST api/<ConstanteController>
        [HttpPost]
        public async Task<IActionResult> InsertConstante([FromBody] Constante constante)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"PN - Error al insertar constante modelo no es valido: { JsonConvert.SerializeObject(constante) } ");
                return BadRequest(ModelState);
            }

            try
            {
                constante.LogsFecha = DateTime.Now;
                
                var newConsta = _mapper.Map<POGECONSTANTE>(constante);

                var resp = await _constanteBusiness.InsertConstante(newConsta);

                if (resp.Status == Configuracion.StatusOk)
                {
                    resp.Message = "Constante creada exitosamente!";
                }

                return Ok(resp);
            }
            catch (Exception e)
            {
                _logger.LogError($"PN - Error al insertar constante: { e.Message }");
                return BadRequest(e.Message);
            }
        }

        // PUT api/<ConstanteController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] Constante constante)
        {
            ResponseStatus r;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != constante.IdConstante)
            {
                return BadRequest();
            }

            try
            {
                var updConsta = _mapper.Map<POGECONSTANTE>(constante);
                await _constanteBusiness.UpdateConstante(id, updConsta);
                r = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch (KeyNotFoundException dx)
            {
                _logger.LogError($"PN - Error al actualizar la constante: { dx.Message }");
                return NotFound(dx);
            }
            catch (Exception e)
            {
                _logger.LogError($"PN - Error al actualizar la constante: { JsonConvert.SerializeObject(constante) } :: { e.Message }");
                return StatusCode(500, e.Message);
            }
        } 
    }
}
