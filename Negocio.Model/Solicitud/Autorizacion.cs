namespace Negocio.Model
{
    public class Autorizacion
    {
        public int CodigoSolicitud { get; set; }
        public string EstadoAutorizacion { get; set; }
        public int EstadoActual { get; set; }
        public string Observacion { get; set; }
        public int IdUsuario { get; set; }
        public string TipoAutorizacion { get; set; }

    }
}