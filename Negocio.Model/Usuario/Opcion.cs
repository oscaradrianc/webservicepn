namespace Negocio.Model
{
    public class Opcion
    {
        public int IdOpcion { get; set; }
        public string Nombre { get; set; }
        public string Url { get; set; }
        public int? Padre { get; set; }
        public int Orden { get; set; }
        public string EsTitulo { get; set; }
        public string Estado { get; set; }
        public string Icono { get; set; }
    }
}
