using Mapster;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class AdministracionRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<POGEAUTORIZADORGERENCIA, AutorizadorGerencia>()
                .Map(dest => dest.IdGerencia, src => src.IDGERENCIA)
                .Map(dest => dest.IdUsuario, src => src.USUAUSUARIO)
                .Map(dest => dest.LogsUsuario, src => src.LOGSUSUARIO)
                .Map(dest => dest.LogsFecha, src => src.LOGSFECHA)
                .Map(dest => dest.IdAutorizadorGerencia, src => src.AUGEAUTORIZADORGERENCIA)
                .TwoWays();

            config.NewConfig<POGECONSTANTE, Constante>()
                .Map(dest => dest.IdConstante, src => src.CONSCONSTANTE)
                .Map(dest => dest.Descripcion, src => src.CONSDESCRIPCION)
                .Map(dest => dest.Referencia, src => src.CONSREFERENCIA)
                .Map(dest => dest.Valor, src => src.CONSVALOR)
                .Map(dest => dest.LogsUsuario, src => src.LOGSUSUARIO)
                .Map(dest => dest.LogsFecha, src => src.LOGSFECHA)
                .TwoWays();
        }
    }
}
