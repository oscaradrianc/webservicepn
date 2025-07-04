using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IUsuario
    {
        Task<List<Usuario>> GetUsuario();
        Usuario GetUsuario(decimal id);
        Task UpdateUsuario(decimal id, POGEUSUARIO usuario);
        Task<ResponseStatus> InsertUsuario(POGEUSUARIO usuario);
        ResponseStatus CrearUsuarioProveedor(POGEUSUARIO usuario, PORTALNEGOCIODataContext cx);
        Usuario DeleteUsuario(decimal id);
        ResponseStatus CambiarClaveUsuario(CambioClave request);
        Task<ResponseStatus> ResetClave(int idUsuario, int diasVenceClave);
    }
}
