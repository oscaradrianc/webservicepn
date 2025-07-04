namespace Negocio.Model
{
    public class EstadoSolicitud
    {
        public int CodigoEstado { get; set; }
        public string Nombre { get; set; }
        public int EstadoAnterior { get; set; }
        public int EstadoSiguiente { get; set; }
        public string EstadoRegistro { get; set; }
        public string Alias { get; set; }
    }
}
