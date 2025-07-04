using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IConstante
    {
        Task<List<POGECONSTANTE>> GetConstante();
        Task<POGECONSTANTE> GetConstante(int id);
        Task UpdateConstante(int id, POGECONSTANTE constante);
        Task<ResponseStatus> InsertConstante(POGECONSTANTE constante);
        
    }
}
