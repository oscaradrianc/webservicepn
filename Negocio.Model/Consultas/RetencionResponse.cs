using Negocio.Data;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class RetencionResponse
    {
        public FENCABEZADORETENCIONResult EncabezadoRetencion { get; set; }
        public List<FDETALLERETENCIONResult> DetalleRetencion { get; set; }
    }
}
