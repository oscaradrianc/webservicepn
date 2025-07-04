using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class PreguntasBusiness : IPreguntas
    {
        private readonly IUtilidades _utilidades;
        public PreguntasBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }

        #region Metodos Publicos

        /// <summary>
        /// Obtiene las preguntas por Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Preguntas> ListPreguntas(int id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return ListarPreguntasAll(id, cx);
            }
        }

        /// <summary>
        /// Obtiene todas las preguntas sin respuesta aun
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Preguntas> ListPreguntasSinRespuesta(int id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return ListarPreguntasAll(id, cx).Where(x=> x.FechaRespuesta == null).ToList();
            }
        }


        /// <summary>
        /// Crea la pregunta
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string CrearPregunta(CrearPregunta request)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();

                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        PONEPREGUNTASOLICITUD tblpregunta = new PONEPREGUNTASOLICITUD();

                        tblpregunta.PRSOPREGUNTA = _utilidades.GetSecuencia("SECU_PONEPREGUNTA", cx);
                        tblpregunta.PROVPROVEEDOR = request.CodigoProveedor;
                        tblpregunta.SOCOSOLICITUD = request.CodigoSolicitud;
                        tblpregunta.PRSOFECHAPREGUNTA = DateTime.Now;
                        tblpregunta.PRSODESCRIPCION = request.Pregunta;

                        cx.PONEPREGUNTASOLICITUDs.InsertOnSubmit(tblpregunta);
                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        //////////////////Envia Correo a las personas parametrizadas cuando se realiza un pregunta//////////////////////////
                        Thread t = new Thread(() =>

                            (new NotificacionBusiness(_utilidades)).GenerarNotificacion("registropregunta", request)
                        );
                        t.Start();
                        t.IsBackground = true;
                        //////////////////////////////////////////////////////////////////////////////////////////


                        return ("OK");
                    }
                    catch(Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return (ex.Message);
                    }
                    finally
                    {
                        cx.Connection.Close();
                    }
                 }
            }
        }

        /// <summary>
        /// Crea la respuesta
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string CrearRespuesta(CrearRespuesta request)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();

                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var query = (from x in cx.PONEPREGUNTASOLICITUDs
                                     where x.PRSOPREGUNTA == request.CodigoPregunta && x.SOCOSOLICITUD == request.CodigoSolicitud
                                     select x).FirstOrDefault();

                        query.PRSORESPUESTA = request.Respuesta;
                        query.PRSOFECHARESPUESTA = DateTime.Now;
                        query.PRSOUSUARIORESPUESTA = request.UsuarioRespuesta;

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        //////////////////Envia Correo al proveedor que realizo la pregunta////////////////////////
                        Thread t = new Thread(() =>

                            (new NotificacionBusiness(_utilidades)).GenerarNotificacion("registrorespuesta", request)
                        );
                        t.Start();
                        t.IsBackground = true;
                        //////////////////////////////////////////////////////////////////////////////////////////


                        return ("OK");
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return (ex.Message);
                    }
                    finally
                    {
                        cx.Connection.Close();
                    }
                }
            }
        }

        #endregion

        #region Metodos Privados
        
        /// <summary>
        /// Lista todas las preguntas
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cx"></param>
        /// <returns></returns>
        private List<Preguntas> ListarPreguntasAll(int id, PORTALNEGOCIODataContext cx)
        {
            var s = (from x in cx.PONEPREGUNTASOLICITUDs
                     join y in cx.PONEPROVEEDORs on x.PROVPROVEEDOR equals y.PROVPROVEEDOR into prov
                     from proveedor in prov.DefaultIfEmpty()
                     join z in cx.POGEUSUARIOs on x.PRSOUSUARIORESPUESTA equals z.USUAUSUARIO into usua
                     from usuario in usua.DefaultIfEmpty()
                     where x.SOCOSOLICITUD == id
                        select new Preguntas
                        {
                            CodigoPregunta = Convert.ToInt32(x.PRSOPREGUNTA),
                            FechaPregunta = Convert.ToDateTime(x.PRSOFECHAPREGUNTA),
                            Pregunta = x.PRSODESCRIPCION,
                            CodigoProveedor = Convert.ToInt32(x.PROVPROVEEDOR),
                            CodigoSolicitud = Convert.ToInt32(x.SOCOSOLICITUD),
                            Respuesta = x.PRSORESPUESTA,
                            FechaRespuesta = x.PRSOFECHARESPUESTA,
                            UsuarioRespuesta = Convert.ToInt32(x.PRSOUSUARIORESPUESTA),
                            NombreProveedor = proveedor.PROVRAZONSOCIAL,
                            NombreUsuario = usuario.USUANOMBRE

                        }).OrderBy(x=> x.FechaPregunta).ToList();
            return s;   
        }
        #endregion
    }
}
