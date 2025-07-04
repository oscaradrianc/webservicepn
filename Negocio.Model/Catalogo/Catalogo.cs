using System;

namespace Negocio.Model
{
    public class Catalogo
    {
        public int CodigoInterno { get; set; }
        public string CodigoCatalogo { get; set; }
        public string Tipo { get; set; }
        public string Nombre { get; set; }
        public int UnidadMedida { get; set; }
        public string Estado { get; set; }
        public string Medida { get; set; }
        public int LogsUsuario { get; set; }
        public DateTime LogsFecha { get; set; }
    }
}
