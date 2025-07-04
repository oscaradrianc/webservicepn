using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface ICotizacion
    {
        Task<ResponseStatus> RegistrarCotizacion(Cotizacion request);
        List<CotizacionesPorSolicitud> ListarSolicitudesCotizacion();
        List<SolicitudCotizacion> ListCotizacionProveedor(int idProveedor);
        List<CotizacionesEstado> ListCotizacionProveedorEstado(int idProveedor);
        List<CotizacionProveedor> ListarCotizacionesOfertadas(int codigoSolicitud, int estado);
        Task<List<AdjuntoCotizacion>> ListarAdjuntosCotizacion(int solicitud, int codigoProveedor);
        List<DocumentoInvitacion> GetDocumentosRequeridos(int codigoSolicitud);
        string Adjudicar(Adjudicacion request);
        Adjudicacion GetAdjudicadoXSolicitud(int codigoSolicitud);        
        List<ValoresArchivo> ValidarCargaMasiva(CotizacionMasiva request);
        List<DetalleCotizacion> TransformarArchivo(List<ValoresArchivo> validaciones);
        Task<List<DocumentoFichaTecnica>> ObtenerListaFichasTecnicas(int idSolicitud, int idProveedor);

    }
}
