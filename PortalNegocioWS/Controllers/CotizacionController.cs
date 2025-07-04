using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SWNegocio.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionController : ControllerBase
    {

        
        private readonly ICotizacion _cotizacionoBusiness;

        public CotizacionController(ICotizacion cotizacion)
        {
            _cotizacionoBusiness = cotizacion;
        }

        [HttpPost]
        [Route("registrar")]
        public async Task<IActionResult> RegistrarCotizacion(Cotizacion request) 
        {
            ResponseStatus result = new ResponseStatus();
            try
            {
                //Registra la cotizacion, si es exitoso el registro , crea la cotizacion en el sistema
                result = await _cotizacionoBusiness.RegistrarCotizacion(request);
                return Ok(result);
            }
            catch(Exception e)
            {
                result.Status = Configuracion.StatusError;
                result.Message = e.Message;
                return StatusCode(500, result);
            }
        }


        [HttpGet]
        [Route("listarsolicitudes")]
        public Response<List<CotizacionesPorSolicitud>> ListarSolicitudesCotizacion()
        {
            Response<List<CotizacionesPorSolicitud>> r = new Response<List<CotizacionesPorSolicitud>>();
            r.Data = _cotizacionoBusiness.ListarSolicitudesCotizacion();
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }

        [HttpGet]
        [Route("listarcotizacionesproveedor")]
        public Response<List<SolicitudCotizacion>> listCotizacionProveedor(int idProveedor)
        {
            Response<List<SolicitudCotizacion>> r = new Response<List<SolicitudCotizacion>>();
            r.Data = _cotizacionoBusiness.ListCotizacionProveedor(idProveedor);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }

        [HttpGet]
        [Route("listarcotizacionesestadoproveedor")]
        public Response<List<CotizacionesEstado>> listCotizacionEstadoProveedor(int idProveedor)
        {
            Response<List<CotizacionesEstado>> r = new Response<List<CotizacionesEstado>>();
            r.Data = _cotizacionoBusiness.ListCotizacionProveedorEstado(idProveedor);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }


        [HttpGet]
        [Route("listarcotizaciones")]
        public Response<List<CotizacionProveedor>> ListarCotizaciones(int id, int estado)
        {
            Response<List<CotizacionProveedor>> r = new Response<List<CotizacionProveedor>>();
            r.Data = _cotizacionoBusiness.ListarCotizacionesOfertadas(id, estado);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }

        [HttpGet]
        [Route("listaradjuntos")]
        public async Task<Response<List<AdjuntoCotizacion>>> ListarAdjuntos(int solicitud, int codigoProveedor)
        {
            Response<List<AdjuntoCotizacion>> r = new Response<List<AdjuntoCotizacion>>();
            r.Data = await _cotizacionoBusiness.ListarAdjuntosCotizacion(solicitud, codigoProveedor);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }

        [HttpPost]
        [Route("adjudicar")]
        public IActionResult Adjudicar(Adjudicacion request)
        {
            string result = _cotizacionoBusiness.Adjudicar(request);
            if (result == "OK")
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet]
        [Route("getadjudicadoxsolicitud")]
        public Response<Adjudicacion> GetAdjudicadoXSolicitud(int codigoSolicitud)
        {
            Response<Adjudicacion> r = new Response<Adjudicacion>();
            r.Data = _cotizacionoBusiness.GetAdjudicadoXSolicitud(codigoSolicitud);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }


        [HttpGet]
        [Route("getdocumentosrequeridos")]
        public Response<List<DocumentoInvitacion>> GetDocumentosRequeridos(int codigoSolicitud)
        {
            Response<List<DocumentoInvitacion>> r = new Response<List<DocumentoInvitacion>>();
            r.Data = _cotizacionoBusiness.GetDocumentosRequeridos(codigoSolicitud);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }

        /// <summary>
        /// Valida el archivo con informacion de los elementos de la cotizacion con los valores indicados por el proveedor
        /// </summary>
        /// <param name="request">archivo B64</param>
        /// <returns></returns>
        [HttpPost]
        [Route("cargamasiva")]
        public IActionResult CargaMasiva([FromBody] CotizacionMasiva request)
        {
            try
            {
                if (request is null) return BadRequest();
                // Valida el archivo
                var validaciones = _cotizacionoBusiness.ValidarCargaMasiva(request);

                if (validaciones.Any(x => !string.IsNullOrEmpty(x.error))) return BadRequest(validaciones);

                // Transforma el archivo en detalle de solicitud
                var result = _cotizacionoBusiness.TransformarArchivo(validaciones);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }
        }

        [HttpGet]
        [Route("listarfichatecnica")]
        public async Task<IActionResult> ListarFichaTecnica(int idSolicitud, int idProveedor)
        {
            try
            {
                Response<List<DocumentoFichaTecnica>> r = new Response<List<DocumentoFichaTecnica>>();
                r.Data = await _cotizacionoBusiness.ObtenerListaFichasTecnicas(idSolicitud, idProveedor);
                r.Status = new ResponseStatus { Status = "OK", Message = "" };

                return Ok(r);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }
        }

    }
}