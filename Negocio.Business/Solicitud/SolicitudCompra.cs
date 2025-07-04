using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Negocio.Business.Utilidades;
using Negocio.Data;
using Negocio.Model;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class SolicitudBusiness : ISolicitudCompra
    {
        private readonly INotificacion _notificacion;
        private readonly IUtilidades _utilidades;
        private readonly IStorageService _storageService;
        public SolicitudBusiness(INotificacion notificacion, IUtilidades utilidades, IStorageService storageService)
        {
            _notificacion = notificacion;
            _utilidades = utilidades;
            _storageService = storageService;
        }

        #region Metodos Publicos
        /// <summary>
        /// Metodo para crear la solicitud de compra en el sistema
        /// </summary>
        /// <param name="request">Objeto complejo solicitudcompra</param>
        /// <returns>OK si todo esta bien o el mensaje de error</returns>
        public async Task<string> RegistrarSolicitud(SolicitudCompra request)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        //var validacionFile = UtilidadesBusiness.ValidarRutaArchivo()
                        //Solicitud de Compra
                        var tblSolicitud = new PONESOLICITUDCOMPRA();
                        int codigoSolicitud = _utilidades.GetSecuencia("SECU_PONESOLICITUDCOMPRA", cx);
                        request.CodigoSolicitud = codigoSolicitud;
                        tblSolicitud.SOCOSOLICITUD = codigoSolicitud;
                        tblSolicitud.SOCOFECHA = DateTime.Now;
                        tblSolicitud.SOCODESCRIPCION = request.Descripcion;
                        tblSolicitud.CLASAREA9 = request.Area;
                        tblSolicitud.CLASTIPOCONTRATACION3 = request.TipoContratacion;
                        tblSolicitud.SOCOTIPOSOLICITUD = request.TipoSolicitud;                        
                        tblSolicitud.SOCOPRESUPUESTO = request.Presupuesto;
                        tblSolicitud.SOCOVALOR = request.ValorSAIA; //Siempre viene el valor sin importar el tipo de presupuesto

                        if (tblSolicitud.SOCOPRESUPUESTO == Configuracion.TipoPresupuestoSaia) //Si es presupuesto SAIA ingresa estos valores de lo contrario descarta
                        {
                            tblSolicitud.SOCONUMEROSAIA = _utilidades.IsDecimal(request.NumeroSAIA);
                            tblSolicitud.SOCOFECHASAIA = request.FechaSAIA != null ? Convert.ToDateTime(request.FechaSAIA) : (DateTime?)null;
                            tblSolicitud.SOCOPROYECTOSAIA = request.ProyectoSAIA;
                        }
                        
                        tblSolicitud.USUAUSUARIO = request.Usuario;
                        tblSolicitud.SOCOFECHACIERRE = null; //Al insertar no debe ingresar este dato DateTime.Now.AddDays(3); //TODO Realizar logica para dias habiles
                        tblSolicitud.SOCOETAPA = "S";
                        tblSolicitud.SOCOESTADO = GetEstadoSolicitud(request.NumeroSAIA == null ? "GERENCIA" : "COMPRAS").FirstOrDefault().CodigoEstado.ToString();
                        tblSolicitud.LOGSUSUARIO = request.Usuario;
                        tblSolicitud.LOGSFECHA = DateTime.Now;
                        cx.PONESOLICITUDCOMPRAs.InsertOnSubmit(tblSolicitud);
                        cx.SubmitChanges();

                        //almacena los detalles de las solicitudes de compra
                        CargarDetalleSolicitud(request.ArticulosSolicitud, cx, codigoSolicitud, request.Usuario);
                        //alamcena los documentos adjuntos de la solicitud de compra
                        if (request.Terminos != null)
                        {
                            await CargarAnexosSolicitud(request.Terminos, cx, codigoSolicitud, request.Usuario);
                        }

                        if(request.Anexos != null) { 
                            await CargarAnexosSolicitud(request.Anexos, cx, codigoSolicitud, request.Usuario);
                        }

                        dbContextTransaction.Commit();

                        //Enviar notificacion segun sea el caso
                        if(request.NumeroSAIA == null)//Notificacion gerencia
                        {
                            Thread t = new Thread(() =>
                            {
                                _notificacion.GenerarNotificacion("autorizagerencia", request);
                            });

                            t.Start();
                            t.IsBackground = true;
                        }
                        else //Notificacion compras
                        {
                            Thread t = new Thread(() =>
                            {
                                _notificacion.GenerarNotificacion("autorizacompras", request);
                            });

                            t.Start();
                            t.IsBackground = true;
                        }


                        return ("OK");
                    }
                    catch(Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return ex.Message;
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string ActualizarSolicitud(SolicitudCompra request)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        //Solicitud de Compra
                        var tblSolicitud = (from p in cx.PONESOLICITUDCOMPRAs
                                            where p.SOCOSOLICITUD == request.CodigoSolicitud
                                            select p).FirstOrDefault();

 
                        tblSolicitud.SOCODESCRIPCION = request.Descripcion;
                        tblSolicitud.CLASAREA9 = request.Area;
                        tblSolicitud.CLASTIPOCONTRATACION3 = request.TipoContratacion;
                        tblSolicitud.SOCOTIPOSOLICITUD = request.TipoSolicitud;
                        tblSolicitud.SOCOPROYECTOSAIA = request.ProyectoSAIA;
                        tblSolicitud.SOCONUMEROSAIA = _utilidades.IsDecimal(request.NumeroSAIA);
                        tblSolicitud.SOCOFECHASAIA = request.FechaSAIA != null ? Convert.ToDateTime(request.FechaSAIA) : (DateTime?)null;
                        tblSolicitud.SOCOPRESUPUESTO = request.Presupuesto;
                        tblSolicitud.SOCOVALOR = request.ValorSAIA;
                        //tblSolicitud.USUAUSUARIO = request.Usuario;//No actualiza el usuario porque se va a manejar como el usuario que creo la solicitud.
                        tblSolicitud.SOCOFECHACIERRE = request.FechaCierre;
                        tblSolicitud.SOCOETAPA = "S";
                        tblSolicitud.SOCOFECHAPREGUNTA = request.FechaPregunta;
                        tblSolicitud.SOCOFECHARESPUESTA = request.FechaRespuesta;
                        tblSolicitud.SOCOFECHAPUBLICACION = request.FechaPublicacion;
                        tblSolicitud.LOGSUSUARIO = request.Usuario;
                        tblSolicitud.LOGSFECHA = DateTime.Now;
                        cx.SubmitChanges();
                        //Elimina el detalle de la solicitud
                        EliminarDetalleSolicitud(request.ArticulosSolicitud, cx, request.CodigoSolicitud, request.Usuario);
                        //almacena los detalles de las solicitudes de compra
                        CargarDetalleSolicitud(request.ArticulosSolicitud, cx, request.CodigoSolicitud, request.Usuario);
                        //Establece los documentos necesarios para invitacion si los hay
                        InsertaDocumentosInvitacion(request.DocumentosInvitacion, cx, request.CodigoSolicitud, request.Usuario);

                        dbContextTransaction.Commit();
                        return ("OK");
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return ex.Message;
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string ActualizarFechasSolicitud(SolicitudCompra request)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        //Solicitud de Compra
                        var tblSolicitud = (from p in cx.PONESOLICITUDCOMPRAs
                                            where p.SOCOSOLICITUD == request.CodigoSolicitud
                                            select p).FirstOrDefault();

                        tblSolicitud.SOCOFECHACIERRE = request.FechaCierre;
                        tblSolicitud.SOCOFECHAPREGUNTA = request.FechaPregunta;
                        tblSolicitud.SOCOFECHARESPUESTA = request.FechaRespuesta;
                        tblSolicitud.LOGSFECHA = DateTime.Now;
                        tblSolicitud.LOGSUSUARIO = request.Usuario;

                        //alamcena los documentos adjuntos de la solicitud de compra
                        if (request.Terminos != null)
                        {                            
                            ActualizarAnexosSolicitud(request.Terminos, cx, request.CodigoSolicitud, request.Usuario);
                        }

                        if (request.Anexos != null)
                        {
                            ActualizarAnexosSolicitud(request.Anexos, cx, request.CodigoSolicitud, request.Usuario);                            
                        }

                        cx.SubmitChanges();

                        dbContextTransaction.Commit();
                        return ("OK");
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return ex.Message;
                    }
                }
            }
        }

        /// <summary>
        /// Compara 2 byte[]
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns>Retorna true si los 2 parmetros son iguale</returns>
        private static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Saia> ObtenerSaia()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

                var s = (from x in cx.PONEVSAIAs
                         select new Saia { Numero = x.NUMERO, Proyecto = x.PROYECTO, FechaSolicitud = Convert.ToDateTime(x.FECHASOLICITUD), Descripcion = x.DESCRIPCION, ValorEstimado = x.VALORESTIMADO }).ToList();
                return s;
            }
        }

        /// <summary>
        /// Obtiene las solicitudes en estado Pendiente autorizacion gerente por el id de usuario 
        /// </summary>
        /// <param name="estado">Estado de solicitud a consutlar</param>
        /// <param name="idUsuario">Id del usaurio autorizador</param>
        /// <returns></returns>
        public async Task<List<SolicitudCompra>> ListSolicitudPorAutorizador(string estado, int idUsuario)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

                IQueryable<SolicitudCompra> lst_solicitud = (from x in cx.PONESOLICITUDCOMPRAs
                                                             join y in cx.PONEESTADOSOLICITUDs on x.SOCOESTADO equals y.ESSOESTADO
                                                             join ar in cx.PONEVAREAs on x.CLASAREA9 equals ar.CODAREA
                                                             join a in cx.POGEAUTORIZADORGERENCIAs on (decimal)ar.GERENCIA equals a.IDGERENCIA
                                                             where a.USUAUSUARIO == idUsuario && x.SOCOESTADO == estado
                                                             select new SolicitudCompra
                                                             {
                                                                 Area = Convert.ToInt32(x.CLASAREA9),
                                                                 CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                                                                 Descripcion = x.SOCODESCRIPCION,
                                                                 Estado = x.SOCOESTADO,
                                                                 EstadoNombre = y.ESSONOMBRE,
                                                                 TipoAcceso = y.ESSOTIPO,
                                                                 FechaSolicitud = x.SOCOFECHA,
                                                                 FechaCierre = x.SOCOFECHACIERRE,
                                                                 Presupuesto = x.SOCOVALOR.ToString()
                                                             }).OrderBy(x => x.FechaSolicitud);                

                    return await Task.FromResult(lst_solicitud.ToList());                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public List<SolicitudCompra> ListSolicitud(int tipo, FiltroSolicitud filtro = null)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

               IQueryable<SolicitudCompra> lst_solicitud = (from x in cx.PONESOLICITUDCOMPRAs
                                 join y in cx.PONEESTADOSOLICITUDs on x.SOCOESTADO equals y.ESSOESTADO
                                 select new SolicitudCompra
                                 {
                                     Area = Convert.ToInt32(x.CLASAREA9),
                                     CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                                     Descripcion = x.SOCODESCRIPCION,
                                     Estado = x.SOCOESTADO,
                                     EstadoNombre = y.ESSONOMBRE,
                                     TipoAcceso = y.ESSOTIPO,
                                     FechaSolicitud = x.SOCOFECHA,
                                     FechaCierre = x.SOCOFECHACIERRE,
                                     Presupuesto = x.SOCOVALOR.ToString()
                                 }).OrderBy(x => x.FechaSolicitud);

                if (!(tipo == 0))
                {
                    return lst_solicitud.Where(x => x.Estado == tipo.ToString()).ToList();
                } else
                {
                    // Aplica los filtros
                    var predicate = PredicateBuilder.True<SolicitudCompra>();

                    if (filtro.Area != 0)
                    {
                        predicate = predicate.And(i => i.Area == filtro.Area);
                    }

                    if (filtro.CodigoSolicitud != 0)
                    {
                        predicate = predicate.And(i => i.CodigoSolicitud == filtro.CodigoSolicitud);
                    }

                    if (filtro.EstadoSolicitud != 0)
                    {
                        predicate = predicate.And(i => i.Estado == filtro.EstadoSolicitud.ToString());
                    }

                    if ((filtro.FechaInicial != DateTime.MinValue) && (filtro.FechaFinal != DateTime.MinValue))
                    {
                        predicate = predicate.And(i => (i.FechaSolicitud.Date >= filtro.FechaInicial.Date)
                                                       && i.FechaSolicitud.Date <= filtro.FechaFinal.Date);
                    }

                    return lst_solicitud.Where(predicate).ToList();
                }
                
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public List<SolicitudCompra> ListInvitacion(string tipo)
        {   
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

                var s = (from x in cx.PONESOLICITUDCOMPRAs
                         where x.SOCOTIPOSOLICITUD == tipo.ToString() && DateTime.Now.Date <= x.SOCOFECHACIERRE  
                         && x.SOCOESTADO == GetEstadoSolicitud("PUBLICADO").FirstOrDefault().CodigoEstado.ToString()
                         select new SolicitudCompra
                         {
                             Area = Convert.ToInt32(x.CLASAREA9),
                             CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                             Descripcion = x.SOCODESCRIPCION,
                             Estado = x.SOCOESTADO,
                             FechaPublicacion = x.SOCOFECHAPUBLICACION,
                             FechaSolicitud = x.SOCOFECHA,
                             FechaCierre = x.SOCOFECHACIERRE,
                             ArticulosSolicitud = (from p in x.PONEDETALLESOLICITUDs
                                                   join q in cx.PONECATALOGOs on p.CATACATALOGO equals q.CATACATALOGO
                                                   join r in cx.POGECLASEVALORs on q.CLASUNIDADMEDIDA4 equals r.CLVACLASEVALOR
                                                   where p.SOCOSOLICITUD == Convert.ToInt32(x.SOCOSOLICITUD)
                                                   select new DetalleSolicitud
                                                   {
                                                       Cantidad = Convert.ToInt32(p.DESOCANTIDAD),
                                                       Caracteristicas = p.DESOCARACTERISTICAS,
                                                       Catalogo = Convert.ToInt32(p.CATACATALOGO),
                                                       CodigoSolicitud = Convert.ToInt32(p.SOCOSOLICITUD),
                                                       CodigoDetalle = Convert.ToInt32(p.DESODETALLESOLICITUD),
                                                       Medida = r.CLVAVALOR
                                                   }).ToList(),
                         }).ToList();
                return s;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SolicitudCompra GetAdjuntosSolicitud(int id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

                var g = cx.PONESOLICITUDCOMPRAs.Where(x => x.SOCOSOLICITUD == id).Select(x => new SolicitudCompra()
                {

                    Usuario = Convert.ToInt32(x.USUAUSUARIO),
                    Estado = x.SOCOESTADO,
                    TipoContratacion = Convert.ToInt32(x.CLASTIPOCONTRATACION3),
                    Etapa = x.SOCOETAPA,
                    ObservacionAutorizacion = x.SOCOOBSERVACIONESAUTORIZACION,
                    FechaPublicacion = x.SOCOFECHAPUBLICACION,
                    FechaPregunta = x.SOCOFECHAPREGUNTA,
                    FechaRespuesta = x.SOCOFECHARESPUESTA,
                    FechaPropuestas = x.SOCOFECHAPROPUESTAS,
                    FechaCierre = x.SOCOFECHACIERRE,
                    NumeroSAIA =x.SOCONUMEROSAIA.ToString(),
                    ProyectoSAIA = Convert.ToInt32(x.SOCOPROYECTOSAIA),
                    FechaSAIA = x.SOCOFECHASAIA,
                    TipoSolicitud = x.SOCOTIPOSOLICITUD,
                    Presupuesto = x.SOCOPRESUPUESTO,
                    ValorSAIA = Convert.ToInt64(x.SOCOVALOR),
                    ArticulosSolicitud = (from p in x.PONEDETALLESOLICITUDs
                                          join q in cx.PONECATALOGOs on p.CATACATALOGO equals q.CATACATALOGO
                                          join r in cx.POGECLASEVALORs on q.CLASUNIDADMEDIDA4 equals r.CLVACLASEVALOR
                                          select new DetalleSolicitud
                                          {
                                              Cantidad = Convert.ToInt32(p.DESOCANTIDAD),
                                              Caracteristicas = p.DESOCARACTERISTICAS,
                                              Catalogo = Convert.ToInt32(p.CATACATALOGO),
                                              CodigoSolicitud = Convert.ToInt32(p.SOCOSOLICITUD),
                                              CodigoDetalle = Convert.ToInt32(p.DESODETALLESOLICITUD),
                                              Medida = r.CLVAVALOR
                                          }).ToList(),
                    Area = Convert.ToInt32(x.CLASAREA9),
                    Terminos = GetDocumentosSolicitud(x, cx, 332),
                    Anexos = GetDocumentosSolicitud(x, cx, 333),
                    CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                    Descripcion = x.SOCODESCRIPCION,
                    FechaSolicitud = x.SOCOFECHA
                }).FirstOrDefault();

                return g;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SolicitudCompra GetSolicitud(int id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var g = cx.PONESOLICITUDCOMPRAs.Where(x => x.SOCOSOLICITUD == id).Select(x => new SolicitudCompra()
                {
                    Usuario = Convert.ToInt32(x.USUAUSUARIO),
                    Estado = x.SOCOESTADO,
                    TipoContratacion = Convert.ToInt32(x.CLASTIPOCONTRATACION3),
                    Etapa = x.SOCOETAPA,
                    ObservacionAutorizacion = x.SOCOOBSERVACIONESAUTORIZACION,
                    FechaPublicacion = x.SOCOFECHAPUBLICACION,
                    FechaPregunta = x.SOCOFECHAPREGUNTA,
                    FechaRespuesta = x.SOCOFECHARESPUESTA,
                    FechaPropuestas = x.SOCOFECHAPROPUESTAS,
                    FechaCierre = x.SOCOFECHACIERRE,
                    NumeroSAIA = x.SOCONUMEROSAIA.ToString(),
                    ProyectoSAIA = Convert.ToInt32(x.SOCOPROYECTOSAIA),
                    FechaSAIA = x.SOCOFECHASAIA,
                    TipoSolicitud = x.SOCOTIPOSOLICITUD,
                    Presupuesto = x.SOCOPRESUPUESTO,
                    ValorSAIA = Convert.ToInt64(x.SOCOVALOR),                   
                    Area = Convert.ToInt32(x.CLASAREA9),
                    Terminos = GetDocumentosSolicitud(x,cx, 332),
                    Anexos =  GetDocumentosSolicitud(x, cx, 333),
                    CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                    Descripcion = x.SOCODESCRIPCION,
                    FechaSolicitud = x.SOCOFECHA,
                    ArticulosSolicitud = (from p in x.PONEDETALLESOLICITUDs
                                          join q in cx.PONECATALOGOs on p.CATACATALOGO equals q.CATACATALOGO
                                          join r in cx.POGECLASEVALORs on q.CLASUNIDADMEDIDA4 equals r.CLVACLASEVALOR
                                          select new DetalleSolicitud
                                          {
                                              Cantidad = Convert.ToInt32(p.DESOCANTIDAD),
                                              Caracteristicas = p.DESOCARACTERISTICAS,
                                              Id = Convert.ToInt32(p.CATACATALOGO),
                                              CodigoSolicitud = Convert.ToInt32(p.SOCOSOLICITUD),
                                              CodigoDetalle = Convert.ToInt32(p.DESODETALLESOLICITUD),
                                              Medida = r.CLVAVALOR
                                          }).ToList(),
                }).SingleOrDefault();

                return g;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string AutorizarSolicitud(Autorizacion request)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                SolicitudCompra soli = new SolicitudCompra();
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        PONESOLICITUDCOMPRA tblsolicitudCompra = cx.PONESOLICITUDCOMPRAs.Where(x => x.SOCOSOLICITUD == request.CodigoSolicitud).FirstOrDefault();

                        tblsolicitudCompra.LOGSFECHA = DateTime.Now;
                        tblsolicitudCompra.LOGSUSUARIO = request.IdUsuario;

                        if (request.EstadoAutorizacion == Configuracion.EstadoActivo)
                        {
                            soli.CodigoSolicitud = (int)tblsolicitudCompra.SOCOSOLICITUD;
                            soli.Descripcion = tblsolicitudCompra.SOCODESCRIPCION;

                            tblsolicitudCompra.SOCOESTADO = GetEstadoSolicitud(request.EstadoActual).FirstOrDefault().ESSOESTADOSIGUIENTE;
                            tblsolicitudCompra.SOCOOBSERVACIONESAUTORIZACION = request.Observacion;
                        }
                        else
                        {
                            tblsolicitudCompra.SOCOESTADO = GetEstadoSolicitud(request.EstadoActual).FirstOrDefault().ESSOESTADOANTERIOR;
                            tblsolicitudCompra.SOCOOBSERVACIONESRECHAZA = request.Observacion;
                        }

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        if(request.EstadoAutorizacion == Configuracion.EstadoActivo && request.TipoAutorizacion != Configuracion.TipoAutorizacionGerencia)
                        {                            
                            Thread t = new Thread(() =>
                            {
                                string tipoNotificacion = Configuracion.NotificacionAutoCompras;
                                //string tipoNotificacion = request.TipoAutorizacion == Configuracion.TipoAutorizacionCompras ? Configuracion.NotificacionPublicacionInvitacion : Configuracion.NotificacionAutoCompras;
                                _notificacion.GenerarNotificacion(tipoNotificacion, soli);
                            });

                            t.Start();
                            t.IsBackground = true;
                        }
                        return ("OK");
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return (ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene el estado de la solicitud por el alias del estado
        /// </summary>
        /// <param name="alias"></param>
        /// <returns>Estado de la solicitud</returns>
        public List<EstadoSolicitud> GetEstadoSolicitud(string alias)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return (from p in cx.PONEESTADOSOLICITUDs
                        where p.ESSOESTADOREGISTRO == "A" && p.ESSOALIAS == alias
                        select new EstadoSolicitud { 
                            Nombre = p.ESSONOMBRE,
                            Alias = p.ESSOALIAS,
                            CodigoEstado = Convert.ToInt32(p.ESSOESTADO),
                            EstadoAnterior = Convert.ToInt32(p.ESSOESTADOANTERIOR),
                            EstadoRegistro = p.ESSOESTADOREGISTRO,
                            EstadoSiguiente = Convert.ToInt32(p.ESSOESTADOSIGUIENTE)
                        }).ToList();
            }


        }

        public List<SolicitudAreaEstado> GetSolicitudesXEstadoYArea()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var sl = (from s in cx.PONESOLICITUDCOMPRAs
                          join e in cx.PONEESTADOSOLICITUDs on s.SOCOESTADO equals e.ESSOESTADO
                          join a in cx.PONEVAREAs on s.CLASAREA9 equals a.CODAREA  
                          group new { e.ESSOALIAS, a.CODAREA, a.NOMBRE } by new { e.ESSOALIAS, a.CODAREA, a.NOMBRE } into soliGroup
                          select new SolicitudAreaEstado
                            {
                              Cantidad = soliGroup.Count(),
                              Estado = soliGroup.Key.ESSOALIAS,
                              CodArea = soliGroup.Key.CODAREA,
                              Area = soliGroup.Key.NOMBRE
                            }).ToList();

                return sl;                        
            }

        }

        public List<EstadoSolicitud> GetEstadosSolicitud()
        {
            List<EstadoSolicitud> lta = new List<EstadoSolicitud>();
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

                var e = (from es in cx.PONEESTADOSOLICITUDs
                         select es);

                var est = e.ToList();

                foreach(var item in est)
                {
                    lta.Add(new EstadoSolicitud { CodigoEstado = Convert.ToInt32(item.ESSOESTADO), Nombre = item.ESSONOMBRE });

                }
                   
                /*


                         select new EstadoSolicitud
                         {
                             CodigoEstado = Convert.ToInt32(es.ESSOESTADO),
                             Nombre = es.ESSONOMBRE
                         }).ToList();*/

                return lta;            
            }
        }


        /// <summary>
        /// Funcion llamada por el cron cada que se cambia de dia evalue la fecha de cierre
        /// </summary>
        /// <param name="fecha">La funcion se debe ejecutar todos los dias a las 00:00:01 debe venir la fecha de ese dia para comparar las solicitudes publicadas para cerrarlas</param>
        /// <returns></returns>
        public void CerrarInvitacion(DateTime fecha)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var q = from s in cx.PONESOLICITUDCOMPRAs
                        where s.SOCOESTADO == Configuracion.EstadoSolicitudPublicado
                          && Convert.ToDateTime(s.SOCOFECHACIERRE).Date < fecha
                        select s;

                using (PORTALNEGOCIODataContext cx1 = new PORTALNEGOCIODataContext())
                {
                    cx1.Connection.Open();
                    using (var dbContextTransaction = cx1.Connection.BeginTransaction())
                    {

                        foreach (var solicitud in q)
                        {
                            var soli = cx1.PONESOLICITUDCOMPRAs.Where(s => s.SOCOSOLICITUD == solicitud.SOCOSOLICITUD).SingleOrDefault();

                            if (soli != null)
                            {
                                soli.SOCOESTADO = Configuracion.EstadoSolicitudCerrado;
                                soli.USUAUSUARIO = -1;

                                cx1.SubmitChanges();
                                
                            }
                        }

                        dbContextTransaction.Commit();
                    }
                }
            }
        }

        public List<Usuario> ObtenerAutorizadoresArea(int idArea)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var q = from a in cx.POGEAUTORIZADORGERENCIAs
                        join u in cx.POGEUSUARIOs on a.USUAUSUARIO equals u.USUAUSUARIO
                        join ar in cx.PONEVAREAs on a.IDGERENCIA equals ar.GERENCIA
                        where ar.CODAREA == idArea
                        select new Usuario
                        {
                            IdUsuario = (int)u.USUAUSUARIO,
                            Nombres = u.USUANOMBRE,
                            Email = u.USUACORREO
                        };

                return q.ToList();
            }
        }

        public List<ValoresArchivo> ValidarCargaMasiva(SolicitudMasiva request)
        {
            // Leer archivo de excel
            var valores = request.ArchivoB64.LeerArchvoExcel(1);

            // Validar el archivo
            Func<List<ValoresArchivo>, List<ConfiguracionArchivo>, List<ValoresArchivo>> validacionCargaMasiva = ValidacionCargaMasiva;

            //Valida el archivo de excel
            var valoresValidados = ExcelUtilities.ValidarArchivoExcel(validacionCargaMasiva,valores, null);

            // Retornar elementos validados
            return valoresValidados.Any() ? valoresValidados : valores;
        }

        public List<DetalleSolicitud> TransformarArchivo(List<ValoresArchivo> validaciones)
        {
            List<DetalleSolicitud> detallesSolicitud = new List<DetalleSolicitud>();
            
            var codigoCatalogo = validaciones.Where(x=> x.celda.StartsWith("A")).Select(x=> x.valor).ToList();
            var cantidadCatalogo = validaciones.Where(x => x.celda.StartsWith("B")).Select(x => x.valor).ToList();
            var descripcionCatalogo = validaciones.Where(x => x.celda.StartsWith("C")).Select(x => x.valor).ToList();

            detallesSolicitud = codigoCatalogo.Zip(cantidadCatalogo, (x, y )=>
            {
                return new DetalleSolicitud
                {
                    Catalogo = Convert.ToInt32(x),
                    Cantidad = Convert.ToDecimal(y)
                };
            }).Zip(descripcionCatalogo, (x, y) =>
            {
                return new DetalleSolicitud
                {
                    Catalogo = x.Catalogo,
                    Cantidad = x.Cantidad,
                    Caracteristicas = y.ToString()
                };
            }).ToList();

            return detallesSolicitud;
        }

        public async Task<List<CotizacionesPorSolicitud>> GetCotizacionesxSolicitud(int idSolicitud)
        {
            //var solicitudBusiness = new SolicitudBusiness();
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var s = await Task.Run(() =>
                {
                    return (from x in cx.PONESOLICITUDCOMPRAs
                            join j in cx.PONECOTIZACIONs on x.SOCOSOLICITUD equals j.SOCOSOLICITUD
                            join p in cx.PONEPROVEEDORs on j.PROVPROVEEDOR equals p.PROVPROVEEDOR
                            where x.SOCOSOLICITUD == idSolicitud
                            select new CotizacionesPorSolicitud
                            {
                                Area = Convert.ToInt32(x.CLASAREA9),
                                CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                                CodigoCotizacion = Convert.ToInt32(j.COTICOTIZACION),
                                Descripcion = x.SOCODESCRIPCION,
                                Estado = x.SOCOESTADO,
                                //FechaSolicitud = x.SOCOFECHA,                         
                                FechaCierre = x.SOCOFECHACIERRE,
                                FechaCotizacion = j.COTIFECHA,
                                Proveedor = p.PROVRAZONSOCIAL,
                                IdProveedor = Convert.ToInt32(p.PROVPROVEEDOR),
                                Valor = (j.COTIVALOR != null) ? (j.COTIVALOR) : 0
                            }).OrderBy(x => x.CodigoCotizacion).ThenBy(x => x.Proveedor).ToList();
                });

                return s;
            }                
        }
        #endregion

        #region Metodos Privados
        private List<ValoresArchivo>  ValidacionCargaMasiva(List<ValoresArchivo> valores, List<ConfiguracionArchivo> configuracion) { 
                using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
                {
                    List<ValoresArchivo> valoresInvalidos = new List<ValoresArchivo>();
                    const string columnaCodigo = "A";
                    const string columnaCantidad = "B";

                    //1. Valida que los campos Codigo y cantidad esten diligenciados
                    var valoresNulos = valores.Where(x => (x.celda.Contains(columnaCodigo) || x.celda.Contains(columnaCantidad))
                                                            && string.IsNullOrEmpty(x.valor))
                                                .ToList();

                    //2. Valida que la columna cantidad sea numerico
                    var valoresNoNumericos = valores.Where(x => (x.celda.Contains(columnaCantidad))
                                                            && !int.TryParse(x.valor, out _))
                                                .ToList();

                    if (valoresNulos.Any())
                        valoresInvalidos.AddRange(valoresNulos.Select(x=> new ValoresArchivo { 
                            celda = x.celda,
                            hoja = x.hoja,
                            valor = x.valor,
                            error = "El valor es requerido",
                        }));

                    if (valoresNoNumericos.Any())
                        valoresInvalidos.AddRange(valoresNoNumericos.Select(x => new ValoresArchivo
                        {
                            celda = x.celda,
                            hoja = x.hoja,
                            valor = x.valor,
                            error = "El valor debe ser numérico",
                        }));

                    if (valoresInvalidos.Any())
                        return valoresInvalidos.Select(x => new ValoresArchivo
                        {
                            celda = x.celda,
                            valor = x.valor,
                            hoja = x.hoja,
                            error = x.error
                        }).ToList();

                    // Obtiene los codigos de catalogo
                    var codigosCatalogo = cx.PONECATALOGOs.Select(x => x.CATACODCATALOGO.ToString())
                                                          .ToList();

                    // Obtiene los codigos que no existen en el catalogo
                    var codigosNoExistentes = valores.Where(x => x.celda.StartsWith(columnaCodigo) && !codigosCatalogo.Contains(x.valor))
                    .ToList();

                    return codigosNoExistentes.Select(x => new ValoresArchivo
                    {
                        celda = x.celda,
                        valor = x.valor,
                        hoja = x.hoja,
                        error = "No existe el código en el catálogo actual"
                    }).ToList();
                }
        }


        private async Task CargarAnexosSolicitud(List<Documento> documentoSolicitud, PORTALNEGOCIODataContext cx, int codigoSolicitud, int idUsuario)
        {
            foreach (var item in documentoSolicitud)
            {
                // Inserta el blob
                //var tblBlobs = new PONEBLOB();
                //tblBlobs.BLOBDATO = _utilidades.DecodificarArchivo(item.DataB64);
                //int codigoBlob = _utilidades.GetSecuencia("SECU_PONEBLOB", cx);
                //tblBlobs.BLOBBLOB = codigoBlob;
                //cx.PONEBLOBs.InsertOnSubmit(tblBlobs);
                //cx.SubmitChanges();
                var folder = $"invitaciones\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString().PadLeft(2, '0')}\\{codigoSolicitud.ToString()}";

                //byte[] data = _utilidades.DecodificarArchivo(item.DataB64); //System.Convert.FromBase64String(item.DataB64);
                var (contentType, data) = _utilidades.DecodificarArchivoContentType(item.DataB64);
                using var fileStream = new MemoryStream(data);
                var rutaCompleta = await _storageService.SaveFileAsync(folder, item.Nombre, fileStream, contentType);

                // Inserta el documento
                var tblDocumento = new PONEDOCUMENTO();
                int codigoDocumento = _utilidades.GetSecuencia("SECU_PONEDOCUMENTO", cx);
                tblDocumento.DOCUDOCUMENTO = codigoDocumento;
                //tblDocumento.BLOBBLOB = codigoBlob;
                tblDocumento.DOCUNOMBRE = item.Nombre;
                tblDocumento.DOCUFECHACREACION = DateTime.Now;
                tblDocumento.CLASTIPODOCUMENTO8 = item.Tipo;
                tblDocumento.LOGSUSUARIO = idUsuario;
                tblDocumento.DOCURUTA = rutaCompleta;
                tblDocumento.DOCUCONTENTTYPE = contentType;
                cx.PONEDOCUMENTOs.InsertOnSubmit(tblDocumento);
                cx.SubmitChanges();

                //Inserta la relacion
                var tblDocumentoSolicitud = new PONEDOCUMENTOSOLICITUD();
                tblDocumentoSolicitud.DOCUDOCUMENTO = codigoDocumento;
                tblDocumentoSolicitud.DOSOSECUENCIA = _utilidades.GetSecuencia("SECU_PONEDOCUMENTOSOLICITUD", cx);
                tblDocumentoSolicitud.SOCOSOLICITUD = codigoSolicitud;
                tblDocumentoSolicitud.LOGSUSUARIO = idUsuario;
                cx.PONEDOCUMENTOSOLICITUDs.InsertOnSubmit(tblDocumentoSolicitud);
                cx.SubmitChanges();
            }
        }

        private void ActualizarAnexosSolicitud(List<Documento> documentoSolicitud, PORTALNEGOCIODataContext cx, int codigoSolicitud, int idUsuario)
        {
            foreach (var item in documentoSolicitud)
            {
                var docSol = (from d in cx.PONEDOCUMENTOs
                              join ds in cx.PONEDOCUMENTOSOLICITUDs on d.DOCUDOCUMENTO equals ds.DOCUDOCUMENTO
                              where d.CLASTIPODOCUMENTO8 == item.Tipo && ds.SOCOSOLICITUD == codigoSolicitud
                              select d
                              ).SingleOrDefault();

                if(docSol != null)
                {
                    byte[] adjunto = _utilidades.DecodificarArchivo(item.DataB64);

                    //var blob = cx.PONEBLOBs.Where(b => b.BLOBBLOB == docSol.BLOBBLOB).SingleOrDefault();

                    //Si retorna datos, quiere decir que el blob que vienen es diferente al ya cargado, por lo que se asume que se esta actualizando
                    //Por consiguiente se realiza el update
                    /*if(blob != null)
                    {
                        if(!ByteArrayCompare(blob.BLOBDATO, adjunto))
                        {
                            blob.BLOBDATO = adjunto;
                            docSol.LOGSFECHA = DateTime.Now;
                            docSol.LOGSUSUARIO = idUsuario;
                            docSol.DOCUNOMBRE = item.Nombre;
                            cx.SubmitChanges();
                        }
                    }*/
                }
                else
                {
                    //Si entra aca es porque la solicitud no tiene documentos anexos por lo que realiza el insert del anexo segun el tipo
                    // Inserta el blob
                    /*var tblBlobs = new PONEBLOB();
                    tblBlobs.BLOBDATO = _utilidades.DecodificarArchivo(item.DataB64);
                    int codigoBlob = _utilidades.GetSecuencia("SECU_PONEBLOB", cx);
                    tblBlobs.BLOBBLOB = codigoBlob;
                    cx.PONEBLOBs.InsertOnSubmit(tblBlobs);
                    cx.SubmitChanges();
                    */
                    // Inserta el documento
                    var tblDocumento = new PONEDOCUMENTO();
                    int codigoDocumento = _utilidades.GetSecuencia("SECU_PONEDOCUMENTO", cx);
                    tblDocumento.DOCUDOCUMENTO = codigoDocumento;
                    //tblDocumento.BLOBBLOB = codigoBlob;
                    tblDocumento.DOCUNOMBRE = item.Nombre;
                    tblDocumento.DOCUFECHACREACION = DateTime.Now;
                    tblDocumento.CLASTIPODOCUMENTO8 = item.Tipo;
                    tblDocumento.LOGSUSUARIO = idUsuario;
                    cx.PONEDOCUMENTOs.InsertOnSubmit(tblDocumento);
                    cx.SubmitChanges();

                    //Inserta la relacion
                    var tblDocumentoSolicitud = new PONEDOCUMENTOSOLICITUD();
                    tblDocumentoSolicitud.DOCUDOCUMENTO = codigoDocumento;
                    tblDocumentoSolicitud.DOSOSECUENCIA = _utilidades.GetSecuencia("SECU_PONEDOCUMENTOSOLICITUD", cx);
                    tblDocumentoSolicitud.SOCOSOLICITUD = codigoSolicitud;
                    tblDocumentoSolicitud.LOGSUSUARIO = idUsuario;
                    cx.PONEDOCUMENTOSOLICITUDs.InsertOnSubmit(tblDocumentoSolicitud);
                    cx.SubmitChanges();
                }
            }
        }


        private void EliminarDetalleSolicitud(List<DetalleSolicitud> articulosSolicitud, PORTALNEGOCIODataContext cx, int codigoSolicitud, int idUsuario)
        {
            var detalle = cx.PONEDETALLESOLICITUDs.Where(x => x.SOCOSOLICITUD == codigoSolicitud).ToList();

            detalle.ForEach(s => s.LOGSUSUARIO = idUsuario);
            cx.SubmitChanges();            

            foreach (var item in detalle)
            {
                cx.PONEDETALLESOLICITUDs.DeleteOnSubmit(item);
            }
            
            cx.SubmitChanges();
        }

        private void CargarDetalleSolicitud(List<DetalleSolicitud> articulosSolicitud, PORTALNEGOCIODataContext cx, int codigoSolicitud, int idUsuario)
        {
            foreach (var item in articulosSolicitud)
            {
                // Inserta el detalle
                var tblDetalleSolicitud = new PONEDETALLESOLICITUD();
                tblDetalleSolicitud.DESODETALLESOLICITUD = _utilidades.GetSecuencia("SECU_PONEDETALLESOLICITUD", cx);
                tblDetalleSolicitud.CATACATALOGO = item.Id;
                tblDetalleSolicitud.DESOCANTIDAD = Convert.ToDouble(item.Cantidad);
                tblDetalleSolicitud.DESOCARACTERISTICAS = item.Caracteristicas;
                tblDetalleSolicitud.SOCOSOLICITUD = codigoSolicitud;
                tblDetalleSolicitud.LOGSUSUARIO = idUsuario;
                                
                cx.PONEDETALLESOLICITUDs.InsertOnSubmit(tblDetalleSolicitud);
                cx.SubmitChanges();
            }

            cx.SubmitChanges();
        }

        private void InsertaDocumentosInvitacion(List<DocumentoInvitacion> documentosInvitacion, PORTALNEGOCIODataContext cx, int codigoSolicitud, int idUsuario)
        {
            foreach (var item in documentosInvitacion)
            {

                // Inserta los documentos necesarios para la cotizacion
                var tblDocsInvitacion = new PONEDOCSINVITACION();
                tblDocsInvitacion.DOCISECUENCIA  = _utilidades.GetSecuencia("SECU_PONEDOCSINVITACION", cx);
                tblDocsInvitacion.DOCIDOCUMENTO18 = item.CodigoDocumento;
                tblDocsInvitacion.SOCOSOLICITUD = codigoSolicitud;
                tblDocsInvitacion.LOGSUSUARIO = idUsuario;

                cx.PONEDOCSINVITACIONs.InsertOnSubmit(tblDocsInvitacion);
                cx.SubmitChanges();
            }

            cx.SubmitChanges();
        }
        

        private List<PONEESTADOSOLICITUD> GetEstadoSolicitud(int estado)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return (from p in cx.PONEESTADOSOLICITUDs
                        where p.ESSOESTADOREGISTRO == "A" && p.ESSOESTADO == estado.ToString()
                        select p).ToList();
            }                
        }

        private List<Documento> GetDocumentosSolicitud(PONESOLICITUDCOMPRA x, PORTALNEGOCIODataContext cx, int tipoDocumento)
        {
          var t = (from p in x.PONEDOCUMENTOSOLICITUDs
                         join q in cx.PONEDOCUMENTOs on p.DOCUDOCUMENTO equals q.DOCUDOCUMENTO
                         //join r in cx.PONEBLOBs on q.BLOBBLOB equals r.BLOBBLOB
                         where q.CLASTIPODOCUMENTO8 == tipoDocumento
                  select new Documento
                    {
                        FechaCreacion = q.DOCUFECHACREACION,
                        //CodigoBlob = Convert.ToInt32(q.PONEBLOB.BLOBBLOB),
                        CodigoDocumento = Convert.ToInt32(q.DOCUDOCUMENTO),
                        //DataB64 = Convert.ToBase64String(q.PONEBLOB.BLOBDATO),
                        ContentType = q.DOCUCONTENTTYPE,
                        Ruta = q.DOCURUTA,
                      Nombre = q.DOCUNOMBRE,
                        Tipo = Convert.ToInt32(q.CLASTIPODOCUMENTO8)
                    }).ToList();

            return t;

        }

        #endregion

    }
} 
