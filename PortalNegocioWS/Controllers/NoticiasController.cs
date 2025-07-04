
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Server.HttpSys;
using Negocio.Business;
using Negocio.Model;
using RedisManager.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Threading.Tasks;

namespace SWNegocio.Controllers
{
    /// <summary>
    /// Controlador para manejo de noticias
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class NoticiasController : ControllerBase
    {

        private readonly INoticias _noticiasBusiness;

        public NoticiasController(INoticias noticias)
        {
            _noticiasBusiness = noticias;
        }

        /// <summary>
        /// Funcion para retornar las noticias
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors]
        [Route("list")]
        //[Cached(600)]
        public async Task<IActionResult> ListNoticias()
        {
            Response<List<Noticias>> r = new Response<List<Noticias>>();
            r.Data = await _noticiasBusiness.ListNoticias();
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return Ok(r);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerNoticiaPorId(int id)
        {
            Response<Noticias> r = new Response<Noticias>();
            r.Data = await _noticiasBusiness.ConsultarNoticiaPorId(id);
            r.Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = string.Empty };

            return Ok(r);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarNoticia(decimal id, [FromBody] Noticias noticia)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != noticia.CodigoNoticia)
            {
                return BadRequest();
            }

            try
            {
                var resp = await _noticiasBusiness.ActualizarNoticia(id, noticia);
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


    }
}