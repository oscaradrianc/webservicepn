using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IParametroGeneral
    {
        Task<List<POGECLASE>> ObtenerClases();
        Task<POGECLASEVALOR> ObtenerClaseValor(int idClaseValor);
        Task<List<POGECLASEVALOR>> ObtenerClaseValorPorClase(int idClase);
        Task<ResponseStatus> InsertarClaseValor(POGECLASEVALOR claseValor);
        Task<ResponseStatus> ActualizarClaseValor(int id, POGECLASEVALOR claseValor);


    }
}
