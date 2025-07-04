using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IAutorizadorGerencia
    {
        Task<List<POGEAUTORIZADORGERENCIA>> ObtenerAutorizadores();
        Task<List<POGEAUTORIZADORGERENCIA>> ObtenerAutorizadores(int idGerencia);
        Task<ResponseStatus> InsertarAutorizadorGerencia(POGEAUTORIZADORGERENCIA autorizador);
        Task<ResponseStatus> EliminarAutorizadorGerencia(int idAutorizarGerencia, int idUsuarioElimina);
    }
}
