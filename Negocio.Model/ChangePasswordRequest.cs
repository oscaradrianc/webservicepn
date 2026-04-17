using System.ComponentModel.DataAnnotations;

namespace Negocio.Model
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contrasena es requerida")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La nueva contrasena es requerida")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "El origen es requerido")]
        public string Origen { get; set; }
    }
}
