using AutoMapper;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
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

            CreateMap<FPROVEEDORESREGISTRADOSMEResult, ProveedoresPorMes>()
                .ForMember(d => d.NumeroMes, opt => opt.MapFrom(src => src.NUMEROMES))
                .ForMember(d => d.Mes, opt => opt.MapFrom(src => src.NOMBREMES))
                .ForMember(d => d.CantidadRegistros, opt => opt.MapFrom(src => src.NROREGISTROS))
                .ReverseMap();

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
                .ReverseMap();

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

            CreateMap<POGEAUTORIZADORGERENCIA, AutorizadorGerencia>()
                .ForMember(d => d.IdGerencia, opt => opt.MapFrom(src => src.IDGERENCIA))
                .ForMember(d => d.IdUsuario, opt => opt.MapFrom(src => src.USUAUSUARIO))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ForMember(d => d.LogsFecha, opt => opt.MapFrom(src => src.LOGSFECHA))
                .ForMember(d => d.IdAutorizadorGerencia, opt => opt.MapFrom(src => src.AUGEAUTORIZADORGERENCIA))
                .ReverseMap();

            CreateMap<POGEROL, Rol>()
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.ROLEROL))
                .ForMember(d => d.Nombre, opt => opt.MapFrom(src => src.ROLENOMBRE))
                .ForMember(d => d.Estado, opt => opt.MapFrom(src => src.ROLEESTADO))
                .ForMember(d => d.Observacion, opt => opt.MapFrom(src => src.ROLEOBSERVACION))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ReverseMap();

            CreateMap<POGEOPCIONXROL, OpcionxRol>()
                .ForMember(d => d.IdRolxOpcion, opt => opt.MapFrom(src => src.OPROOPCIONXROL))
                .ForMember(d => d.IdRol, opt => opt.MapFrom(src => src.ROLEROL))
                .ForMember(d => d.IdOpcion, opt => opt.MapFrom(src => src.OPCIOPCION))
                .ForMember(d => d.LogsUsuario, opt => opt.MapFrom(src => src.LOGSUSUARIO))
                .ReverseMap();

            CreateMap<PONENOTICIA, Noticias>()
                .ForMember(d => d.CodigoNoticia, opt => opt.MapFrom(src => src.NOTINOTICIA))
                .ForMember(d => d.Titulo, opt => opt.MapFrom(src => src.NOTITITULO))
                .ForMember(d => d.Fecha, opt => opt.MapFrom(src => src.NOTIFECHA))
                .ForMember(d => d.Contenido, opt => opt.MapFrom(src => src.NOTICONTENIDO))
                .ForMember(d => d.Estado, opt => opt.MapFrom(src => src.NOTIESTADO))              
                .ForMember(d => d.URL, opt => opt.MapFrom(src => src.NOTIURL))
                .ForMember(d => d.FotoB64, opt => opt.MapFrom(src => src.PONEBLOB))
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
