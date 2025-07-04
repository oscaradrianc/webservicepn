using System;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class SolicitudCompra
    {
        public int CodigoSolicitud { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public int Usuario { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string EstadoNombre { get; set; }
        public string TipoAcceso { get; set; }
        public int TipoContratacion { get; set; }
        public string Etapa { get; set; }
        public string ObservacionAutorizacion { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public DateTime? FechaPregunta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public DateTime? FechaPropuestas { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int Area { get; set; }
        public string NumeroSAIA { get; set; }
        public int? ProyectoSAIA { get; set; }
        public DateTime? FechaSAIA { get; set; }
        public string TipoSolicitud { get; set;  }
        public string Presupuesto { get; set; }
        public long ValorSAIA { get; set; }
        public List<DetalleSolicitud> ArticulosSolicitud { get; set; }
        public List<Documento> Terminos { get; set; }
        public List<Documento> Anexos { get; set; }
        public List<DocumentoInvitacion> DocumentosInvitacion { get; set; }

    }
}