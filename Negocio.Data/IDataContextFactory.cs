using Negocio.Data;

namespace Negocio.Data
{
    public interface IDataContextFactory
    {
        PORTALNEGOCIODataContext Create();
    }
}
