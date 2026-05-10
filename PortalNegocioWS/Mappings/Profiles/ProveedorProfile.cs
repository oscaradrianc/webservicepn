using Mapster;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class ProveedorRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<FPROVEEDORESREGISTRADOSMEResult, ProveedoresPorMes>()
                .Map(dest => dest.NumeroMes, src => src.NUMEROMES)
                .Map(dest => dest.Mes, src => src.NOMBREMES)
                .Map(dest => dest.CantidadRegistros, src => src.NROREGISTROS)
                .TwoWays();
        }
    }
}
