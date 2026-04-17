using AutoMapper;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class NotificacionProfile : Profile
    {
        public NotificacionProfile()
        {
            CreateMap<PONENOTICIA, Noticias>()
                .ForMember(d => d.CodigoNoticia, opt => opt.MapFrom(src => src.NOTINOTICIA))
                .ForMember(d => d.Titulo, opt => opt.MapFrom(src => src.NOTITITULO))
                .ForMember(d => d.Fecha, opt => opt.MapFrom(src => src.NOTIFECHA))
                .ForMember(d => d.Contenido, opt => opt.MapFrom(src => src.NOTICONTENIDO))
                .ForMember(d => d.Estado, opt => opt.MapFrom(src => src.NOTIESTADO))
                .ForMember(d => d.URL, opt => opt.MapFrom(src => src.NOTIURL))
                .ForMember(d => d.CorreoNotificacion, opt => opt.Ignore())
                .ForMember(d => d.ArchivoB64, opt => opt.Ignore())
                .ForMember(d => d.FotoB64, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}