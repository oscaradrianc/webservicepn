namespace Negocio.Model
{
    public class CotizacionProveedor
    {
        public int CodigoCotizacion { get; set; }
        public int CodigoProveedor { get; set; }
        public decimal CantidadSolicitud { get; set; }
        public string UnidadSolicitud { get; set; }
        public int Catalogo { get; set; }
        public string NombreProducto { get; set; }
        public decimal CantidadCotizacion { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorIVA { get; set; }
        public string RazonSocial { get; set; }
        public string Identificacion { get; set; }

    }
}
