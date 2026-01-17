using Negocio.Data;
using Negocio.Model;
using Negocio.Model.Consultas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class ConsultasBusiness: IConsultas
    {

        #region Metodos Publicos
        /*public Response<IQueryable<FOBTENERPAGOResult>> ObtenerPagos(int idEmpresa, decimal idProveedor, int periodoInicial, int periodoFinal)
        {
            Response<IQueryable<FOBTENERPAGOResult>> resp = new Response<IQueryable<FOBTENERPAGOResult>>();
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.DeferredLoadingEnabled = false;
                resp = new Response<IQueryable<FOBTENERPAGOResult>>
                {
                    Data = cx.FOBTENERPAGO(idEmpresa, idProveedor, periodoInicial, periodoFinal),
                    Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = "" }
                };

                return resp;
            }
        }*/
        public Response<List<FOBTENERPAGOResult>> ObtenerPagos(int idEmpresa, decimal idProveedor, int periodoInicial, int periodoFinal)
        {
            Response<List<FOBTENERPAGOResult>> resp;
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                int periodoConbleActual = cx.PONEVPERIODOCONTABLEs
                         .Select(p => p.ANOACTUAL * 100 + p.MESACTUAL).SingleOrDefault();

                if (periodoInicial >= periodoConbleActual && periodoFinal >= periodoConbleActual)
                {
                    resp = new Response<List<FOBTENERPAGOResult>>
                    {
                        Data = null,
                        Status = new ResponseStatus { Status = Configuracion.StatusError, Message = "No se puede realizar la consulta, el periodo de consulta no está cerrado en contabilidad." }
                    };

                    return resp;
                }


                var listaPagos = cx.FOBTENERPAGO(idEmpresa, idProveedor, periodoInicial, periodoFinal).ToList();

                return new Response<List<FOBTENERPAGOResult>>
                {
                    Data = listaPagos,
                    Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = "" }
                };
            }
        }

        public Response<DetallePagoResponse> ObtenerDetallePago(int idEmpresa, int vigOrpa, int orpa, int nroAuxiliar )
        {
            Response<DetallePagoResponse> resp = new Response<DetallePagoResponse>();
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                DetallePagoResponse detallePago = new DetallePagoResponse();
                detallePago.DetallePago = cx.FDETALLEPAGO((decimal)idEmpresa, (decimal)vigOrpa, (decimal)orpa, (decimal)nroAuxiliar).SingleOrDefault();
                detallePago.Descuentos =  cx.FDESCUENTOPAGO((decimal)vigOrpa, (decimal)orpa, (decimal)idEmpresa).ToList();

                resp = new Response<DetallePagoResponse>
                {
                    Data = detallePago,
                    Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = "" }
                };
            }
            return resp;
        }

        /// <summary>
        /// Consulta para obtener datos para generar informe de retenciones por proveedor y vigencia
        /// </summary>
        /// <param name="idEmpresa"></param>
        /// <param name="vigOrpa"></param>
        /// <param name="orpa"></param>
        /// <param name="nroAuxiliar"></param>
        /// <returns></returns>
        public Response<RetencionResponse> ObtenerRetenciones(int idProveedor, string tipoRetencion, int periodo1, int periodo2)
        {
            Response<RetencionResponse> resp = new Response<RetencionResponse>();
            RetencionResponse retencion = new RetencionResponse();
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                int periodoConbleActual = cx.PONEVPERIODOCONTABLEs
                         .Select(p => p.ANOACTUAL * 100 + p.MESACTUAL).SingleOrDefault();

                if(periodo1 >= periodoConbleActual && periodo2 >= periodoConbleActual)
                {
                    resp = new Response<RetencionResponse>
                    {
                        Data = null,
                        Status = new ResponseStatus { Status = Configuracion.StatusError, Message = "No se puede realizar la consulta, el periodo de consulta no está cerrado en contabilidad." }
                    };

                    return resp;
                }
                

                retencion.EncabezadoRetencion = cx.FENCABEZADORETENCION((int)periodo1, (int)periodo2, tipoRetencion, (int)idProveedor).SingleOrDefault();
                retencion.DetalleRetencion = cx.FDETALLERETENCION((int)periodo1, (int)periodo2, tipoRetencion, (int)idProveedor).ToList();

                if(retencion.EncabezadoRetencion == null)
                {
                    resp = new Response<RetencionResponse>
                    {
                        Data = null,
                        Status = new ResponseStatus { Status = Configuracion.StatusError, Message = "La consulta no arrojo resultados." }
                    };

                    return resp;
                }

                resp = new Response<RetencionResponse>
                {
                    Data = retencion,
                    Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = "" }
                };
            }

            return resp;
        }

        public Response<List<EstadoProceso>> ObtenerEstadoProcesos(int? idSolicitud, string fechaInicial, string fechaFinal, string estado)
        {
            Response<List<EstadoProceso>> resp = new Response<List<EstadoProceso>>();

            try
            {
                using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
                {
                    string[] tipoEstados = { "5", "6", "7", "8" };
                    var lta = (from s in cx.PONESOLICITUDCOMPRAs
                               join e in cx.PONEESTADOSOLICITUDs on s.SOCOESTADO equals e.ESSOESTADO
                               let nrocoti = (int)cx.PONECOTIZACIONs.Count(c => c.SOCOSOLICITUD == s.SOCOSOLICITUD)
                               where tipoEstados.Contains(e.ESSOESTADO)
                               && (idSolicitud == null || s.SOCOSOLICITUD == idSolicitud)
                               && (estado == null || s.SOCOESTADO == estado)                               
                               select new EstadoProceso
                               {
                                   CodigoSolicitud = (int)s.SOCOSOLICITUD,
                                   Descripcion = s.SOCODESCRIPCION,
                                   FechaPublicacion = s.SOCOFECHAPUBLICACION,
                                   FechaPregunta = s.SOCOFECHAPREGUNTA,
                                   FechaRespuesta = s.SOCOFECHARESPUESTA,
                                   FechaCierre = s.SOCOFECHACIERRE,
                                   TipoSolicitud = s.SOCOTIPOSOLICITUD,
                                   Estado = s.SOCOESTADO,
                                   EstadoNombre = e.ESSONOMBRE,
                                   NroCotizaciones = nrocoti
                               });

                    if (fechaInicial != null && fechaInicial != "null") 
                    {
                        lta = lta.Where(p => Convert.ToDateTime(p.FechaPublicacion).Date >= Convert.ToDateTime(fechaInicial).Date);
                    }
                    if (fechaFinal != null && fechaFinal != "null")
                    {
                        lta = lta.Where(p => Convert.ToDateTime(p.FechaPublicacion).Date <= Convert.ToDateTime(fechaFinal).Date);
                    }

                    resp.Data = lta.ToList();
                    resp.Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = string.Empty };
                }
            }
            catch (Exception ex)
            {
                resp.Status = new ResponseStatus { Status = Configuracion.StatusError, Message = ex.Message };
                resp.Data = null;
            }

            return resp;
        }


        public Response<List<SolicitudAnulado>> ObtenerSolicitudesAnuladas(string fechaInicial, string fechaFinal)
        {
            Response<List<SolicitudAnulado>> resp = new Response<List<SolicitudAnulado>>();

            try
            {
                using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
                {                   
                    var lta = (from s in cx.PONESOLICITUDCOMPRAs
                               join e in cx.PONEESTADOSOLICITUDs on s.SOCOESTADO equals e.ESSOESTADO
                               join u in cx.POGEUSUARIOs on s.LOGSUSUARIO equals u.USUAUSUARIO
                               join a in cx.PONEVAREAs on s.CLASAREA9 equals a.CODAREA
                               let nrocoti = (int)cx.PONECOTIZACIONs.Count(c => c.SOCOSOLICITUD == s.SOCOSOLICITUD)
                               where s.SOCOESTADO == "1" //Estado Anulado                               
                               select new SolicitudAnulado
                               {
                                   CodigoSolicitud = (int)s.SOCOSOLICITUD,
                                   Descripcion = s.SOCODESCRIPCION,
                                   FechaSolicitud = s.SOCOFECHA,                                   
                                   FechaAnulacion = s.LOGSFECHA,                                  
                                   Estado = s.SOCOESTADO,
                                   EstadoNombre = e.ESSONOMBRE,
                                   UsuarioAnulo = u.USUANOMBRE,
                                   Valor = s.SOCOVALOR,
                                   Area = a.NOMBRE,
                                   MotivoAnulo = s.SOCOOBSERVACIONESRECHAZA
                               });

                    if (fechaInicial != null && fechaInicial != "null")
                    {
                        lta = lta.Where(p => Convert.ToDateTime(p.FechaSolicitud).Date >= Convert.ToDateTime(fechaInicial).Date);
                    }
                    if (fechaFinal != null && fechaFinal != "null")
                    {
                        lta = lta.Where(p => Convert.ToDateTime(p.FechaSolicitud).Date <= Convert.ToDateTime(fechaFinal).Date);
                    }

                    resp.Data = lta.ToList();
                    resp.Status = new ResponseStatus { Status = Configuracion.StatusOk, Message = string.Empty };
                }
            }
            catch (Exception ex)
            {
                resp.Status = new ResponseStatus { Status = Configuracion.StatusError, Message = ex.Message };
                resp.Data = null;
            }

            return resp;
        }


        #endregion

    }
}
