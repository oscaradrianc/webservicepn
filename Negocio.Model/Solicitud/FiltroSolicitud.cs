using System;

namespace Negocio.Model
{
    public class FiltroSolicitud
    {
        
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public int Area { get; set; }
        public int CodigoSolicitud { get; set; }
        public int EstadoSolicitud { get; set; }
    }
}
