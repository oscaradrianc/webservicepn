using System;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class CotizacionesPorSolicitud
    {
        public int CodigoSolicitud { get; set; }
        public int? CodigoCotizacion { get; set; }
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; }
        public DateTime? FechaCotizacion { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? Area { get; set; }
        public long? Valor { get; set; }
        public List<DetalleSolicitud> ArticulosSolicitud { get; set; }
        
    }
}
