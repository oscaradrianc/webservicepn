using System;

namespace Negocio.Model
{
    public class Saia
    {
        public string Numero { get; set; }
        public string Proyecto { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Descripcion { get; set; }
        public decimal? ValorEstimado { get; set; }
    }
}
