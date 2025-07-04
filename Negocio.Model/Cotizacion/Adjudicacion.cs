using System.Collections.Generic;

namespace Negocio.Model
{
    public class Adjudicacion
    {
        public  int CodigoAdjudicacion { get; set; }
        //public DateTime FechaAdjudicacion { get; set; }        
        public string Observacion { get; set; }        
        public int CodigoSolicitud { get; set; }
        public int CodigoUsuario { get; set; }
        public string EstadoSolicitud { get; set; }
        public List<Adjudicados> Adjudicados { get; set; }
        //public string NombreProveedor { get; set; }
    }
}
