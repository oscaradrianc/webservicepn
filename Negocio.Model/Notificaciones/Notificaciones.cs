using System;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class Notificacion
    {
        public int IdNotificacion { get; set; }
        public string Nombre { get; set; }
        public string Asunto { get; set; }
        public string Estado { get; set; }
        public string Plantilla { get; set; }
        public String Tipo { get; set; }
        public int LogsUsuario { get; set; }
        public List<Usuario> Usuarios { get; set; }


    }
}
