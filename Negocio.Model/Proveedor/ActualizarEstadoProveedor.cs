namespace Negocio.Model
{
    public class ActualizarEstadoProveedor
    {
        public int CodigoProveedor { get; set; }
        public string Estado { get; set; }
        public int UsuarioAutoriza { get; set; }
        public Documento DocumentoRevision { get; set; }

    }
}
