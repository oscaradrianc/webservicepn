using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Negocio.Model
{
    public class Cotizacion
    {
        public int CodigoCotizacion { get; set; }
        public DateTime FechaCotizacion { get; set; }
        public decimal ValorCotizacion { get; set; }
        public int PorcentajeIVA { get; set; }

        // SAFE: Cotizacion is only used in RegistrarCotizacion (create only, no update methods in ICotizacion)
        // The FK fields are used in WHERE clause and INSERT, so 0 would cause logical errors
        [Range(1, int.MaxValue, ErrorMessage = "El codigo de proveedor debe ser mayor a cero")]
        public int CodigoProveedor { get; set; }

        // SAFE: Same reasoning as CodigoProveedor - only used in create operations
        [Range(1, int.MaxValue, ErrorMessage = "El codigo de solicitud debe ser mayor a cero")]
        public int CodigoSolicitud { get; set; }

        // SAFE: Same reasoning as above - only used in create operations
        [Range(1, int.MaxValue, ErrorMessage = "El codigo de usuario debe ser mayor a cero")]
        public int CodigoUsuario { get; set; }

        public string Observacion { get; set; }
        public int FormaPago { get; set; }
        public DateTime FechaEntrega { get; set; }
        public List<DetalleCotizacion> ElementosCotizacion { get; set; }
        public List<Documento> DocumentoCotizacion { get; set; }
        public List<Documento> DocumentoAdicional { get; set; }
        public string NombreProveedor { get; set; }
    }
}
