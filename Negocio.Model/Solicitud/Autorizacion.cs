using System.ComponentModel.DataAnnotations;

namespace Negocio.Model
{
    public class Autorizacion
    {
        [Range(1, int.MaxValue, ErrorMessage = "El codigo de solicitud debe ser mayor a cero")]
        public int CodigoSolicitud { get; set; }

        [Required(ErrorMessage = "El estado de autorizacion es requerido")]
        public string EstadoAutorizacion { get; set; }

        public int EstadoActual { get; set; }

        public string Observacion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El id de usuario debe ser mayor a cero")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El tipo de autorizacion es requerido")]
        public string TipoAutorizacion { get; set; }

    }
}