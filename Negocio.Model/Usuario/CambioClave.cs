using System.ComponentModel.DataAnnotations;

namespace Negocio.Model
{
    public class CambioClave
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La clave anterior es requerida")]
        public string ClaveAnterior { get; set; }

        [Required(ErrorMessage = "La nueva clave es requerida")]
        public string NuevaClave { get; set; }
    }
}
