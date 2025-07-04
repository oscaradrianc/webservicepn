using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface INotificacion
    {
        List<Notificacion> GetNotificacion();
        List<Notificacion> GetNotificacion(decimal id);
        Task<ResponseStatus> UpdateNotificacion(decimal id, Notificacion notificacion);
        int InsertNotificacion(Notificacion notificacion);
        Notificacion DeleteNotificacion(decimal id);
        void GenerarNotificacion(string referencia, object obj);
        //List<NotificacionUsuarioBusiness> ObtenerNotificacionesxUsuario();
        //List<NotificacionUsuarioBusiness> ObtenerUsuarioxIdNotificacion(int idNotificacion);
    }
}
