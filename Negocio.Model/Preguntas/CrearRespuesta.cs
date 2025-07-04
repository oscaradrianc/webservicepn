namespace Negocio.Model
{
    public class CrearRespuesta
    {
        public int CodigoSolicitud { get; set; }
        public int CodigoProveedor { get; set; }
        public int CodigoPregunta { get; set; }
        public string Respuesta { get; set; }
        public int UsuarioRespuesta { get; set; }
    }
}
