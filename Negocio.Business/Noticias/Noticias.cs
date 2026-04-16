using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class NoticiasBusiness: INoticias
    {
        private readonly IUtilidades _utilidades;
        private readonly IDataContextFactory _factory;

        public NoticiasBusiness(IUtilidades utilidades, IDataContextFactory factory)
        {
            _utilidades = utilidades;
            _factory = factory;
        }

        public async Task<List<Noticias>> ListNoticias()
        {
            using (var cx = _factory.Create())
            {

                var s = (from x in cx.PONENOTICIAs
                         where x.NOTIESTADO == "A"
                         select new Noticias
                         {
                             CodigoNoticia = Convert.ToInt32(x.NOTINOTICIA),
                             Contenido = x.NOTICONTENIDO,
                             Titulo = x.NOTITITULO,
                             URL = x.NOTIURL,
                             Fecha = x.NOTIFECHA,
                             FotoB64 = x.PONEBLOB.BLOBDATO


                         }).ToList();
                return await Task.FromResult(s);
            }
        }

        public async Task<Noticias> ConsultarNoticiaPorId(int id)
        {
            using (var cx = _factory.Create())
            {
                var n = (from x in cx.PONENOTICIAs
                         where x.NOTINOTICIA == id
                         select new Noticias
                         {
                             CodigoNoticia = Convert.ToInt32(x.NOTINOTICIA),
                             Contenido = x.NOTICONTENIDO,
                             Titulo = x.NOTITITULO,
                             URL = x.NOTIURL,
                             Fecha = x.NOTIFECHA,
                             FotoB64 = x.PONEBLOB.BLOBDATO
                         }).SingleOrDefault();
                return await Task.FromResult(n);
            }
        }

        public async Task<ResponseStatus> ActualizarNoticia(decimal id, Noticias noticia)
        {
            ResponseStatus resp = new ResponseStatus();
            using (var cx = _factory.Create())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var noti = cx.PONENOTICIAs.Where(n => n.NOTINOTICIA == noticia.CodigoNoticia).SingleOrDefault();

                        if (noti != null)
                        {
                            noti.NOTITITULO = noticia.Titulo;
                            noti.NOTICONTENIDO = noticia.Contenido;
                            noti.NOTIURL = noticia.URL;

                            var blob = cx.PONEBLOBs.Where(b => b.BLOBBLOB == noti.BLOBBLOB).SingleOrDefault();
                            blob.BLOBDATO = _utilidades.DecodificarArchivo(noticia.ArchivoB64);
                            //int codigoBlob = _utilidades.GetSecuencia("SECU_PONEBLOB", cx);
                            //blob.BLOBBLOB = codigoBlob;
                            //cx.PONEBLOBs.InsertOnSubmit(tblBlobs);
                            //cx.SubmitChanges();
                        }
                        else
                        {
                            resp.Status = Configuracion.StatusError;
                            resp.Message = $"No se encuentra el id { id } en la tabla de noticias.";
                        }

                        cx.SubmitChanges();
                        await Task.Run(() => dbContextTransaction.Commit());
                        resp.Status = Configuracion.StatusOk;
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al actualizar el registro de noticia " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al actualizar registro de noticia " + ex.Message);
                    }
                }
            }

            return resp;
        }
    }
}
