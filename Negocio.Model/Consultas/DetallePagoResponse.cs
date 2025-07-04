using Negocio.Data;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class DetallePagoResponse 
    {
        public FDETALLEPAGOResult DetallePago { get; set; }
        public List<FDESCUENTOPAGOResult> Descuentos { get; set; }
    }
}
