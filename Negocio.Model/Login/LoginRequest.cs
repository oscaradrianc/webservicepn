using System.ComponentModel.DataAnnotations;

namespace Negocio.Model
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contrasena es requerida")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El origen es requerido")]
        public string Origen { get; set; } //I Interno, P Usuario proveedor para controlar el login del frontend o backend sea accesido por el tipo de usuario respectivo
    }
}