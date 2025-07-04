using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWNegocio.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class OpcionController : ControllerBase
    {

        private readonly IOpcion _opcionBusiness;

        public OpcionController(IOpcion opcion)
        {
            _opcionBusiness = opcion;
        }

        [HttpGet]
        //[Route("Get")]
        public IActionResult GetOpcion()
        {
            var lst_opcion = _opcionBusiness.GetOpcion();

            if (lst_opcion == null)
            {
                return NotFound();
            }

            return Ok(lst_opcion);
        }

        [HttpGet]
        [Route("GetId")]
        public IActionResult GetOpcion(decimal id)
        {
            var lst_opcion = _opcionBusiness.GetOpcion(id);

            if (lst_opcion == null)
            {
                return NotFound();
            }

            return Ok(lst_opcion);
        }


        [HttpPut("{id}")]
        //[Route("Update")]
        public async Task<IActionResult> UpdateOpcion(decimal id, Opcion opcion)
        {
            ResponseStatus res = new ResponseStatus();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != opcion.IdOpcion)
            {
                return BadRequest();
            }

            try
            {
                res = await _opcionBusiness.UpdateOpcion(id, opcion);
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

        [HttpPost]
        //[Route("Insert")]
        public async Task<IActionResult> InsertOpcion(Opcion opcion)
        {
            ResponseStatus res = new ResponseStatus();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
               res = await _opcionBusiness.InsertOpcion(opcion);
            }
            catch
            {
                return BadRequest();
            }

            return Ok(res);
        }

        [HttpDelete]
       // [Route("Delete")]
        public IActionResult DeleteOpcion(decimal id)
        {
            Opcion catalogo = new Opcion();
            try
            {
                catalogo = _opcionBusiness.DeleteOpcion(id);
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


        /*private PortalNegociosEntities db = new PortalNegociosEntities();

        // GET: api/Opcion
        public Response<IQueryable<POGE_OPCION>> GetPOGE_OPCION()
        {            
            Response<IQueryable<POGE_OPCION>> resp = new Response<IQueryable<POGE_OPCION>>();

            resp.Data = db.POGE_OPCION; ;
            resp.Status = new ResponseStatus { Status = Configuracion.StatusOk };
            return resp;
        }

        // GET: api/Opcion/5
        [ResponseType(typeof(POGE_OPCION))]
        public async Task<IHttpActionResult> GetPOGE_OPCION(decimal id)
        {
            POGE_OPCION pOGE_OPCION = await db.POGE_OPCION.FindAsync(id);
            if (pOGE_OPCION == null)
            {
                return NotFound();
            }

            return Ok(pOGE_OPCION);
        }

        // PUT: api/Opcion/5
        [ResponseType(typeof(void))]
        public async Task<ResponseStatus> PutPOGE_OPCION(decimal id, POGE_OPCION pOGE_OPCION)
        {
            ResponseStatus resp = new ResponseStatus();
            OpcionBusiness opcionBusiness = new OpcionBusiness();

            if (!ModelState.IsValid)
            {
                resp.Status = "Error";
                return resp;
            }

            if (id != pOGE_OPCION.OPCI_OPCION)
            {
                resp.Status = "Error";
                return resp;
            }

            resp = await opcionBusiness.ActualizarOpcion(id, pOGE_OPCION);

            return resp;
        }

        // POST: api/Opcion
        [ResponseType(typeof(POGE_OPCION))]
        public async Task<ResponseStatus> PostPOGE_OPCION(POGE_OPCION pOGE_OPCION)
        {
            ResponseStatus resp = new ResponseStatus();
            OpcionBusiness usuarioBusiness = new OpcionBusiness();

            if (!ModelState.IsValid)
            {
                resp.Status = "ERROR";
                resp.Message = "Modelo Invalido";
                return resp; // -- BadRequest(ModelState);
            }


            resp = await usuarioBusiness.CrearOpcion(pOGE_OPCION);

            resp.Status = "OK";
            resp.Message = "";
            return resp;
        }

        // DELETE: api/Opcion/5
        [ResponseType(typeof(POGE_OPCION))]
        public async Task<IHttpActionResult> DeletePOGE_OPCION(decimal id)
        {
            POGE_OPCION pOGE_OPCION = await db.POGE_OPCION.FindAsync(id);
            if (pOGE_OPCION == null)
            {
                return NotFound();
            }

            db.POGE_OPCION.Remove(pOGE_OPCION);
            await db.SaveChangesAsync();

            return Ok(pOGE_OPCION);
        }*/

    }
}