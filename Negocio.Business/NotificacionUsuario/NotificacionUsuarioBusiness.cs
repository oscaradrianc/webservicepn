using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Negocio.Data;
using Negocio.Model;

namespace Negocio.Business
{
    public class NotificacionUsuarioBusiness : INotificacionUsuario
    {
        private readonly IUtilidades _utilidades;
        public NotificacionUsuarioBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }

        //////Notificacion usuario
        public List<NotificacionUsuario> ObtenerNotificacionesxUsuario()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_notiusuario = (from p in cx.POGENOTIFICACIONXUSUARIOs
                                       select GetModelObject(p)).ToList();

                return lst_notiusuario;
            }
        }


        public List<NotificacionUsuario> ObtenerUsuarioxIdNotificacion(int idNotificacion)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_notiusuario = (from p in cx.POGENOTIFICACIONXUSUARIOs
                                       where p.NOTINOTIFICACION == idNotificacion
                                       select GetModelObject(p)).ToList();

                return lst_notiusuario;
            }
        }

        public ResponseStatus InsertNotificacionUsuario(NotificacionUsuario notificacionUsuario)
        {
            ResponseStatus result = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var notiUsua = cx.POGENOTIFICACIONXUSUARIOs.Where(n => n.NOTINOTIFICACION == notificacionUsuario.IdNotificacion && n.USUAUSUARIO == notificacionUsuario.IdUsuario).SingleOrDefault();

                        if (notiUsua != null)
                        {
                            dbContextTransaction.Rollback();
                            result.Status = Configuracion.StatusError;
                            result.Message = "Ya existe un registro de notificación x usuarios para el usuario y notificación indicados ingresados.";
                        }
                        else
                        {
                            int codigo = _utilidades.GetSecuencia("SECU_POGENOTIFICACIONXUSUARIO", cx);
                            notificacionUsuario.IdNotificacionUsuario = codigo;
                            cx.POGENOTIFICACIONXUSUARIOs.InsertOnSubmit(GetDBObject(notificacionUsuario));
                            cx.SubmitChanges();
                            dbContextTransaction.Commit();
                            result.Status = Configuracion.StatusOk;
                        }
                    }                    
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        result.Status = Configuracion.StatusError;
                        result.Message = "Error al insertar notificación x usuario";
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        result.Status = Configuracion.StatusError;
                        result.Message = "Error al insertar notificación x usuario";
                    }
                }
            }

            return result;
        }


        public ResponseStatus EliminarNotificacionUsuario(int idNotificacionUsuario, int idUsuario)
        {
            ResponseStatus result = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var notiUsua = cx.POGENOTIFICACIONXUSUARIOs.SingleOrDefault(n => n.NOUSNOTIFICACIONXUSUARIO == idNotificacionUsuario);

                        if(notiUsua != null)
                        {
                            //Primero actualizo el campo logsusuario con el valor del usuario que esta borrando
                            //Para que la auditoria tome en id del usuario que esta realizando la accion.
                            notiUsua.LOGSUSUARIO = idUsuario;
                            cx.SubmitChanges();


                            cx.POGENOTIFICACIONXUSUARIOs.DeleteOnSubmit(notiUsua);
                            cx.SubmitChanges();
                            result.Status = Configuracion.StatusOk;
                        }
                        else
                        {
                            result.Status = Configuracion.StatusError;
                            result.Message = "No existe el registro indicado";
                        }
                                                
                        dbContextTransaction.Commit();
                    }                    
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        result.Status = Configuracion.StatusError;
                        result.Message = "Error al eliminar notificación x usuario";
                    }
                }
            }

            return result;
        }


        #region metodos privados
        /// <summary>
        /// Obtiene el objeto del modelo dado el objeto tabla
        /// </summary>
        /// <param name="obj">Objeto tabla</param>
        /// <returns>Objeto Modelo</returns>
        private NotificacionUsuario GetModelObject(POGENOTIFICACIONXUSUARIO obj)
        {
            NotificacionUsuario n = new NotificacionUsuario();
            n.IdNotificacionUsuario = (int)obj.NOUSNOTIFICACIONXUSUARIO;
            n.IdNotificacion = (int)obj.NOTINOTIFICACION;
            n.IdUsuario = (int)obj.USUAUSUARIO;

            return n;
        }

        /// <summary>
        /// Obtiene el objeto de tabla dado el objeto modelo
        /// </summary>
        /// <param name="obj">Objeto modelo</param>
        /// <returns>Objeto Tabla</returns>
        private POGENOTIFICACIONXUSUARIO GetDBObject(NotificacionUsuario obj)
        {
            POGENOTIFICACIONXUSUARIO n = new POGENOTIFICACIONXUSUARIO();
            n.NOUSNOTIFICACIONXUSUARIO = obj.IdNotificacionUsuario;
            n.NOTINOTIFICACION = obj.IdNotificacion;
            n.USUAUSUARIO = obj.IdUsuario;


            return n;
        }
        #endregion

    }
}
