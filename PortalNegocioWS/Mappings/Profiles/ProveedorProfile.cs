using AutoMapper;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class ProveedorProfile : Profile
    {
        public ProveedorProfile()
        {
            CreateMap<FPROVEEDORESREGISTRADOSMEResult, ProveedoresPorMes>()
                .ForMember(d => d.NumeroMes, opt => opt.MapFrom(src => src.NUMEROMES))
                .ForMember(d => d.Mes, opt => opt.MapFrom(src => src.NOMBREMES))
                .ForMember(d => d.CantidadRegistros, opt => opt.MapFrom(src => src.NROREGISTROS))
                .ReverseMap();
        }
    }
}