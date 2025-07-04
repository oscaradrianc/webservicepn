using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Model;
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
    public class RolController : ControllerBase
    {
        private readonly IRol _rolBusiness;
        private readonly IMapper _mapper;
        private readonly ILogger<RolController> _logger;

        public RolController(IRol rol, IMapper mapper, ILogger<RolController> logger)
        {
            _rolBusiness = rol;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        //[Route("Get")]
        public async Task<IActionResult> GetCatalogo()
        {
            Response<List<Rol>> r = new Response<List<Rol>>();

            try
            {
                var lst_rol = await _rolBusiness.GetRol();
                r.Data = _mapper.Map<List<Rol>>(lst_rol);
                r.Status = new ResponseStatus { Status = Configuracion.StatusOk };
                return Ok(r);
            }
            catch (Exception e)
            {

                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{id:int}")]
        //[Route("GetId")]
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
        //[Route("Update")]
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
                resp = await _rolBusiness.InsertRol(_mapper.Map<POGEROL>(rol));
            }
            catch
            {
                return BadRequest();
            }

            return Ok(resp);
        }

        [HttpDelete]
        //[Route("Delete")]
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