using System;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class SolicitudCotizacion
    {
        public int CodigoSolicitud { get; set; }
        public int CodigoCotizacion { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int Usuario { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public int TipoContratacion { get; set; }
        public string Etapa { get; set; }
        public string ObservacionAutorizacion { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public DateTime? FechaPregunta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public DateTime? FechaPropuestas { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int Area { get; set; }
        public int? NumeroSAIA { get; set; }
        public int? ProyectoSAIA { get; set; }
        public DateTime? FechaSAIA { get; set; }
        public string TipoSolicitud { get; set;  }
        public string Presupuesto { get; set; }
        public long ValorSAIA { get; set; }
        public long ValorCotizacion { get; set; }
        public List<DetalleSolicitud> ArticulosSolicitud { get; set; }
        public List<DetalleCotizacion> ArticulosCotizacion { get; set; }
        public List<Documento> Terminos { get; set; }
        public List<Documento> Anexos { get; set; }

    }
}