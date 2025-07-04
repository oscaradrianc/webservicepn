using System;

namespace Negocio.Model
{
    public class EstadoProceso
    {
        public int CodigoSolicitud { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string EstadoNombre { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public DateTime? FechaPregunta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string TipoSolicitud { get; set; }        
        public int NroCotizaciones { get; set; }
    }
}
