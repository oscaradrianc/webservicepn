using Negocio.Data;
using Negocio.Model;
using Negocio.Model.Consultas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negocio.Business
{
    public interface IConsultas
    {
        //Response<IQueryable<FOBTENERPAGOResult>> ObtenerPagos(int idEmpresa, decimal idProveedor, int periodoInicial, int periodoFinal);
        Response<List<FOBTENERPAGOResult>> ObtenerPagos(int idEmpresa, decimal idProveedor, int periodoInicial, int periodoFinal);
        Response<DetallePagoResponse> ObtenerDetallePago(int idEmpresa, int vigOrpa, int orpa, int nroAuxiliar);
        Response<RetencionResponse> ObtenerRetenciones(int idProveedor, string tipoRetencion, int periodo1, int periodo2);
        Response<List<EstadoProceso>> ObtenerEstadoProcesos(int? idSolicitud, string fechaInicial, string fechaFinal, string estado);
        Response<List<SolicitudAnulado>> ObtenerSolicitudesAnuladas(string fechaInicial, string fechaFinal);
    }
}
