namespace Negocio.Model
{
    public class ChangePasswordRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string Origen { get; set; }
    }
}
