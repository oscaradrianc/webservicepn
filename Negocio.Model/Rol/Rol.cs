using System.Collections.Generic;

namespace Negocio.Model
{
    public class Rol
    {
        public decimal Id { get; set; }

        public string Nombre { get; set; }

        public string Estado { get; set; }
        public string Observacion { get; set; }
        public int LogsUsuario { get; set; }

        public List<OpcionxRol> ListaOpciones { get; set; }


    }
}
