using Mapster;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class NotificacionRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PONENOTICIA, Noticias>()
                .Map(dest => dest.CodigoNoticia, src => src.NOTINOTICIA)
                .Map(dest => dest.Titulo, src => src.NOTITITULO)
                .Map(dest => dest.Fecha, src => src.NOTIFECHA)
                .Map(dest => dest.Contenido, src => src.NOTICONTENIDO)
                .Map(dest => dest.Estado, src => src.NOTIESTADO)
                .Map(dest => dest.URL, src => src.NOTIURL)
                // DTO-only fields — no DB columns for these on PONENOTICIA
                .Ignore(dest => dest.CorreoNotificacion)
                .Ignore(dest => dest.ArchivoB64)
                .Ignore(dest => dest.FotoB64)
                .TwoWays();
        }
    }
}
