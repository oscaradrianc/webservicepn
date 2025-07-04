using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Negocio.Business;
using Negocio.Model;
using Org.BouncyCastle.Utilities.Collections;
using RedisManager.Cache;


namespace SWNegocio.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UtilidadesController : ControllerBase
    {
        private readonly IUtilidades _utilidadesBusiness;
        private readonly IConfiguration _configuration;
        private readonly IStorageService _storageService;

        public UtilidadesController(IUtilidades utilidades, IConfiguration configuracion, IStorageService storageService)
        {
            _utilidadesBusiness = utilidades;
            _configuration = configuracion;
            _storageService = storageService;
        }

        [HttpGet]
        [Route("actividadesconomicas")]
        public Response<List<ActividadEconomica>> ObtenerActividadEconomica()
        {
            Response<List<ActividadEconomica>> response = new Response<List<ActividadEconomica>>();
            response.Data = _utilidadesBusiness.ObtenerActividadEconomica();
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;

        }

        [HttpGet]
        [Route("obtenercatalogo")]
        public Response<List<Catalogo>> ObtenerCatalogo()
        {
            Response<List<Catalogo>> response = new Response<List<Catalogo>>();           
            response.Data = _utilidadesBusiness.ObtenerCatalogo();
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;

        }

        [HttpGet]
        [Route("obtenercatalogo/{id}")]
        public Response<List<Catalogo>> ObtenerCatalogo(string id)
        {
            Response<List<Catalogo>> response = new Response<List<Catalogo>>();
            response.Data = _utilidadesBusiness.ObtenerCatalogo(id);
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;

        }


        /// <summary>
        /// Servicio retorna parametros
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Cached(600)]
        [Route("clasevalor")]
        public async Task<Response<List<ClaseValor>>> ObtenerClaseValor(int idClase)
        {
            Response<List<ClaseValor>> response = new Response<List<ClaseValor>>();
            response.Data = await _utilidadesBusiness.ObtenerClaseValor(idClase);
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;
        }

        /// <summary>
        /// Servicio para obtener listado de paises
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("obtenerpais")]
        public Response<List<Pais>> ObtenerPais()
        {
            Response<List<Pais>> response = new Response<List<Pais>>();

            var lta = _utilidadesBusiness.ObtenerPais();
                
                response.Data = lta;
                response.Status = new ResponseStatus { Status = "OK", Message = "" };
                return response;
            
        }

        /// <summary>
        /// Servicio para obtener listado de departamentos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("obtenerdepto")]
        public Response<List<Departamento>> ObtenerDepartamento()
        {
            Response<List<Departamento>> response = new Response<List<Departamento>>();
            var lta = _utilidadesBusiness.ObtenerDepartamento();
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;
            
        }

        /// <summary>
        /// Servicio para obtener listado de municipios
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("obtenermpio")]
        public Response<List<Municipio>> ObtenerMunicipio()
        {
            Response<List<Municipio>> response = new Response<List<Municipio>>();
            response.Data = _utilidadesBusiness.ObtenerMunicipios();
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;

        }

        /// <summary>
        /// Servicio para obtener listado de municipios
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("obtenerdocumentosxpersona")]
        public Response<List<DocumentosxPersona>> DocumentosxPersona()
        {
            Response<List<DocumentosxPersona>> response = new Response<List<DocumentosxPersona>>();
            var lta = _utilidadesBusiness.ObtenerDocumentos();
            response.Data = lta;
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;
            
        }

        [HttpGet]
        [Route("obtenerareas")]
        public Response<List<Areas>> ObtenerAreas()
        {
            Response<List<Areas>> response = new Response<List<Areas>>();
            var lta = _utilidadesBusiness.ObtenerAreas();
            response.Data = lta;
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;

        }

        [HttpGet]
        [Route("obtenergerencias")]
        public Response<List<Gerencias>> ObtenerGerencias()
        {
            Response<List<Gerencias>> response = new Response<List<Gerencias>>();
            var lta = _utilidadesBusiness.ObtenerGerencias();
            response.Data = lta;
            response.Status = new ResponseStatus { Status = "OK", Message = "" };
            return response;
        }

        /// <summary>
        /// Retorna la cadena encriptada
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("encriptar")]
        public Response<string> Encriptar(Encriptar request)
        {
            Response<string> response = new Response<string>();
            try
            {
                response.Data = _utilidadesBusiness.Encriptar(request.Texto, _configuration.GetSection("EncryptedKey").Value);
                response.Status = new ResponseStatus { Status = "OK", Message = "" };
                return response;
            }
            catch (Exception ex)
            {
                response.Status = new ResponseStatus { Status = "BAD", Message = ex.Message };
                return response;
            }

        }

        /// <summary>
        /// Retorna la cadena encriptada
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("constante")]
        public Response<string> GetConstante(string constante)
        {
            Response<string> response = new Response<string>();
            try
            {
                response.Data = _utilidadesBusiness.GetConstante(constante);
                response.Status = new ResponseStatus { Status = "OK", Message = "" };
                return response;
            }
            catch (Exception ex)
            {
                response.Status = new ResponseStatus { Status = "BAD", Message = ex.Message };
                return response;
            }
            
        }

        [HttpGet]
        [Route("viewfile")]
        public async Task<IActionResult> ViewFile(int id)
        {
            try
            {
                var doc = await _utilidadesBusiness.ObtenerDocumentoxId(id);

                if(doc !=null)
                {
                    var file = await _storageService.GetFileStreamAsync(doc.DOCURUTA);
                    return File(file, doc.DOCUCONTENTTYPE, doc.DOCUNOMBRE);
                }
                else
                    { return NotFound(); }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("validarfile")]
        public async Task<string> ValidarFile(string basePath)
        {

        }


    }
}