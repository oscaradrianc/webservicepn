using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SWNegocio.Controllers
{
    [ApiController]    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class SolicitudController : ControllerBase
    {
        private readonly ISolicitudCompra _solicitudBusiness;
        private readonly ILogger<SolicitudController> _logger;
    
        public SolicitudController(ISolicitudCompra solicitudCompra, ILogger<SolicitudController> logger)
        {
            _solicitudBusiness = solicitudCompra;
            _logger = logger;
           
        }

        [HttpPost]
        [Route("registrar")]
        public async Task<IActionResult> RegistrarSolicitud(SolicitudCompra request) 
        {

            //Registra el proveedor, si es exitoso el registro del proveedor, crea el usuario del sistema
            string result = await _solicitudBusiness.RegistrarSolicitud(request);
            if (result == "OK") {
                return Ok();
            } else
            {
                return Content(HttpStatusCode.BadRequest.ToString(), result);
            }

        }

        [HttpPost]
        [Route("actualizar")]
        public IActionResult ActualizarSolicitud(SolicitudCompra request)
        {
            try
            {
                string result = _solicitudBusiness.ActualizarSolicitud(request);
                if (result == "OK")
                {
                    return Ok();
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest.ToString(), result);
                }
            }
            catch(Exception e)
            {
                _logger.LogError($"Error al insertar clase valor: { JsonConvert.SerializeObject(request) } :: { e.Message } ");
                return Content(HttpStatusCode.BadRequest.ToString(), e.Message);
            }
        }


        /// <summary>
        /// Funcion para retornar el listado de datos con los numeros de SAIA
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors]
        [Route("getsaia")]
        public Response<List<Saia>> ObtenerSaia()
        {
            Response<List<Saia>> r = new Response<List<Saia>>
            {
                Data = _solicitudBusiness.ObtenerSaia(),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return r;
        }

        /// <summary>
        /// Funcion para retornar el listado de solicitudes por tipo de solicitud
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("list")]
        public Response<List<SolicitudCompra>> ListSolicitud(int tipo, [FromQuery]FiltroSolicitud filtro)
        {
            Response<List<SolicitudCompra>> r = new Response<List<SolicitudCompra>>
            {
                Data = _solicitudBusiness.ListSolicitud(tipo, filtro),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return r;
        }

        
        /// <summary>
        /// Funcion para retornar las solicitudes pendientes por autorizar segun el usuario
        /// </summary>
        /// <param name="estado"></param>
        /// <param name="idUsuario"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("solicidesxautorizador")]
        public async Task<Response<List<SolicitudCompra>>> ListSolicitudPorAutorizador(string estado, int idUsuario)
        {
            Response<List<SolicitudCompra>> r = new Response<List<SolicitudCompra>>
            {
                Data = await _solicitudBusiness.ListSolicitudPorAutorizador(estado, idUsuario),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return r;
        }



        /// <summary>
        /// Funcion para retornar el listado de solicitudes por tipo de solicitud
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("listhistorico")]
        public Response<List<SolicitudCompra>> ListSolicitudHistorico([FromQuery] FiltroSolicitud filtro)
        {
            Response<List<SolicitudCompra>> r = new Response<List<SolicitudCompra>>
            {
                Data = _solicitudBusiness.ListSolicitud(0, filtro),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return r;
        }

        /// <summary>
        /// Funcion para retornar el listado de solicitudes por tipo de solicitud
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [EnableCors]
        [Route("listinvitacion")]
        public Response<List<SolicitudCompra>> ListInvitaciones(string tipo)
        {
            Response<List<SolicitudCompra>> r = new Response<List<SolicitudCompra>>
            {
                Data = _solicitudBusiness.ListInvitacion(tipo),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return r;
        }

        /// <summary>
        /// Funcion para retornar la solicitud de compra por Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get")]
        public Response<SolicitudCompra> GetSolicitud(int id)
        {
            Response<SolicitudCompra> r = new Response<SolicitudCompra>
            {
                Data = _solicitudBusiness.GetSolicitud(id),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return r;
        }

        /// <summary>
        /// Funcion para autorizar la solicitud de compra por la gerencia
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("Autorizar")]
        public IActionResult AutorizarSolicitud(Autorizacion request)
        {
            
            string result = _solicitudBusiness.AutorizarSolicitud(request);
            if (result == "OK")
            {
                return Ok();
            }
            else
            {
                return Content(HttpStatusCode.BadRequest.ToString(), result);
            }
        }


        /// <summary>
        /// Funcion para retornar la solicitud de compra por Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("getadjuntos")]
        public Response<SolicitudCompra> GetAdjuntosSolicitud(int id)
        {
            Response<SolicitudCompra> r = new Response<SolicitudCompra>
            {
                Data = _solicitudBusiness.GetAdjuntosSolicitud(id),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return r;
        }

        /// <summary>
        /// Actualiza las fechas de la solicitud
        /// </summary>
        /// <param name="request">Objeto Solicitud de Compra</param>
        /// <returns></returns>
        [HttpPost]
        [Route("actualizarfechas")]
        public IActionResult ActualizarFechas(SolicitudCompra request)
        {
            try
            {
                string result = _solicitudBusiness.ActualizarFechasSolicitud(request);
                if (result == "OK")
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError($"Error al insertar clase valor: { JsonConvert.SerializeObject(request) } :: { result } ");
                    return Content(HttpStatusCode.BadRequest.ToString(), result);
                }
            }
            catch(Exception e)
            {
                _logger.LogError($"Error al insertar clase valor: { JsonConvert.SerializeObject(request) } :: { e.Message } ");
                return Content(HttpStatusCode.BadRequest.ToString(), e.Message);
            }
        }


        [HttpGet]
        [Route("obtenersolicitudesxestadoyarea")]
        public Response<List<SolicitudAreaEstado>> GetSolicitudEstadoArea()
        {
            Response<List<SolicitudAreaEstado>> res = new Response<List<SolicitudAreaEstado>>
            {
                Data = _solicitudBusiness.GetSolicitudesXEstadoYArea(),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return res;
        }

        [HttpGet]
        [Route("getestados")]
        public Response<List<EstadoSolicitud>> GetAllEstadosSolicitud()
        {
            Response<List<EstadoSolicitud>> res = new Response<List<EstadoSolicitud>>
            {
                Data = _solicitudBusiness.GetEstadosSolicitud(),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return res;
        }

        /// <summary>
        /// Valida el archivo con informacion de detalle de solicitud
        /// </summary>
        /// <param name="request">archivo B64</param>
        /// <returns></returns>
        [HttpPost]
        [Route("cargamasiva")]
        public IActionResult CargaMasiva(SolicitudMasiva request)
        {
            try
            {
                if (request is null) return BadRequest();
                // Valida el archivo
                var validaciones = _solicitudBusiness.ValidarCargaMasiva(request);

                if(validaciones.Any(x=> !string.IsNullOrEmpty(x.error))) return BadRequest(validaciones);

                // Transforma el archivo en detalle de solicitud
                var result = _solicitudBusiness.TransformarArchivo(validaciones);

                return Ok(result);
            }
            catch(Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }     

        }


        [HttpGet]
        [Route("cotizacionesxsolicitud")]
        public async Task<Response<List<CotizacionesPorSolicitud>>> GetCotizacionesxSolicitud(int idSolicitud)
        {
            Response<List<CotizacionesPorSolicitud>> res = new Response<List<CotizacionesPorSolicitud>>
            {
                Data = await _solicitudBusiness.GetCotizacionesxSolicitud(idSolicitud),
                Status = new ResponseStatus { Status = "OK", Message = "" }
            };

            return res;
        }

    }
}