using Mapster;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class AuthRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<POGEUSUARIO, Usuario>()
                .Map(dest => dest.IdUsuario, src => src.USUAUSUARIO)
                .Map(dest => dest.Identificador, src => src.USUAIDENTIFICADOR)
                .Map(dest => dest.Nombres, src => src.USUANOMBRE)
                .Map(dest => dest.Identificacion, src => src.USUAIDENTIFICACION)
                .Map(dest => dest.Estado, src => src.USUAESTADO)
                .Map(dest => dest.Clave, src => src.USUACLAVE)
                .Map(dest => dest.LogsUsuario, src => src.LOGSUSUARIO)
                .Map(dest => dest.LogsFecha, src => src.LOGSFECHA)
                .Map(dest => dest.Email, src => src.USUACORREO)
                .Map(dest => dest.Tipo, src => src.USUATIPO)
                .Map(dest => dest.UrlDefecto, src => src.USUAURLDEFECTO)
                .Map(dest => dest.IdRol, src => src.ROLEROL)
                .Map(dest => dest.IdProveedor, src => src.PROVPROVEEDOR)
                .Map(dest => dest.IdArea, src => src.CLASAREA2)
                .Map(dest => dest.VenceClave, src => src.USUAVENCECLAVE)
                .Map(dest => dest.FechaVence, src => src.USUAFECHAVENCE)
                .Map(dest => dest.CambiarClave, src => src.USUACAMBIARCLAVE)
                // DTO-only fields not present on POGEUSUARIO — ignore both directions.
                // TwoWays() + Ignore() correctly prevents writing these in the reverse
                // (Usuario -> POGEUSUARIO) direction since they have no DB column anyway.
                .Ignore(dest => dest.ResultadoLogin)
                .Ignore(dest => dest.Token)
                .Ignore(dest => dest.Proveedor)
                .Ignore(dest => dest.Opciones)
                .TwoWays();

            config.NewConfig<POGEROL, Rol>()
                .Map(dest => dest.Id, src => src.ROLEROL)
                .Map(dest => dest.Nombre, src => src.ROLENOMBRE)
                .Map(dest => dest.Estado, src => src.ROLEESTADO)
                .Map(dest => dest.Observacion, src => src.ROLEOBSERVACION)
                .Map(dest => dest.LogsUsuario, src => src.LOGSUSUARIO)
                .Ignore(dest => dest.ListaOpciones)
                .TwoWays();

            config.NewConfig<POGEOPCIONXROL, OpcionxRol>()
                .Map(dest => dest.IdRolxOpcion, src => src.OPROOPCIONXROL)
                .Map(dest => dest.IdRol, src => src.ROLEROL)
                .Map(dest => dest.IdOpcion, src => src.OPCIOPCION)
                .Map(dest => dest.LogsUsuario, src => src.LOGSUSUARIO)
                .TwoWays();
        }
    }
}
