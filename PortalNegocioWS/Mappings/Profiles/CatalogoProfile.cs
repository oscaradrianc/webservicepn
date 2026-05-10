using Mapster;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Mappings.Profiles
{
    public class CatalogoRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PONECATALOGO, Catalogo>()
                .Map(dest => dest.CodigoInterno, src => src.CATACATALOGO)
                .Map(dest => dest.CodigoCatalogo, src => src.CATACODCATALOGO)
                .Map(dest => dest.Nombre, src => src.CATANOMBRE)
                .Map(dest => dest.Estado, src => src.CATAESTADO)
                .Map(dest => dest.UnidadMedida, src => src.CLASUNIDADMEDIDA4)
                .Map(dest => dest.Tipo, src => src.CATATIPO)
                .Map(dest => dest.LogsFecha, src => src.LOGSFECHA)
                .Map(dest => dest.LogsUsuario, src => src.LOGSUSUARIO)
                .Ignore(dest => dest.Medida)
                .TwoWays();

            config.NewConfig<POGECLASE, Clases>()
                .Map(dest => dest.IdClase, src => src.CLASCLASE)
                .Map(dest => dest.NombreClase, src => src.CLASNOMBRE)
                .Map(dest => dest.Editable, src => src.CLASEDITABLE)
                .TwoWays();

            config.NewConfig<POGECLASEVALOR, ClaseValor>()
                .Map(dest => dest.IdClaseValor, src => src.CLVACLASEVALOR)
                .Map(dest => dest.Clase, src => src.CLASCLASE)
                .Map(dest => dest.CodigoValor, src => src.CLVACODIGOVALOR)
                .Map(dest => dest.Descripcion, src => src.CLVADESCRIPCION)
                .Map(dest => dest.Estado, src => src.CLVAESTADO)
                .Map(dest => dest.Valor, src => src.CLVAVALOR)
                .Map(dest => dest.LogsUsuario, src => src.LOGSUSUARIO)
                .TwoWays();
        }
    }
}
