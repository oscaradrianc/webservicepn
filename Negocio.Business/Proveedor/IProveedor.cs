using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IProveedor
    {
        Task<ResponseStatus> RegistrarProveedor(Proveedor request);
        Task ActualizarProveedor(Proveedor request);
        Task<List<Proveedor>> ObtenerProveedorXEstado(string estado);
        Task<string> AutorizarProveedor(ActualizarEstadoProveedor estadoProveedor);
        string CambiarEstadoProveedor(ActualizarEstadoProveedor estadoProveedor);        
        List<ProveedorDatosBasicos> ConsultarDatosBasicosProveedor();
        Proveedor ObtenerProveedor(int idProveedor);
        Task<ProveedorFormato> ObtenerProveedorFormato(int idProveedor);
        string ActualizarDocsProveedor(Proveedor proveedor);
        Task<List<ProveedorEstado>> ObtenerCantidadProveedorPorEstado();
        Task<List<FPROVEEDORESREGISTRADOSMEResult>> ObtenerNroProveedoresRegistradoPorMes(int vigencia);
        Task<ProveedorFormato> ObtenerProveedorFormatoJson(Proveedor proveedor);
    }
}
