using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IArchivoExcel
    {
        void RegistrarArchivo(ArchivoExcel request);
        Task<string> ObtenerFormatoProveedor(int idProveedor);
        Task<string> ObtenerFormatoProveedorJson(Proveedor proveedor);
    }
}
