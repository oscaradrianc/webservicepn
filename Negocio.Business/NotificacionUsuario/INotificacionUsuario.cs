using System;
using System.Collections.Generic;
using System.Text;
using Negocio.Model;

namespace Negocio.Business
{
    public interface INotificacionUsuario
    {
        List<NotificacionUsuario> ObtenerNotificacionesxUsuario();
        List<NotificacionUsuario> ObtenerUsuarioxIdNotificacion(int idNotificacion);
        ResponseStatus InsertNotificacionUsuario(NotificacionUsuario notificacionUsuario);
        ResponseStatus EliminarNotificacionUsuario(int idNotificacionUsuario, int idUsuario);


    }
}