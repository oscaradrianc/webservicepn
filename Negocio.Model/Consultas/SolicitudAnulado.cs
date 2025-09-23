using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Model.Consultas
{
    public class SolicitudAnulado
    {
        public int CodigoSolicitud { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string EstadoNombre { get; set; }
        public double? Valor { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public string Area { get; set; }
        public string UsuarioAnulo { get; set; }
        public string MotivoAnulo { get; set; }
    }
}
