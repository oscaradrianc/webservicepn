using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Data;
using Negocio.Model;
using Negocio.Model.Consultas;
using System.Collections.Generic;


namespace SWNegocio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ConsultaController : ControllerBase
    {

        private readonly IConsultas _consultaBusiness;

        public ConsultaController(IConsultas consulta)
        {
           _consultaBusiness = consulta;
        }

        [HttpGet, Route("getpagos")]
        public Response<List<FOBTENERPAGOResult>> GetPagos(int idEmpresa, decimal idProveedor, int periodoInicial, int periodoFinal)
        {
            return _consultaBusiness.ObtenerPagos(idEmpresa, idProveedor, periodoInicial, periodoFinal);
        }

        [HttpGet, Route("getdetallepago")]
        public Response<DetallePagoResponse> GetDetallePago(int idEmpresa, int vigOrpo, int orpa, int nroAuxiliar)
        {
            return _consultaBusiness.ObtenerDetallePago(idEmpresa, vigOrpo, orpa, nroAuxiliar);
        }

        [HttpGet, Route("getretencion")]
        public Response<RetencionResponse> GetRetencion(int idProveedor, string tipoRetencion, int periodo1, int periodo2)
        {

            return _consultaBusiness.ObtenerRetenciones(idProveedor, tipoRetencion, periodo1, periodo2);
        }

        [HttpGet, Route("getestadoprocesos")]
        public Response<List<EstadoProceso>> GetEstadoProcesos(int? idSolicitud, string fechaInicial, string fechaFinal, string estado)
        {
            return _consultaBusiness.ObtenerEstadoProcesos(idSolicitud, fechaInicial, fechaFinal, estado);
        }

        [HttpGet, Route("getsolicitudesanuladas")]
        public Response<List<SolicitudAnulado>> GetSolicitudesAnuladas(string fechaInicial, string fechaFinal)
        {
            return _consultaBusiness.ObtenerSolicitudesAnuladas(fechaInicial, fechaFinal);
        }
    }
}
