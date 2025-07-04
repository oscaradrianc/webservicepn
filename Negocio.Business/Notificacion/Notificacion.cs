using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Negocio.Model;
using Negocio.Data;
using Stubble.Core.Builders;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class NotificacionBusiness : INotificacion
    {
        private readonly IUtilidades _utilidades;
        public NotificacionBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }
        #region Metodos Publicos

        /// <summary>
        /// Obtiene Todos los Elementos de la notificacion
        /// </summary>
        /// <returns>Lista de elementos de la notificacion</returns>
        public List<Notificacion> GetNotificacion()
        {

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_notificacion = (from p in cx.POGENOTIFICACIONs
                                    select GetModelObject(p)).ToList();

                return lst_notificacion;
            }

        }

        /// <summary>
        /// Obtiene Todos los Elementos de la notificacion dado el Id
        /// </summary>
        /// <param name="id">Codigo de la notificacion</param>
        /// <returns>elemento de notificacion</returns>
        public List<Notificacion> GetNotificacion(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_notificacion = (from p in cx.POGENOTIFICACIONs
                                    where p.NOTINOTIFICACION == id
                                    select GetModelObject(p)).ToList();

                return lst_notificacion;
            }
        }

        /// <summary>
        /// Actualiza el elemento de la notificacion
        /// </summary>
        /// <param name="id">Codigo de la notificacion</param>
        /// <param name="notificacion">Objeto Notificacion</param>
        public async Task<ResponseStatus> UpdateNotificacion(decimal id, Notificacion notificacion)
        {
            ResponseStatus res = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var query = cx.POGENOTIFICACIONs.Where(x => x.NOTINOTIFICACION == id).FirstOrDefault();

                        if (query == null)
                        {
                            //throw new KeyNotFoundException("No se encontro el dato");
                            res.Status = Configuracion.StatusError;
                            res.Message = "No se encontro el dato";
                        }
                        else
                        {
                            //query = GetDBObject(notificacion);
                            query.NOTIASUNTO = notificacion.Asunto;
                            query.NOTIESTADO = notificacion.Estado;
                            query.NOTINOMBRE = notificacion.Nombre;
                            query.NOTIPLANTILLA = notificacion.Plantilla;
                            query.NOTITIPO = notificacion.Tipo;
                            query.LOGSUSUARIO = notificacion.LogsUsuario;
                        }

                        cx.SubmitChanges();
                        await Task.Run(() => dbContextTransaction.Commit());
                        res.Status = Configuracion.StatusOk;
                    }
                    catch (KeyNotFoundException dx)
                    {
                        res.Status = Configuracion.StatusError;
                        res.Message = "Error al actualizar notificacion " + dx.Message;
                        await Task.Run(() => dbContextTransaction.Rollback());                        
                    }
                    catch (Exception ex)
                    {
                        res.Status = Configuracion.StatusError;
                        res.Message = "Error al actualizar notificacion " + ex.Message;
                        await Task.Run(() => dbContextTransaction.Rollback());                        
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Inserta el elemento de la notificacion
        /// </summary>
        /// <param name="notificacion">Objeto Notificacion</param>
        /// <returns>Codigo del elemento de la notificacion </returns>
        public int InsertNotificacion(Notificacion notificacion)
        {
            int codigo = 0;

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        codigo = _utilidades.GetSecuencia("SECU_POGENOTIFICACION", cx);
                        notificacion.IdNotificacion = codigo;
                        cx.POGENOTIFICACIONs.InsertOnSubmit(GetDBObject(notificacion));
                        cx.SubmitChanges();
                        dbContextTransaction.Commit();
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al insertar notificacion " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al insertar catalogo " + ex.Message);
                    }

                }
            }

            return codigo;
        }

        /// <summary>
        /// Elimina el elemento de la notificacion
        /// </summary>
        /// <param name="id">Codigo del elemento de la notificacion</param>
        /// <returns>Elemento de la notificacion recientemente eliminado</returns>
        public Notificacion DeleteNotificacion(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        Notificacion notificacion = new Notificacion();
                        var query = cx.POGENOTIFICACIONs.Where(x => x.NOTINOTIFICACION == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException("No se encontro el dato");
                        }
                        else
                        {
                            cx.POGENOTIFICACIONs.DeleteOnSubmit(query);
                        }

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        return GetModelObject(query);
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al eliminar notificacion " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al eliminar notificacion " + ex.Message);
                    }

                }
            }
        }

        /// <summary>
        /// Genera las notificaciones por correo electronico
        /// </summary>
        /// <param name="referencia"></param>
        /// <param name="obj"></param>
        public void GenerarNotificacion(string referencia, object obj)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

                Notificacion noti = ObtenerNotificacion(referencia, cx);
                string mensaje = string.Empty;
                string parametros = string.Empty;
                List<string> correos = new List<string>();
                string correoProv = string.Empty;

                if (noti != null)
                {
                    var stubble = new StubbleBuilder().Build();

                    switch (referencia)
                    {
                        case "nuevousuario":

                            Usuario user = obj as Usuario;

                            correos.Add(user.Email);
                            mensaje = stubble.Render(noti.Plantilla, user);
                            _utilidades.SendMail(correos, noti.Asunto, mensaje);

                            break;
                        case "resetpassword":

                            Usuario userReset = obj as Usuario;

                            correos.Add(userReset.Email);
                            mensaje = stubble.Render(noti.Plantilla, userReset);
                            _utilidades.SendMail(correos, noti.Asunto, mensaje);

                            break;
                        case "registroproveedor":

                            foreach (Usuario usua in noti.Usuarios)
                            {
                                correos.Add(usua.Email);
                            }

                            if (correos.Count > 0)
                            {
                                Proveedor prov = obj as Proveedor;

                                mensaje = stubble.Render(noti.Plantilla, prov);
                                _utilidades.SendMail(correos, noti.Asunto, mensaje);
                            }
                            break;
                        case "confregistroprov":

                            Proveedor provConf = obj as Proveedor;

                            if(provConf.Email != null)
                            {
                                correos.Add(provConf.Email);

                                mensaje = stubble.Render(noti.Plantilla, provConf);
                                _utilidades.SendMail(correos, noti.Asunto, mensaje);
                            }

                            break;
                        case Configuracion.NotificacionAutoGerencia: // "autorizagerencia":
                            SolicitudCompra sC = obj as SolicitudCompra;
                            mensaje = stubble.Render(noti.Plantilla, sC);

                            List<Usuario> auto = ObtenerAutorizadorGerenciaxArea(sC.Area, cx);

                            foreach (Usuario usua in auto)
                            {
                                correos.Add(usua.Email);
                            }

                            if (correos.Count > 0)
                            {
                                _utilidades.SendMail(correos, noti.Asunto, mensaje);
                            }

                            break;
                        case Configuracion.NotificacionAutoCompras: // "autorizacompras":

                            foreach (Usuario usua in noti.Usuarios)
                            {
                                correos.Add(usua.Email);
                            }

                            if (correos.Count > 0)
                            {
                                SolicitudCompra sCG = obj as SolicitudCompra;
                                mensaje = stubble.Render(noti.Plantilla, sCG);

                                _utilidades.SendMail(correos, noti.Asunto, mensaje);
                            }

                            break;
                        case "autorizacionproveedo":

                            Usuario provAuto = obj as Usuario;
                            correoProv = ProveedorBusiness.ObtenerEmailxProveedor((int)provAuto.IdProveedor);

                            if (correoProv != null)
                            {
                                mensaje = stubble.Render(noti.Plantilla, provAuto);
                                correos.Add(correoProv);
                                _utilidades.SendMail(correos, noti.Asunto, mensaje);
                            }
                            break;
                        case "registropregunta":

                            foreach (Usuario usua in noti.Usuarios)
                            {
                                correos.Add(usua.Email);
                            }

                            if (correos.Count > 0)
                            {
                                CrearPregunta preg = obj as CrearPregunta;
                                mensaje = stubble.Render(noti.Plantilla, preg);

                                _utilidades.SendMail(correos, noti.Asunto, mensaje);
                            }

                            break;

                        case "registrorespuesta":

                            CrearRespuesta respuesta = obj as CrearRespuesta;
                            correoProv = ProveedorBusiness.ObtenerEmailxProveedor(respuesta.CodigoProveedor);

                            if (!string.IsNullOrEmpty(correoProv))
                            {
                                mensaje = stubble.Render(noti.Plantilla, respuesta);
                                correos.Add(correoProv);
                                _utilidades.SendMail(correos, noti.Asunto, mensaje);
                            }
                            break;
                        case "confirmacioncotizaci":

                            Cotizacion cotiProv = obj as Cotizacion;
                            EnviarNotificacionProveedor(cotiProv.CodigoProveedor, noti, cotiProv, cx);

                            break;
                        case "registrocotizacion":

                            Cotizacion coti = obj as Cotizacion;
                            coti.NombreProveedor = cx.FOBTENERRAZONSOCIALPROVEEDOR(coti.CodigoProveedor);
                            EnviarNotificacionInterno(noti, coti, cx);

                            break;
                        case Configuracion.NotificacionPublicacionInvitacion: //Envia correo a todos los proveedores registrados cuando se publica invitacion

                            correos = _utilidades.ObtenerCorreosProveedor();

                            if (correos.Count > 0)
                            {
                                SolicitudCompra sPub = obj as SolicitudCompra;
                                mensaje = stubble.Render(noti.Plantilla, sPub);

                                _utilidades.SendMail(correos, noti.Asunto, mensaje, true);
                            }

                            break;
                        case Configuracion.NotificacionAjudicado: //Envia correo al adjudicar

                            NotificacionAdjudicacion notiAdj = obj as NotificacionAdjudicacion;
                            correos = _utilidades.ObtenerCorreoProveedoresSolicitud(notiAdj.CodigoSolicitud);
                            
                            if(correos.Count() > 0)
                            {
                                mensaje = stubble.Render(noti.Plantilla, notiAdj);

                                _utilidades.SendMail(correos, noti.Asunto, mensaje, true);
                            }

                            break;
                        case Configuracion.NotificacionDesierto: //Envia correo al adjudicar

                            NotificacionAdjudicacion notiDesierto = obj as NotificacionAdjudicacion;
                            correos = _utilidades.ObtenerCorreoProveedoresSolicitud(notiDesierto.CodigoSolicitud);

                            if (correos.Count() > 0)
                            {
                                mensaje = stubble.Render(noti.Plantilla, notiDesierto);

                                _utilidades.SendMail(correos, noti.Asunto, mensaje, true);
                            }

                            break;
                        case Configuracion.NotificacionActualizacionDatos: //Envia correo a todos los proveedores registrados cuando se publica invitacion

                            correos = _utilidades.ObtenerCorreosProveedor();

                            if (correos.Count > 0)
                            {
                                //SolicitudCompra sPub = obj as SolicitudCompra;
                                mensaje = noti.Plantilla; //stubble.Render(noti.Plantilla, sPub);

                                _utilidades.SendMail(correos, noti.Asunto, mensaje, true);
                            }

                            break; ;
                    }
                }
            }           
        }
       

        #endregion

        #region Metodos Privados

        /// <summary>
        /// Envia el mensaje cuando es para personal interno de la empresa de empresa
        /// Segun la notificacion obtiene los usuarios parametrizados para la notificacion de referencia
        /// </summary>
        /// <param name="noti">Objecto notificacion con los usuarios x notificacion</param>        
        /// <param name="obj">Objcto para reemplazar en la platilla</param>
        /// <param name="cx">Conexion a base de datos.</param>
        private void EnviarNotificacionInterno(Notificacion noti, object obj, PORTALNEGOCIODataContext cx)
        {
            List<string> correos = new List<string>();
            var stubble = new StubbleBuilder().Build();

            foreach (Usuario usua in noti.Usuarios)
            {
                correos.Add(usua.Email);
            }

            if (correos.Count > 0)
            {
                string mensaje = stubble.Render(noti.Plantilla, obj);

                _utilidades.SendMail(correos, noti.Asunto, mensaje);
            }
        }

        /// <summary>
        /// Envia notificacion a un proveedor determinado recibe objeto para reemplazar en plantilla
        /// </summary>
        /// <param name="codigoProveedor"></param>
        /// <param name="noti"></param>
        /// <param name="obj"></param>
        /// <param name="cx"></param>
        private void EnviarNotificacionProveedor(int codigoProveedor, Notificacion noti, object obj, PORTALNEGOCIODataContext cx)
        {
            var stubble = new StubbleBuilder().Build();
            List<string> correos = new List<string> { ProveedorBusiness.ObtenerEmailxProveedor(codigoProveedor) };

            if (correos.Count > 0)
            {
                string mensaje = stubble.Render(noti.Plantilla, obj);                
                _utilidades.SendMail(correos, noti.Asunto, mensaje);
            }
        }

        /// <summary>
        /// Obtiene el objeto del modelo dado el objeto tabla
        /// </summary>
        /// <param name="obj">Objeto tabla</param>
        /// <returns>Objeto Modelo</returns>
        private Notificacion GetModelObject(POGENOTIFICACION obj)
        {
            Notificacion n = new Notificacion();
            n.IdNotificacion = (int)obj.NOTINOTIFICACION;
            n.Asunto = obj.NOTIASUNTO;
            n.Estado = obj.NOTIESTADO;
            n.Nombre = obj.NOTINOMBRE;
            n.Plantilla = obj.NOTIPLANTILLA;
            n.Tipo = obj.NOTITIPO;            

            return n;
        }

        /// <summary>
        /// Obtiene el objeto de tabla dado el objeto modelo
        /// </summary>
        /// <param name="obj">Objeto modelo</param>
        /// <returns>Objeto Tabla</returns>
        private POGENOTIFICACION GetDBObject(Notificacion obj)
        {
            POGENOTIFICACION n = new POGENOTIFICACION();
            n.NOTINOTIFICACION = obj.IdNotificacion;
            n.NOTIASUNTO = obj.Asunto;
            n.NOTIESTADO = obj.Estado;
            n.NOTINOMBRE = obj.Nombre;
            n.NOTIPLANTILLA = obj.Plantilla;
            n.NOTITIPO = obj.Tipo;

            return n;
        }


        /// <summary>
        /// Obtiene datos de autorizador de gerencia de solicitudes 
        /// </summary>
        /// <param name="codigoArea"></param>
        /// <returns></returns>
        private List<Usuario> ObtenerAutorizadorGerenciaxArea(int codigoArea, PORTALNEGOCIODataContext cx)
        {

            List<Usuario> usuaAuto = (from a in cx.FAUTORIZADORGERENCIAXAREA(codigoArea)
                                      select new Usuario { IdUsuario = Convert.ToInt32(a.USUAUSUARIO), Nombres = a.USUANOMBRE, Email = a.USUACORREO }).ToList();

            return usuaAuto;

        }

        /// <summary>
        /// Obtiene la notificacion de la BD por la referencia
        /// </summary>
        /// <param name="referencia"></param>
        /// <param name="cx"></param>
        /// <returns></returns>
        private Notificacion ObtenerNotificacion(string referencia, PORTALNEGOCIODataContext cx)
        {

            Notificacion noti = new Notificacion();

            noti = (from n in cx.POGENOTIFICACIONs
                    where n.NOTINOMBRE.Equals(referencia)
                    select new Notificacion
                    {
                        IdNotificacion = Convert.ToInt32(n.NOTINOTIFICACION),
                        Asunto = n.NOTIASUNTO,
                        Nombre = n.NOTINOMBRE,
                        Estado = n.NOTIESTADO,
                        Plantilla = Convert.ToString(n.NOTIPLANTILLA),
                        Tipo = n.NOTITIPO
                    }).SingleOrDefault();

            //Si hay notificacion se consulta los usuarios para la notificacio
            if (noti != null)
            {
                noti.Usuarios = (from n in cx.POGENOTIFICACIONXUSUARIOs
                                 join u in cx.POGEUSUARIOs on n.USUAUSUARIO equals u.USUAUSUARIO
                                 where n.NOTINOTIFICACION == noti.IdNotificacion
                                 select new Usuario
                                 {
                                     IdUsuario = Convert.ToInt32(u.USUAUSUARIO),
                                     Nombres = u.USUANOMBRE,
                                     Email = u.USUACORREO
                                 }).ToList();
            }

            return noti;
        }

        #endregion


    }
}
