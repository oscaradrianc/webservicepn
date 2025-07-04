using System;
using System.Collections.Generic;

namespace Negocio.Model
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        //public string NombreUsuario { get; set; }
        public string Identificador { get; set; }
        public string Nombres { get; set; }
        public string Identificacion { get; set; }
        public string Estado { get; set; }
        public string Clave { get; set; }
        public int? LogsUsuario { get; set; }
        public DateTime? LogsFecha { get; set; }
        public string Email { get; set; }
        public string Tipo { get; set; }
        public string UrlDefecto { get; set; }
        public int? IdRol { get; set; }
        public int? IdProveedor { get; set; }
        public int? IdArea { get; set; }
        public string VenceClave { get; set; }
        public DateTime? FechaVence { get; set; }
        public string CambiarClave { get; set; }
        public int ResultadoLogin { get; set; }
        public string Token { get; set; }
        //public string RefreshToken { get; set; }
        //public DateTime RefreshTokenExpiryTime { get; set; }
        public Proveedor Proveedor { get; set; }
        public List<Opcion> Opciones { get; set; }
    }
}
