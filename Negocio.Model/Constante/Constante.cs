using Microsoft.Extensions.Configuration;
using System;


namespace Negocio.Model
{
    public class Constante
    {
        public int IdConstante { get; set; }
        public string Referencia { get; set; }
        public string Descripcion { get; set; }
        public string Valor { get; set; }
        public DateTime LogsFecha { get; set; }
        public int LogsUsuario { get; set; }
    }
}
