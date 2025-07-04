using System;

namespace Negocio.Model
{
    public class Noticias
    {
        public int CodigoNoticia { get; set; }
        public string Titulo { get; set; }
        public DateTime Fecha { get; set; }
        public string Contenido { get; set; }
        public string Estado { get; set; }
        public string CorreoNotificacion { get; set; }
        public string URL { get; set; }
        public byte[] FotoB64 { get; set; }
        public string ArchivoB64 { get; set; }
    }
}
