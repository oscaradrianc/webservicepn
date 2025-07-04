
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;



namespace SWNegocio.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class CatalogoController : ControllerBase
    {

        private readonly ICatalogo _catalogoBusiness;
        private readonly IMapper _mapper;

        public CatalogoController(ICatalogo catalogo, IMapper mapper)
        {
            _catalogoBusiness = catalogo;
            _mapper = mapper;
        }
        
        [HttpGet]
        //[Route("Get")]
        public async Task<IActionResult> Get()
        {
            var lst_catalogo = await _catalogoBusiness.GetCatalogo();

            if (lst_catalogo == null)
            {
                return NotFound();
            }

            return Ok(lst_catalogo);
        }

        [HttpGet]
        [Route("getcatalogodescunimed")]
        public async Task<ActionResult> GetCatalogoConMedida()
        {
            var ltaCatalogo = await _catalogoBusiness.GetCatalogoConMedida();

            if (ltaCatalogo == null)
            {
                return NotFound();
            }

            return Ok(ltaCatalogo);
        }        


        [HttpGet]
        [Route("GetId")]
        public IActionResult GetCatalogo(decimal id)
        {
            var lst_catalogo = _catalogoBusiness.GetCatalogo(id);
            
            if (lst_catalogo == null)
            {
                return NotFound();
            }

            return Ok(lst_catalogo);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCatalogo(decimal id, [FromBody]Catalogo catalogo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != catalogo.CodigoInterno)
            {
                return BadRequest();
            }           

            try
            {                
                var resp = await _catalogoBusiness.UpdateCatalogo(id, _mapper.Map<PONECATALOGO>(catalogo));
                return Ok(resp);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }            
        }

        [HttpPost]
        //[Route("Insert")]
        public async Task<IActionResult> InsertCatalogo(Catalogo catalogo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resp = await _catalogoBusiness.InsertCatalogo(_mapper.Map<PONECATALOGO>(catalogo));
                return Ok(resp);
            }
            catch
            {
                return BadRequest();
            }          
        }

        [HttpDelete]
        [Route("Delete")]
        public IActionResult DeleteCatalogo(decimal id)
        {
            Catalogo catalogo = new Catalogo();
            try
            {
               catalogo = _catalogoBusiness.DeleteCatalogo(id);
            }
            catch (KeyNotFoundException dx)
            {
                return NotFound(dx);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(catalogo);
        }

    }
}