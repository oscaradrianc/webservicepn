using System.Collections.Generic;

namespace Negocio.Model
{
    public class DetalleCotizacion
    {
        public int CodigoDetalleCot { get; set; }
        public int CodigoCotizacion { get; set; }
        public int CantidadRequerida { get; set; }
        public int Cantidad { get; set; }
        public decimal ValorIVA { get; set; }
        public decimal ValorUnitario { get; set; }
        public int Catalogo { get; set; }
        public decimal PorcentajeIVA { get; set; }
        public List<Documento> FichaTecnica { get; set; }
    }
}
