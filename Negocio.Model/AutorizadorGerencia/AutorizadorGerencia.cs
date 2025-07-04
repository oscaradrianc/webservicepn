using System;

namespace Negocio.Model
{
    public class AutorizadorGerencia
    {
        public int IdAutorizadorGerencia { get; set; }
        public int IdGerencia { get; set; }
        public int IdUsuario { get; set; }
        public int LogsUsuario { get; set; }
        public DateTime LogsFecha { get; set; }            
    }
}
