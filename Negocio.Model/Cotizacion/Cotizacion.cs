using System;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class Cotizacion
    {
        public int CodigoCotizacion { get; set; }
        public DateTime FechaCotizacion { get; set; }
        public decimal ValorCotizacion { get; set; }
        public int PorcentajeIVA { get; set; }
        public int CodigoProveedor { get; set; }
        public int CodigoSolicitud { get; set; }
        public int CodigoUsuario { get; set; }
        public string Observacion { get; set; }
        public int FormaPago { get; set; }
        public DateTime FechaEntrega { get; set; }
        public List<DetalleCotizacion> ElementosCotizacion { get; set; }
        public List<Documento> DocumentoCotizacion { get; set; }
        public List<Documento> DocumentoAdicional { get; set; }
        public string NombreProveedor { get; set; }
    }
}
