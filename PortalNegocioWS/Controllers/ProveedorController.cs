using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SWNegocio.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedor _proveedorBusiness;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProveedorController(IProveedor proveedor, IMapper mapper, ILogger<ProveedorController> logger)
        {
            _proveedorBusiness = proveedor;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("registrar")]
        public async Task<IActionResult> RegistrarProveedor(Proveedor request)
        {
            try
            {
                //Registra el proveedor, si es exitoso el registro del proveedor, crea el usuario del sistema
                var result = await _proveedorBusiness.RegistrarProveedor(request);                
                return Ok(result);                
            }
            catch(Exception e)
            {
                return Content(HttpStatusCode.InternalServerError.ToString(), e.Message);
            }            
        }



        [HttpGet("{id}")]
        public IActionResult Get(decimal id)
        {
            Proveedor proveedor = _proveedorBusiness.ObtenerProveedor((int)id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return Ok(proveedor);
        }


        [HttpGet]
        [Route("proveedorxautorizar")]
        public async Task<Response<List<Proveedor>>> ObtenerProveedorXEstado(string estado)
        {
            Response<List<Proveedor>> response = new Response<List<Proveedor>>();
            List<Proveedor> proveedores = await _proveedorBusiness.ObtenerProveedorXEstado(estado);
            response.Data = proveedores;
            response.Status = new ResponseStatus { Status = "OK", Message = "" };

            return response;
        }


        [HttpPost]
        [Route("autorizar")]
        public async Task<IActionResult> AutorizarProveedor(ActualizarEstadoProveedor estadoProveedor)
        {
            string result = await _proveedorBusiness.AutorizarProveedor(estadoProveedor);
            if (result == "OK")
            {
                return Ok();
            }
            else
            {
                return Content(HttpStatusCode.BadRequest.ToString(), result);
            }
        }

        [HttpPost]
        [Route("actualizarestado")]
        public IActionResult ActualizarEstado(ActualizarEstadoProveedor estadoProveedor)
        {
            string result = _proveedorBusiness.CambiarEstadoProveedor(estadoProveedor);
            if (result == "OK")
            {
                return Ok();
            }
            else
            {
                return Content(HttpStatusCode.BadRequest.ToString(), result);
            }
        }

        [HttpGet]
        [EnableCors]
        [Route("getdatosbasicos")]
        public Response<List<ProveedorDatosBasicos>> ConsultarDatosBasicosProveedor()
        {            
            Response<List<ProveedorDatosBasicos>> resp = new Response<List<ProveedorDatosBasicos>>();
            resp.Data = _proveedorBusiness.ConsultarDatosBasicosProveedor();
            resp.Status = new ResponseStatus { Status = Configuracion.StatusOk };
           
            return resp;
        }


        [HttpGet]
        [Route("obtenerproveedor")]
        public Response<Proveedor> ConsultarProveedor(int idProveedor)
        {
            Response<Proveedor> resp = new Response<Proveedor>();
            resp.Data = _proveedorBusiness.ObtenerProveedor(idProveedor);
            resp.Status = new ResponseStatus { Status = Configuracion.StatusOk };

            return resp;
        }

        [HttpPost]
        [Route("actualizar")]
        public async Task<IActionResult> ActualizarProveedor(Proveedor request)
        {

            //Actualiza el proveedor, si es exitoso el registro del proveedor, crea el usuario del sistema
            await _proveedorBusiness.ActualizarProveedor(request);
           // if (result == "OK")
            {
                return Ok();
            }
           /* else
            {
                return Content(HttpStatusCode.BadRequest.ToString(), result);
            }*/

        }

        [HttpPost]
        [Route("actualizardocs")]
        public IActionResult ActualizarDocsProveedor(Proveedor proveedor)
        {

            //Actualiza los documentos de proveedor
            string result = _proveedorBusiness.ActualizarDocsProveedor(proveedor);
            if (result == "OK")
            {
                return Ok();
            }
            else
            {
                return Content(HttpStatusCode.BadRequest.ToString(), result);
            }

        }

        [HttpGet]
        [Route("proveedorporestado")]
        public async Task<IActionResult> ObtenerProveedoresPorEstado()
        {
            Response<List<ProveedorEstado>> res = new Response<List<ProveedorEstado>>();

            try
            {
                res.Data = await _proveedorBusiness.ObtenerCantidadProveedorPorEstado();
                res.Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = "" };
                return Ok(res);
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }            
        }

        [HttpGet]
        [Route("proveedorpormes")]
        public async Task<IActionResult> ObtenerProveedoresRegistradosPorMes(int vigencia)
        {
            Response<List<ProveedoresPorMes>> res = new Response<List<ProveedoresPorMes>>();

            try
            {
                var r = await _proveedorBusiness.ObtenerNroProveedoresRegistradoPorMes(vigencia);
                res.Data = _mapper.Map<List<ProveedoresPorMes>>(r);
                res.Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = "" };

                return Ok(res);
            }
            catch(Exception e)
            {
                _logger.LogError($"ERRRO PN - ObtenerProveedoresRegistradosPorMes: { e.Message } ");
                return StatusCode(500, e.Message);
            }
        }
    }
}