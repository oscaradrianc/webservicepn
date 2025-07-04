using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface ICatalogo
    {
        Task<List<Catalogo>> GetCatalogo();
        Task<List<Catalogo>> GetCatalogoConMedida();
        List<Catalogo> GetCatalogo(decimal id);
        Task<ResponseStatus> UpdateCatalogo(decimal id, PONECATALOGO catalogo);
        Task<ResponseStatus> InsertCatalogo(PONECATALOGO catalogo);
        Catalogo DeleteCatalogo(decimal id);
    }
}
