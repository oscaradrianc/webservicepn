using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;


namespace SWNegocio.Controllers
{

    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class PreguntasController : ControllerBase
    {
        private readonly IPreguntas _preguntasBusiness;

        public PreguntasController(IPreguntas preguntas)
        {
            _preguntasBusiness = preguntas;
        }

        [HttpGet]
        [Route("list")]
        public Response<List<Preguntas>> ListPreguntas(int idSolicitud)
        {
            Response<List<Preguntas>> r = new Response<List<Preguntas>>();
            r.Data = _preguntasBusiness.ListPreguntas(idSolicitud);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }


        [HttpPost]
        [Route("preguntar")]
        public IActionResult CrearPregunta(CrearPregunta request)
        {
            string result = _preguntasBusiness.CrearPregunta(request);
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
        [Route("listsinrespuesta")]
        public Response<List<Preguntas>> ListPreguntasSinRespuesta(int idSolicitud)
        {
            Response<List<Preguntas>> r = new Response<List<Preguntas>>();
            r.Data = _preguntasBusiness.ListPreguntasSinRespuesta(idSolicitud);
            r.Status = new ResponseStatus { Status = "OK", Message = "" };

            return r;
        }


        [HttpPost]
        [Route("responder")]
        public IActionResult CrearRespuesta(CrearRespuesta request)
        {
            string result = _preguntasBusiness.CrearRespuesta(request);
            if (result == "OK")
            {
                return Ok();
            }
            else
            {
                return Content(HttpStatusCode.BadRequest.ToString(), result);
            }
        }

    }
}