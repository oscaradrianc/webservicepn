namespace Negocio.Model
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Origen { get; set; } //I Interno, P Usuario proveedor para controlar el login del frontend o backend sea accesido por el tipo de usuario respectivo 
    }
}