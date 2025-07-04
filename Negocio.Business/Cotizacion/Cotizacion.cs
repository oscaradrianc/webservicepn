
using Negocio.Business.Utilidades;
using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class CotizacionBusiness : ICotizacion
    {
        private readonly INotificacion _notificacion;
        private readonly ISolicitudCompra _solicitudCompra;
        private readonly IUtilidades _utilidades;
        private readonly IStorageService _storageService;

        public CotizacionBusiness(INotificacion notificacion, ISolicitudCompra solicitudCompra, IUtilidades utilidades, IStorageService storageService)
        {
            _notificacion = notificacion;
            _solicitudCompra = solicitudCompra;
            _utilidades = utilidades;
            _storageService = storageService;
        }

        #region Metodos Publicos
        /// <summary>
        /// Metodo para crear la cotizacion en el sistema
        /// </summary>
        /// <param name="request">Objeto complejo cotizacion</param>
        /// <returns>OK si todo esta bien o el mensaje de error</returns>
        public async Task<ResponseStatus> RegistrarCotizacion(Cotizacion request)
        {            
            ResponseStatus resp = new ResponseStatus();

            using var cx = new PORTALNEGOCIODataContext();
            
            cx.Connection.Open();
            using var dbContextTransaction = cx.Connection.BeginTransaction();

            //Valida si el proveedor ya ha realizado una cotizacion para esta solicitud
            var cantCotizacion = (from p in cx.PONECOTIZACIONs
                                      where p.PROVPROVEEDOR == request.CodigoProveedor
                                      && p.SOCOSOLICITUD == request.CodigoSolicitud
                                      select p).Count();

                if (cantCotizacion == 0)
                {
                    //Cotizacion
                    var tblCotizacion = new PONECOTIZACION();
                    int codigoCotizacion = _utilidades.GetSecuencia("SECU_PONECOTIZACION", cx);
                    tblCotizacion.COTICOTIZACION = codigoCotizacion;
                    tblCotizacion.COTIFECHA = DateTime.Now;
                    tblCotizacion.COTIOBSERVACION = request.Observacion;
                    tblCotizacion.COTIVALOR = Convert.ToInt64(request.ValorCotizacion);
                    tblCotizacion.SOCOSOLICITUD = request.CodigoSolicitud;
                    tblCotizacion.PROVPROVEEDOR = request.CodigoProveedor;
                    tblCotizacion.LOGSUSUARIO = request.CodigoUsuario;
                    tblCotizacion.COTIFORMAPAGO17 = request.FormaPago;
                    tblCotizacion.COTIFECHAENTREGA = request.FechaEntrega;
                    request.CodigoCotizacion = codigoCotizacion;

                    cx.PONECOTIZACIONs.InsertOnSubmit(tblCotizacion);
                    cx.SubmitChanges();

                    if (request.DocumentoCotizacion != null)
                    {
                        await CargarAnexosCotizacion(request.DocumentoCotizacion, cx, codigoCotizacion, request.CodigoSolicitud, request.CodigoUsuario);
                    }

                    if (request.DocumentoAdicional != null)
                    {
                        await CargarAnexosCotizacion(request.DocumentoAdicional, cx, codigoCotizacion, request.CodigoSolicitud, request.CodigoUsuario);
                    }

                    //almacena los detalles de la cotizacion
                    CargarDetalleCotizacion(request.ElementosCotizacion, cx, codigoCotizacion, request.CodigoUsuario);

                    //////////////////Envia Correo indicando a las personas parametrizadas que el proveedor registro una cotizacion//////////////////////////
                    Thread t = new Thread(() =>
                        _notificacion.GenerarNotificacion("registrocotizacion", request)
                    ) ;
                    t.Start();
                    t.IsBackground = true;

                    Thread t1 = new Thread(() =>
                        _notificacion.GenerarNotificacion("confirmacioncotizaci", request)
                    );
                    t1.Start();
                    t1.IsBackground = true;
                    //////////////////////////////////////////////////////////////////////////////////////////

                    dbContextTransaction.Commit();

                    resp.Status = Configuracion.StatusOk;
                    return (resp);
                }
                else
                {
                    resp.Status = Configuracion.StatusError;
                    resp.Message = "El proveedor ya registro una cotización para esta invitación";
                    return resp;
                }           
        }

        /// <summary>
        /// Lista las solicitudes por cotizacion
        /// </summary>
        /// <returns></returns>
        //public List<SolicitudCotizacion> ListarSolicitudesCotizacion()
        public List<CotizacionesPorSolicitud> ListarSolicitudesCotizacion()        
        {
            //var solicitudBusiness = new SolicitudBusiness();
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var s = (from x in cx.PONESOLICITUDCOMPRAs
                     join j in cx.PONECOTIZACIONs on x.SOCOSOLICITUD equals j.SOCOSOLICITUD into solicitud
                     from j in solicitud.DefaultIfEmpty()
                     join p in cx.PONEPROVEEDORs on  (j.PROVPROVEEDOR == 0 ? 0 : j.PROVPROVEEDOR) equals p.PROVPROVEEDOR into proveedor
                     from p in proveedor.DefaultIfEmpty()                     
                     where x.SOCOESTADO == _solicitudCompra.GetEstadoSolicitud("PUBLICADO").FirstOrDefault().CodigoEstado.ToString() ||
                           x.SOCOESTADO == _solicitudCompra.GetEstadoSolicitud("CERRADO").FirstOrDefault().CodigoEstado.ToString()

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
                         ArticulosSolicitud = (from p in x.PONEDETALLESOLICITUDs
                                               where p.SOCOSOLICITUD == Convert.ToInt32(x.SOCOSOLICITUD)
                                               select new DetalleSolicitud
                                               {
                                                   Cantidad = Convert.ToInt32(p.DESOCANTIDAD),
                                                   Caracteristicas = p.DESOCARACTERISTICAS,
                                                   Catalogo = Convert.ToInt32(p.CATACATALOGO),
                                                   CodigoSolicitud = Convert.ToInt32(p.SOCOSOLICITUD),
                                                   CodigoDetalle = Convert.ToInt32(p.DESODETALLESOLICITUD)
                                               }).ToList(),
                     }).OrderBy(x => x.CodigoSolicitud).ThenBy(x => x.CodigoCotizacion).ToList();
            return s;
        }

        
        /// <summary>
        /// Lista las solicitudes por cotizacion por proveedor
        /// </summary>
        /// <returns></returns>
        public List<SolicitudCotizacion> ListCotizacionProveedor(int idProveedor)
        {
            //var solicitudBusiness = new SolicitudBusiness();
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var s = (from x in cx.PONESOLICITUDCOMPRAs
                     join j in cx.PONECOTIZACIONs
                     on x.SOCOSOLICITUD equals j.SOCOSOLICITUD
                     where j.PROVPROVEEDOR == idProveedor
                     select new SolicitudCotizacion
                     {
                         Area = Convert.ToInt32(x.CLASAREA9),
                         CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                         CodigoCotizacion = Convert.ToInt32(j.COTICOTIZACION),
                         Descripcion = x.SOCODESCRIPCION,
                         Estado = x.SOCOESTADO,
                         FechaSolicitud = x.SOCOFECHA,
                         FechaCierre = x.SOCOFECHACIERRE,
                         ValorCotizacion = (long)j.COTIVALOR,
                         ArticulosSolicitud = (from p in x.PONEDETALLESOLICITUDs
                                               where p.SOCOSOLICITUD == Convert.ToInt32(x.SOCOSOLICITUD)
                                               select new DetalleSolicitud
                                               {
                                                   Cantidad = Convert.ToInt32(p.DESOCANTIDAD),
                                                   Caracteristicas = p.DESOCARACTERISTICAS,
                                                   Catalogo = Convert.ToInt32(p.CATACATALOGO),
                                                   CodigoSolicitud = Convert.ToInt32(p.SOCOSOLICITUD),
                                                   CodigoDetalle = Convert.ToInt32(p.DESODETALLESOLICITUD)
                                               }).ToList(),
                     }).Distinct().ToList();
            return s;
        }


        /// <summary>
        /// Retorna un resumen de las cotizaciones por estado de un proveedor
        /// </summary>
        /// <returns></returns>
        public List<CotizacionesEstado> ListCotizacionProveedorEstado(int idProveedor)
        {
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var s = (from x in cx.PONESOLICITUDCOMPRAs
                     join j in cx.PONECOTIZACIONs on x.SOCOSOLICITUD equals j.SOCOSOLICITUD
                     join e in cx.PONEESTADOSOLICITUDs on x.SOCOESTADO equals e.ESSOESTADO
                     where j.PROVPROVEEDOR == idProveedor
                     group x by  e.ESSOALIAS into soliGroup                     
                     select new CotizacionesEstado
                     {
                         Estado = soliGroup.Key,
                         Cantidad = soliGroup.Count(),

                     }).ToList();

            return s;
        }


        /// <summary>
        /// Lista todas las cotizaciones Ofertadas
        /// </summary>
        /// <param name="codigoSolicitud"></param>
        /// <returns></returns>
        public List<CotizacionProveedor> ListarCotizacionesOfertadas(int codigoSolicitud, int estado)
        {
            //var solicitudBusiness = new SolicitudBusiness();
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var query = (from s in cx.PONESOLICITUDCOMPRAs
                         join es in cx.PONEESTADOSOLICITUDs
                            on s.SOCOESTADO equals es.ESSOESTADO
                         join ds in cx.PONEDETALLESOLICITUDs
                            on s.SOCOSOLICITUD equals ds.SOCOSOLICITUD
                         join c in cx.PONECOTIZACIONs
                            on s.SOCOSOLICITUD equals c.SOCOSOLICITUD
                         join ca in cx.PONECATALOGOs
                            on ds.CATACATALOGO equals ca.CATACATALOGO
                         join cv in cx.POGECLASEVALORs
                            on ca.CLASUNIDADMEDIDA4 equals cv.CLVACLASEVALOR
                         join p in cx.PONEPROVEEDORs
                            on c.PROVPROVEEDOR equals p.PROVPROVEEDOR
                         join dc in cx.PONEDETALLECOTIZACIONs
                            on new { c.COTICOTIZACION, ca.CATACATALOGO } equals new { dc.COTICOTIZACION, dc.CATACATALOGO } into DetalleCotizacion
                         from dcl in DetalleCotizacion.DefaultIfEmpty()
                         where es.ESSOESTADO == estado.ToString()
                                && s.SOCOSOLICITUD == codigoSolicitud
                                && dcl.DECOCANTIDAD > 0
                         select new CotizacionProveedor
                         {
                             CodigoCotizacion = Convert.ToInt32(c.COTICOTIZACION),
                             CantidadSolicitud = Convert.ToInt32(ds.DESOCANTIDAD),
                             CantidadCotizacion = Convert.ToInt32(dcl.DECOCANTIDAD),
                             NombreProducto = ca.CATANOMBRE,
                             Catalogo = Convert.ToInt32(ca.CATACATALOGO),
                             Identificacion = p.PROVIDENTIFICACION,
                             CodigoProveedor = Convert.ToInt32(p.PROVPROVEEDOR),
                             RazonSocial = p.PROVRAZONSOCIAL,
                             UnidadSolicitud = cv.CLVAVALOR,
                             ValorUnitario = Convert.ToInt64(dcl.DECOVALORUNITARIO),
                             ValorIVA = Convert.ToInt64(dcl.DECOVALORUNITARIO * (dcl.DECOPORCIVA / 100))


                         }).ToList();


            return query;
        }

        /// <summary>
        /// Obtiene la lista de adjuntos de cotizacion
        /// </summary>
        /// <param name="solicitud"></param>
        /// <param name="codigoProveedor"></param>
        /// <returns></returns>
        public async Task<List<AdjuntoCotizacion>> ListarAdjuntosCotizacion(int solicitud, int codigoProveedor)
        {
            List<AdjuntoCotizacion> lst_cotizacion = new List<AdjuntoCotizacion>();
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var cotizacion = (from p in cx.PONECOTIZACIONs
                              where p.SOCOSOLICITUD == solicitud && p.PROVPROVEEDOR == codigoProveedor
                              select p.COTICOTIZACION).FirstOrDefault();

            var documentos = (from d in cx.PONEDOCUMENTOCOTIZACIONs
                              join q in cx.PONEDOCUMENTOs on d.DOCUDOCUMENTO equals q.DOCUDOCUMENTO
                              join c in cx.POGECLASEVALORs on q.CLASTIPODOCUMENTO8 equals c.CLVACLASEVALOR
                              where d.COTICOTIZACION == cotizacion
                              select new {
                                CodigoDocumento = q.DOCUDOCUMENTO,
                                FechaCreacion = q.DOCUFECHACREACION,
                                //Blob = Convert.ToInt32(q.BLOBBLOB),
                                Ruta = q.DOCURUTA,
                                NombreDocumento = q.DOCUNOMBRE,
                                Tipo = q.CLASTIPODOCUMENTO8,
                                NombreTipo = c.CLVAVALOR,
                                ContentType = q.DOCUCONTENTTYPE
                              }).ToList();

            foreach (var item in documentos)
            {
                //byte[] buffer = (from p in cx.PONEBLOBs
                 //                where p.BLOBBLOB == item.Blob
                  //               select p.BLOBDATO).FirstOrDefault();

                var adjunto = new AdjuntoCotizacion();
                adjunto.CodigoCotizacion = Convert.ToInt32(cotizacion);
                adjunto.Adjunto = new Documento
                {
                    CodigoDocumento = (int)item.CodigoDocumento,
                    FechaCreacion = item.FechaCreacion,
                    //CodigoBlob = item.Blob,
                    //DataB64 = Convert.ToBase64String(buffer),
                    Nombre = item.NombreDocumento,
                    Tipo = Convert.ToInt32(item.Tipo),
                    NombreTipo = item.NombreTipo,
                    Ruta = item.Ruta,
                    ContentType = item.ContentType
                };
                lst_cotizacion.Add(adjunto);

            }

            return await Task.FromResult(lst_cotizacion); 
        }

        /// <summary>
        /// Obtiene los Documentos requeridos para la cotizacion
        /// </summary>
        /// <param name="codigoSolicitud"></param>
        /// <returns></returns>
        public List<DocumentoInvitacion> GetDocumentosRequeridos(int codigoSolicitud)
        {
            List<DocumentoInvitacion> lst_documentos = new List<DocumentoInvitacion>();
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            lst_documentos = (from s in cx.PONEDOCSINVITACIONs
                              join t in cx.POGECLASEVALORs on s.DOCIDOCUMENTO18 equals t.CLVACLASEVALOR
                              where s.SOCOSOLICITUD == codigoSolicitud
                              select new DocumentoInvitacion
                              {
                                  Codigo = Convert.ToInt32(s.DOCISECUENCIA),
                                  CodigoSolicitud = Convert.ToInt32(s.SOCOSOLICITUD),
                                  CodigoDocumento = Convert.ToInt32(s.DOCIDOCUMENTO18),
                                  NombreDocumento = t.CLVAVALOR,
                                  DescripcionDocumento = t.CLVADESCRIPCION
                              }).ToList();

            return lst_documentos;
        }

        /// <summary>
        /// Metodo para adjudicar la cotizacion
        /// </summary>
        /// <param name="request">Objeto complejo cotizacion</param>
        /// <returns>OK si todo esta bien o el mensaje de error</returns>
        public string Adjudicar(Adjudicacion request)
        {
            //var solicitudBusiness = new SolicitudBusiness();          
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            cx.Connection.Open();
            using var dbContextTransaction = cx.Connection.BeginTransaction();
            try
            {
                //Actualiza la solicitud
                var solicitud = (from p in cx.PONESOLICITUDCOMPRAs
                                 where p.SOCOSOLICITUD == request.CodigoSolicitud
                                 select p).FirstOrDefault();

                solicitud.LOGSUSUARIO = request.CodigoUsuario; 
                solicitud.LOGSFECHA = DateTime.Now;

                string estadoSolicitud = string.Empty;
                string etapaSolicitud = string.Empty;

                if (request.EstadoSolicitud == Configuracion.TipoEstadoAdjudicado)
                {
                    estadoSolicitud = _solicitudCompra.GetEstadoSolicitud("ADJUDICADO").FirstOrDefault().CodigoEstado.ToString();
                    etapaSolicitud = Configuracion.TipoEstadoAdjudicado;

                    foreach(var cotixprov in request.Adjudicados) //Inserta la cotizacion o cotizaciones adjudicadas
                    {
                        //Adjudicacion
                        var tblAdjudicacion = new PONEADJUDICACION();
                        int codigoAdjudicacion = _utilidades.GetSecuencia("SECU_PONEADJUDICACION", cx);
                        tblAdjudicacion.ADJUADJUDICACION = codigoAdjudicacion;
                        tblAdjudicacion.COTICOTIZACION = cotixprov.CodigoCotizacion;
                        tblAdjudicacion.SOCOSOLICITUD = cotixprov.CodigoSolicitud;
                        tblAdjudicacion.LOGSUSUARIO = request.CodigoUsuario;

                        cx.PONEADJUDICACIONs.InsertOnSubmit(tblAdjudicacion);
                    }
                }
                else
                {
                    estadoSolicitud = _solicitudCompra.GetEstadoSolicitud("DESIERTO").FirstOrDefault().CodigoEstado.ToString();
                    etapaSolicitud = Configuracion.TipoEstadoDesierto;
                }

                solicitud.SOCOESTADO = estadoSolicitud;                 
                solicitud.SOCOETAPA = etapaSolicitud;
                solicitud.SOCOOBSERVACIONADJUDICACION = request.Observacion;
                
                cx.SubmitChanges();
                dbContextTransaction.Commit();

                Thread t = new Thread(() =>
                {
                    string tipoNotificacion = string.Empty;
                    NotificacionAdjudicacion cuerpoNotificacion;
                    using (PORTALNEGOCIODataContext cx1 = new PORTALNEGOCIODataContext())
                    {
                        if (request.EstadoSolicitud == Configuracion.TipoEstadoAdjudicado)
                        {
                            tipoNotificacion = Configuracion.NotificacionAjudicado;

                            cuerpoNotificacion = (from s in cx1.PONESOLICITUDCOMPRAs
                                                  where s.SOCOSOLICITUD == request.CodigoSolicitud
                                                  select new NotificacionAdjudicacion
                                                  {
                                                      CodigoSolicitud = (int)s.SOCOSOLICITUD,
                                                      Descripcion = s.SOCODESCRIPCION,
                                                      ProveedorAdjudicado = (from p in cx1.PONEPROVEEDORs 
                                                                   join c in cx1.PONECOTIZACIONs on p.PROVPROVEEDOR equals c.PROVPROVEEDOR
                                                                   join a in cx1.PONEADJUDICACIONs on c.COTICOTIZACION equals a.COTICOTIZACION
                                                                   where a.SOCOSOLICITUD == request.CodigoSolicitud
                                                                   select new ProveedorNotificacion
                                                                   {
                                                                       Nit = p.PROVIDENTIFICACION,
                                                                       RazonSocial = p.PROVRAZONSOCIAL
                                                                   }).ToList()                                                     
                                                     
                                                  }).SingleOrDefault();
                        }
                        else
                        {
                            tipoNotificacion = Configuracion.NotificacionDesierto;

                            cuerpoNotificacion = (from s in cx1.PONESOLICITUDCOMPRAs
                                                  where s.SOCOSOLICITUD == request.CodigoSolicitud
                                                  select new NotificacionAdjudicacion
                                                  {
                                                      CodigoSolicitud = (int)s.SOCOSOLICITUD,
                                                      Descripcion = s.SOCODESCRIPCION,
                                                      RazonSocial = string.Empty,
                                                      Nit = string.Empty
                                                  }).SingleOrDefault();
                        }
                    }

                    _notificacion.GenerarNotificacion(tipoNotificacion, cuerpoNotificacion);
                
                });

                t.Start();
                t.IsBackground = true;

                return ("OK");
            }
            catch (Exception ex)
            {
                dbContextTransaction.Rollback();
                return ex.Message;
            }
        }


        public Adjudicacion GetAdjudicadoXSolicitud(int codigoSolicitud)
        {
            Adjudicacion lst_adjudicados = new Adjudicacion();
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            lst_adjudicados = (from s in cx.PONEADJUDICACIONs
                              join t in cx.PONECOTIZACIONs on s.COTICOTIZACION equals t.COTICOTIZACION
                              join p in cx.PONEPROVEEDORs  on t.PROVPROVEEDOR equals p.PROVPROVEEDOR
                              where t.SOCOSOLICITUD == codigoSolicitud
                              select new Adjudicacion
                              {
                                  //TODO: Revisar ya que la fecha de adjudicacion debe salir del historco
                                 //FechaAdjudicacion = DateTime.Now, // s.ADJUFECHA,
                                 //CodigoCotizacion = (int)t.COTICOTIZACION,
                                 //NombreProveedor = p.PROVRAZONSOCIAL
                              }).FirstOrDefault();

            return lst_adjudicados;
        }

        public async Task<List<DocumentoFichaTecnica>> ObtenerListaFichasTecnicas(int idSolicitud, int idProveedor)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                List<DocumentoFichaTecnica> ltaFichas = new List<DocumentoFichaTecnica>();
                var n = (from x in cx.PONECOTIZACIONs
                         join d in cx.PONEDETALLECOTIZACIONs on x.COTICOTIZACION equals d.COTICOTIZACION
                         join dc in cx.PONEDOCUMENTODETALLECOTs on d.DECODETALLE equals dc.DECODETALLE
                         join doc in cx.PONEDOCUMENTOs on dc.DOCUDOCUMENTO equals doc.DOCUDOCUMENTO                         
                         where x.SOCOSOLICITUD == idSolicitud && x.PROVPROVEEDOR == idProveedor
                         select new DocumentoFichaTecnica
                         {
                             CodigoDetalle = Convert.ToInt32(d.DECODETALLE),
                             CodigoCatalogo = d.PONECATALOGO.CATACODCATALOGO,
                             NombreCatalogo = d.PONECATALOGO.CATANOMBRE,
                             NombreDocumento = doc.DOCUNOMBRE,
                             CodigoDocumento = Convert.ToInt32(dc.DOCUDOCUMENTO),
                             //CodigoBlob = Convert.ToInt32(doc.BLOBBLOB),
                             //DataB64 = Convert.ToBase64String(blob.BLOBDATO) 
                         }).ToList();

                foreach (var item in n)
                {
                    byte[] buffer = (from p in cx.PONEBLOBs
                                     where p.BLOBBLOB == item.CodigoBlob
                                     select p.BLOBDATO).FirstOrDefault();

                    var ficha = new DocumentoFichaTecnica
                    {
                        CodigoDetalle = item.CodigoDetalle,
                        CodigoCatalogo = item.CodigoCatalogo,
                        NombreCatalogo = item.NombreCatalogo,
                        NombreDocumento = item.NombreDocumento,
                        CodigoDocumento = item.CodigoDocumento,
                        CodigoBlob = item.CodigoBlob,
                        DataB64 = Convert.ToBase64String(buffer)
                    };


                    ltaFichas.Add(ficha);

                }

                return await Task.FromResult(ltaFichas);
            }
        }
        #endregion

        #region Metodos Privados
        private void CargarDetalleCotizacion(List<DetalleCotizacion> elementosCotizacion, PORTALNEGOCIODataContext cx, int codigoCotizacion, int idUsuario)
        {
            foreach (var item in elementosCotizacion)
            {
                // Inserta el documento
                var tblDetalleCotizacion = new PONEDETALLECOTIZACION();
                int codigoDetalle = _utilidades.GetSecuencia("SECU_PONEDETALLECOTIZACION", cx);
                tblDetalleCotizacion.DECODETALLE = codigoDetalle;
                tblDetalleCotizacion.CATACATALOGO = item.Catalogo;
                tblDetalleCotizacion.DECOCANTIDAD = Convert.ToDecimal(item.Cantidad);
                tblDetalleCotizacion.DECOVALORUNITARIO = Convert.ToInt64(item.ValorUnitario);
                tblDetalleCotizacion.DECOVALORIVA = Convert.ToInt64(item.ValorIVA);
                tblDetalleCotizacion.DECOPORCIVA = (double)item.PorcentajeIVA;
                tblDetalleCotizacion.COTICOTIZACION = codigoCotizacion;
                tblDetalleCotizacion.LOGSUSUARIO = idUsuario;

                cx.PONEDETALLECOTIZACIONs.InsertOnSubmit(tblDetalleCotizacion);
                cx.SubmitChanges();

                if(item.FichaTecnica != null)
                {
                    CargarAnexosDetalle(item.FichaTecnica, cx, codigoDetalle, idUsuario);
                }

            }

        }

        /// <summary>
        /// Carga los anexos del detalle de la cotizacion
        /// </summary>
        /// <param name="documentoDetalle"></param>
        /// <param name="cx"></param>
        /// <param name="codigoDetalle"></param>
        private void CargarAnexosDetalle(List<Documento> documentoDetalle, PORTALNEGOCIODataContext cx, int codigoDetalle, int idUsuario)
        {
            foreach (var item in documentoDetalle)
            {
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
               // tblDocumento.BLOBBLOB = codigoBlob;
                tblDocumento.DOCUNOMBRE = item.Nombre;
                tblDocumento.DOCUFECHACREACION = DateTime.Now;
                tblDocumento.CLASTIPODOCUMENTO8 = item.Tipo;
                tblDocumento.LOGSUSUARIO = idUsuario;
                cx.PONEDOCUMENTOs.InsertOnSubmit(tblDocumento);
                cx.SubmitChanges();

                //Inserta la relacion
                var tblDocumentoDetalle = new PONEDOCUMENTODETALLECOT();
                tblDocumentoDetalle.DOCUDOCUMENTO = codigoDocumento;
                tblDocumentoDetalle.DODESECUENCIA = _utilidades.GetSecuencia("SECU_PONEDOCUMENTODETALLECOT", cx);
                tblDocumentoDetalle.DECODETALLE = codigoDetalle;              
                cx.PONEDOCUMENTODETALLECOTs.InsertOnSubmit(tblDocumentoDetalle);
                cx.SubmitChanges();
            }    

        }
        
        /// <summary>
        /// Carga los anexos de la cotizacion
        /// </summary>
        /// <param name="documentoCotizacion"></param>
        /// <param name="cx"></param>
        /// <param name="codigoCotizacion"></param>
        private async Task CargarAnexosCotizacion(List<Documento> documentoCotizacion, PORTALNEGOCIODataContext cx, int codigoCotizacion, int codigoSolicitud, int idUsuario)
        {
            foreach (var item in documentoCotizacion)
            {
                // Inserta el blob
                /*var tblBlobs = new PONEBLOB();
                tblBlobs.BLOBDATO = _utilidades.DecodificarArchivo(item.DataB64);
                int codigoBlob = _utilidades.GetSecuencia("SECU_PONEBLOB", cx);
                tblBlobs.BLOBBLOB = codigoBlob;
                cx.PONEBLOBs.InsertOnSubmit(tblBlobs);
                cx.SubmitChanges();
                */
                var folder = $"invitaciones\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString().PadLeft(2, '0')}\\" +
                    $"{codigoSolicitud.ToString()}\\cotizacion{codigoCotizacion.ToString()}";

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
                var tblDocumentoCotizacion = new PONEDOCUMENTOCOTIZACION();
                tblDocumentoCotizacion.DOCUDOCUMENTO = codigoDocumento;
                tblDocumentoCotizacion.DOCOSECUENCIA = _utilidades.GetSecuencia("SECU_PONEDOCUMENTOCOTIZACION", cx);
                tblDocumentoCotizacion.COTICOTIZACION = codigoCotizacion;
                tblDocumentoCotizacion.LOGSUSUARIO = idUsuario;
                cx.PONEDOCUMENTOCOTIZACIONs.InsertOnSubmit(tblDocumentoCotizacion);
                cx.SubmitChanges();

            }

        }

        public List<ValoresArchivo> ValidarCargaMasiva(CotizacionMasiva request)
        {
            // Leer archivo de excel
            var valores = request.ArchivoB64.LeerArchvoExcel(1);

            // Validar el archivo
            Func<int, List<ValoresArchivo>, List<ConfiguracionArchivo>, List<ValoresArchivo>> validacionCargaMasiva = ValidacionCargaMasiva;

            //Valida el archivo de excel
            var valoresValidados = ExcelUtilities.ValidarArchivoExcelCotizacion(request.CodigoSolicitud, validacionCargaMasiva, valores, null);

            // Retornar elementos validados
            return valoresValidados.Any() ? valoresValidados : valores;
        }

        public List<DetalleCotizacion> TransformarArchivo(List<ValoresArchivo> validaciones)
        {
            List<DetalleCotizacion> detallesCotizacion = new List<DetalleCotizacion>();

            var codigoCatalogo = validaciones.Where(x => x.celda.StartsWith("A")).Select(x => x.valor).ToList();
            var cantidadRequerida = validaciones.Where(x => x.celda.StartsWith("D")).Select(x => x.valor).ToList();
            var cantidadOfertada = validaciones.Where(x => x.celda.StartsWith("G")).Select(x => x.valor).ToList();
            var valorUnitario = validaciones.Where(x => x.celda.StartsWith("H")).Select(x => x.valor).ToList();
            var porcentajeIva = validaciones.Where(x => x.celda.StartsWith("I")).Select(x => x.valor).ToList();

            detallesCotizacion = codigoCatalogo.Zip(cantidadRequerida, (x, y) =>
            {
                return new DetalleCotizacion
                {
                    Catalogo = Convert.ToInt32(x),
                    CantidadRequerida = Convert.ToInt32(y)                    
                };
            }).Zip(cantidadOfertada, (x, y) =>
            {
                return new DetalleCotizacion
                {
                    Catalogo = x.Catalogo,
                    CantidadRequerida = x.CantidadRequerida,
                    Cantidad = Convert.ToInt32(y)                   
                };
            }).Zip(valorUnitario, (x, y) =>
            {
                return new DetalleCotizacion
                {
                    Catalogo = x.Catalogo,
                    CantidadRequerida = x.CantidadRequerida,
                    Cantidad = x.Cantidad,
                    ValorUnitario = Convert.ToDecimal(y)
                };
            }).Zip(porcentajeIva, (x, y) =>
            {
                return new DetalleCotizacion
                {
                    Catalogo = x.Catalogo,
                    CantidadRequerida = x.CantidadRequerida,
                    Cantidad = x.Cantidad,
                    ValorUnitario = x.ValorUnitario,
                    PorcentajeIVA = Convert.ToDecimal(y)
                };
            }).ToList();

            return detallesCotizacion;
        }

        #endregion

        #region Metodos Privados
        private List<ValoresArchivo> ValidacionCargaMasiva(int codigoSolicitud, List<ValoresArchivo> valores, List<ConfiguracionArchivo> configuracion)
        {
            List<ValoresArchivo> valoresInvalidos = new List<ValoresArchivo>();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {                
                const string columnaCodigoCatalogo = "A";
                const string columnaCantidadRequerida = "D";
                const string columnaCantidadOfertada = "G";
                const string columnaValorUnitario = "H";
                const string columnaPorcentajeIva = "I";

                // Obtiene los codigos de catalogo
                var detaSoli = cx.PONEDETALLESOLICITUDs.Where(x => x.SOCOSOLICITUD == codigoSolicitud).ToList();

                var nroRegistro = valores.Where(x => (x.celda.Contains(columnaCodigoCatalogo))).ToList();

                //Valido que los registros del archivo concuerden con el numero de registros de la solicitud

                if(nroRegistro.Count() > detaSoli.Count())
                {
                    return new List<ValoresArchivo> {
                        new ValoresArchivo
                        {
                            celda = "A2",
                            hoja = 1,
                            valor = "",
                            error = $"El archivo cargado tiene mas elementos ({ nroRegistro.Count() }), que los solicitados en la invitación a cotizar ({ detaSoli.Count() })"
                        }
                    };
                }

                //1. Valida que los campos Codigo y cantidad esten diligenciados
                var valoresNulos = valores.Where(x => (x.celda.Contains(columnaCodigoCatalogo) || x.celda.Contains(columnaCantidadRequerida) || x.celda.Contains(columnaCantidadOfertada) 
                                                    || x.celda.Contains(columnaValorUnitario) || x.celda.Contains(columnaPorcentajeIva))
                                                        && string.IsNullOrEmpty(x.valor))
                                            .ToList();

                //2. Valida que la columna cantidad sea numerico
                var valoresNoNumericos = valores.Where(x => (x.celda.Contains(columnaValorUnitario) || x.celda.Contains(columnaCantidadOfertada) || x.celda.Contains(columnaCantidadRequerida)
                                                          || x.celda.Contains(columnaCodigoCatalogo) || x.celda.Contains(columnaPorcentajeIva))
                                                        && !int.TryParse(x.valor, out _))
                                            .ToList();

                if (valoresNulos.Any())
                    valoresInvalidos.AddRange(valoresNulos.Select(x => new ValoresArchivo
                    {
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
        

                var codigosCatalogo = detaSoli.Select(x => x.CATACATALOGO.ToString()).ToList();

                // Obtiene los codigos que no existen en el catalogo
                var codigosNoExistentes = valores.Where(x => x.celda.StartsWith(columnaCodigoCatalogo) && !codigosCatalogo.Contains(x.valor))
                .ToList();

                if (codigosNoExistentes.Any())
                    valoresInvalidos.AddRange(codigosNoExistentes.Select(x => new ValoresArchivo
                    {
                        celda = x.celda,
                        valor = x.valor,
                        hoja = x.hoja,
                        error = "No existe el código de catalogo en el detalle de la invitación a cotizar."
                    }));

                // Obtiene los registros donde la cantidad ofertada es mayor que la cantidad solicitada
                //Valido que las cantidades ofertadas sean iguales o menores que las soliciadas
                List<ValoresArchivo> valoresCodigoCantidad = valores.Where(x => (x.celda.Contains(columnaCodigoCatalogo)) || x.celda.Contains(columnaCantidadOfertada)).ToList();
                List<ValoresArchivo> cantidadInvalida = new List<ValoresArchivo>();

                detaSoli.ForEach(item =>
                {
                    var reg = valoresCodigoCantidad.Where(x => x.valor == item.CATACATALOGO.ToString() && x.celda.Contains(columnaCodigoCatalogo)).SingleOrDefault();

                    if (reg != null)
                    {
                        string cell = $"{ columnaCantidadOfertada }{reg.celda[1..]}"; //  (string)columnaCantidadOfertada.Concat(reg.celda.Substring(1, reg.celda.Length - 1));
                        var t = valoresCodigoCantidad.Where(x => x.celda.Equals(cell)).SingleOrDefault();
                        if (t != null)
                        {
                            if (Convert.ToInt32(t.valor) > item.DESOCANTIDAD)
                            {
                                cantidadInvalida.Add(new ValoresArchivo
                                {
                                    celda = cell,
                                    valor = t.valor,
                                    hoja = t.hoja,
                                    error = $"La cantidad ofertada ({ t.valor }) para el codigo catalogo { item.CATACATALOGO }, es mayor a la solicitada ({ item.DESOCANTIDAD })."
                                }); ;
                            }
                        }
                    }
                });

                if (cantidadInvalida.Any())
                    valoresInvalidos.AddRange(cantidadInvalida.Select(x => new ValoresArchivo
                    {
                        celda = x.celda,
                        valor = x.valor,
                        hoja = x.hoja,
                        error = x.error // "No existe el código de catalogo en el detalle de la invitación a cotizar."
                    }));


                if (valoresInvalidos.Any())
                    return valoresInvalidos.Select(x => new ValoresArchivo
                    {
                        celda = x.celda,
                        valor = x.valor,
                        hoja = x.hoja,
                        error = x.error
                    }).ToList();                
            }

            return valoresInvalidos;
        }

        #endregion

    }
}
