using System;

namespace Negocio.Model
{
    public class Preguntas
    {
        public int CodigoPregunta { get; set; }
        public DateTime FechaPregunta { get; set; }
        public string Pregunta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string Respuesta { get; set; }
        public int CodigoSolicitud { get; set; }
        public int CodigoProveedor { get; set; }
        public int UsuarioRespuesta { get; set; }
        public string NombreProveedor { get; set; }
        public string NombreUsuario { get; set; }
    }
}
