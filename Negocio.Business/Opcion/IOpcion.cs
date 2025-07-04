using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IOpcion
    {
        List<Opcion> GetOpcion();
        List<Opcion> GetOpcion(decimal id);
        Task<ResponseStatus> UpdateOpcion(decimal id, Opcion opcion);
        Task<ResponseStatus> InsertOpcion(Opcion opcion);
        Opcion DeleteOpcion(decimal id);
    }
}
