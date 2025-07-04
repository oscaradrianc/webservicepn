using System.Collections.Generic;

namespace Negocio.Model
{
    public class NotificacionAdjudicacion
    {
        public int CodigoSolicitud { get; set; }
        public string RazonSocial { get; set; }        
        public string Nit { get; set; }
        public string Descripcion { get; set; }     
        public List<ProveedorNotificacion> ProveedorAdjudicado { get; set; }
    }
}
