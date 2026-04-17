using AutoMapper;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class AdministracionProfile : Profile
    {
        public AdministracionProfile()
        {
            CreateMap<POGEAUTORIZADORGERENCIA, AutorizadorGerencia>()
                .ForMember(d => d.IdGerencia, opt => opt.MapFrom(src => src.IDGERENCIA))
                .ForMember(d => d.IdUsuario, opt => opt.MapFrom(src => src.USUAUSUARIO))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ForMember(d => d.LogsFecha, opt => opt.MapFrom(src => src.LOGSFECHA))
                .ForMember(d => d.IdAutorizadorGerencia, opt => opt.MapFrom(src => src.AUGEAUTORIZADORGERENCIA))
                .ReverseMap();

            CreateMap<POGECONSTANTE, Constante>()
                .ForMember(d => d.IdConstante, opt => opt.MapFrom(src => src.CONSCONSTANTE))
                .ForMember(d => d.Descripcion, opt => opt.MapFrom(src => src.CONSDESCRIPCION))
                .ForMember(d => d.Referencia, opt => opt.MapFrom(src => src.CONSREFERENCIA))
                .ForMember(d => d.Valor, opt => opt.MapFrom(src => src.CONSVALOR))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ForMember(d => d.LogsFecha, opt => opt.MapFrom(src => src.LOGSFECHA))
                .ReverseMap();
        }
    }
}