using AutoMapper;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<POGEUSUARIO, Usuario>()
                .ForMember(d => d.IdUsuario, opt => opt.MapFrom(src => src.USUAUSUARIO))
                .ForMember(d => d.Identificador, opt => opt.MapFrom(src => src.USUAIDENTIFICADOR))
                .ForMember(d => d.Nombres, opt => opt.MapFrom(src => src.USUANOMBRE))
                .ForMember(d => d.Identificacion, opt => opt.MapFrom(src => src.USUAIDENTIFICACION))
                .ForMember(d => d.Estado, opt => opt.MapFrom(src => src.USUAESTADO))
                .ForMember(d => d.Clave, opt => opt.MapFrom(src => src.USUACLAVE))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ForMember(d => d.LogsFecha, opt => opt.MapFrom(src => src.LOGSFECHA))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.USUACORREO))
                .ForMember(d => d.Tipo, opt => opt.MapFrom(src => src.USUATIPO))
                .ForMember(d => d.UrlDefecto, opt => opt.MapFrom(src => src.USUAURLDEFECTO))
                .ForMember(d => d.IdRol, opt => opt.MapFrom(src => src.ROLEROL))
                .ForMember(d => d.IdProveedor, opt => opt.MapFrom(src => src.PROVPROVEEDOR))
                .ForMember(d => d.IdArea, opt => opt.MapFrom(src => src.CLASAREA2))
                .ForMember(d => d.VenceClave, opt => opt.MapFrom(src => src.USUAVENCECLAVE))
                .ForMember(d => d.FechaVence, opt => opt.MapFrom(src => src.USUAFECHAVENCE))
                .ForMember(d => d.CambiarClave, opt => opt.MapFrom(src => src.USUACAMBIARCLAVE))
                .ForMember(d => d.ResultadoLogin, opt => opt.Ignore())
                .ForMember(d => d.Token, opt => opt.Ignore())
                .ForMember(d => d.Proveedor, opt => opt.Ignore())
                .ForMember(d => d.Opciones, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<POGEROL, Rol>()
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.ROLEROL))
                .ForMember(d => d.Nombre, opt => opt.MapFrom(src => src.ROLENOMBRE))
                .ForMember(d => d.Estado, opt => opt.MapFrom(src => src.ROLEESTADO))
                .ForMember(d => d.Observacion, opt => opt.MapFrom(src => src.ROLEOBSERVACION))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ForMember(d => d.ListaOpciones, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<POGEOPCIONXROL, OpcionxRol>()
                .ForMember(d => d.IdRolxOpcion, opt => opt.MapFrom(src => src.OPROOPCIONXROL))
                .ForMember(d => d.IdRol, opt => opt.MapFrom(src => src.ROLEROL))
                .ForMember(d => d.IdOpcion, opt => opt.MapFrom(src => src.OPCIOPCION))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ReverseMap();
        }
    }
}