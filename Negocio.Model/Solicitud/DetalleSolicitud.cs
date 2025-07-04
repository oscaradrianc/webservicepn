namespace Negocio.Model
{
    public class DetalleSolicitud
    {
        public int CodigoDetalle { get; set; }
        public decimal Cantidad { get; set; }
        public string Caracteristicas { get; set; }
        public int CodigoSolicitud { get; set; }
        public int Catalogo { get; set; }
        public string Medida { get; set; }
        public int Id { get; set; }
    }
}