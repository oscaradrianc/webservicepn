using Negocio.Data;
using Negocio.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IRol
    {
        Task<List<POGEROL>> GetRol();
        List<Rol> GetRol(decimal id);
        Task<List<POGEROL>> GetRoles();
        void UpdateRol(decimal id, Rol rol);
        Task<ResponseStatus> InsertRol(POGEROL rol);
        Rol DeleteRol(decimal id);

        Task<List<POGEOPCIONXROL>> GetOpcionRol();
        Task<List<POGEOPCIONXROL>> GetOpcionRol(int idRol);

        Task<ResponseStatus> InsertOpcionRol(POGEOPCIONXROL opcionRol);
        Task<ResponseStatus> DeleteOpcionRol(decimal id);
    }
}
