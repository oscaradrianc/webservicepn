using AutoMapper;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class CatalogoProfile : Profile
    {
        public CatalogoProfile()
        {
            CreateMap<PONECATALOGO, Catalogo>()
                .ForMember(d => d.CodigoInterno, opt => opt.MapFrom(src => src.CATACATALOGO))
                .ForMember(d => d.CodigoCatalogo, opt => opt.MapFrom(src => src.CATACODCATALOGO))
                .ForMember(d => d.Nombre, opt => opt.MapFrom(src => src.CATANOMBRE))
                .ForMember(d => d.Estado, opt => opt.MapFrom(src => src.CATAESTADO))
                .ForMember(d => d.UnidadMedida, opt => opt.MapFrom(src => src.CLASUNIDADMEDIDA4))
                .ForMember(d => d.Tipo, opt => opt.MapFrom(src => src.CATATIPO))
                .ForMember(d => d.LogsFecha, opt => opt.MapFrom(src => src.LOGSFECHA))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ReverseMap();

            CreateMap<POGECLASE, Clases>()
                  .ForMember(d => d.IdClase, opt => opt.MapFrom(src => src.CLASCLASE))
                  .ForMember(d => d.NombreClase, opt => opt.MapFrom(src => src.CLASNOMBRE))
                  .ForMember(d => d.Editable, opt => opt.MapFrom(src => src.CLASEDITABLE))
                .ReverseMap();

            CreateMap<POGECLASEVALOR, ClaseValor>()
                .ForMember(d => d.IdClaseValor, opt => opt.MapFrom(src => src.CLVACLASEVALOR))
                .ForMember(d => d.Clase, opt => opt.MapFrom(src => src.CLASCLASE))
                .ForMember(d => d.CodigoValor, opt => opt.MapFrom(src => src.CLVACODIGOVALOR))
                .ForMember(d => d.Descripcion, opt => opt.MapFrom(src => src.CLVADESCRIPCION))
                .ForMember(d => d.Estado, opt => opt.MapFrom(src => src.CLVAESTADO))
                .ForMember(d => d.Valor, opt => opt.MapFrom(src => src.CLVAVALOR))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ReverseMap();
        }
    }
}