using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface ISolicitudCompra
    {
        Task<string> RegistrarSolicitud(SolicitudCompra request);
        string ActualizarSolicitud(SolicitudCompra request);
        string ActualizarFechasSolicitud(SolicitudCompra request);
        List<Saia> ObtenerSaia();
        List<SolicitudCompra> ListSolicitud(int tipo, FiltroSolicitud filtro);
        List<SolicitudCompra> ListInvitacion(string tipo);
        SolicitudCompra GetAdjuntosSolicitud(int id);
        SolicitudCompra GetSolicitud(int id);
        string AutorizarSolicitud(Autorizacion request);
        List<EstadoSolicitud> GetEstadoSolicitud(string alias);
        List<SolicitudAreaEstado> GetSolicitudesXEstadoYArea();
        List<EstadoSolicitud> GetEstadosSolicitud();
        void CerrarInvitacion(DateTime fecha);
        Task<List<SolicitudCompra>> ListSolicitudPorAutorizador(string estado, int idUsuario);
        List<ValoresArchivo> ValidarCargaMasiva(SolicitudMasiva request);
        List<DetalleSolicitud> TransformarArchivo(List<ValoresArchivo> validaciones);
        Task<List<CotizacionesPorSolicitud>> GetCotizacionesxSolicitud(int idSolicitud);
    }
}
