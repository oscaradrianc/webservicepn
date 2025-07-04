using System.Collections.Generic;
using Negocio.Data;
using Negocio.Model;
using System.Linq;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class CatalogoBusiness : ICatalogo
    {
        private readonly IUtilidades _utilidades;
        public CatalogoBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }
        #region Metodos Publicos

        /// <summary>
        /// Obtiene Todos los Elementos del Catalogo
        /// </summary>
        /// <returns>Lista de elementos del Catalogo</returns>
        public async Task<List<Catalogo>> GetCatalogo()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {                
                return await Task.Run(() => (  
                                                from p in cx.PONECATALOGOs
                                               select GetModelObject(p)).ToList()
                                            );                
            }
        }


        public async Task<List<Catalogo>> GetCatalogoConMedida()
        {            
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {                
                return await Task.Run(() => (from p in cx.PONECATALOGOs
                                             join u in cx.POGECLASEVALORs on p.CLASUNIDADMEDIDA4 equals u.CLVACLASEVALOR
                                             where u.CLASCLASE == Configuracion.ClaseUnidadMedida
                                             select new Catalogo
                                             {
                                                 CodigoInterno = (int)p.CATACATALOGO,
                                                 CodigoCatalogo = p.CATACODCATALOGO,
                                                 Estado = p.CATAESTADO,
                                                 Nombre = p.CATANOMBRE,
                                                 Tipo = p.CATATIPO,
                                                 UnidadMedida = (int)p.CLASUNIDADMEDIDA4,
                                                 Medida = u.CLVAVALOR
                                             }).ToList()
                                        );
            }
        }

        /// <summary>
        /// Obtiene el elemento especifico del Catalogo dado el Id
        /// </summary>
        /// <param name="id">Codigo del Catalogo</param>
        /// <returns>elemento de catalogo</returns>
        public List<Catalogo> GetCatalogo(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_catalogo = (from p in cx.PONECATALOGOs
                                    where p.CATACATALOGO == id
                                    select GetModelObject(p)).ToList();

                return lst_catalogo;
            }
        }

        /// <summary>
        /// Actualiza el elemento del catalogo
        /// </summary>
        /// <param name="id">Codigo del Catalogo</param>
        /// <param name="catalogo">Objeto Catalogo</param>
        public async Task<ResponseStatus> UpdateCatalogo(decimal id, PONECATALOGO catalogo)
        {
            ResponseStatus resp = new ResponseStatus();
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var query = cx.PONECATALOGOs.Where(x => x.CATACATALOGO == id).FirstOrDefault();

                        if (query == null)
                        {
                            //throw new KeyNotFoundException("No se encontro el dato");
                            resp.Status = Configuracion.StatusError;
                            resp.Message = $"No se encuentra el id { id } en la tabla de catálogo.";
                        }
                        else
                        {
                            //query = GetDBObject(catalogo);
                            query.CATACODCATALOGO = catalogo.CATACODCATALOGO;
                            query.CATANOMBRE = catalogo.CATANOMBRE;
                            query.CATATIPO = catalogo.CATATIPO;
                            query.CATAESTADO = catalogo.CATAESTADO;
                            query.CLASUNIDADMEDIDA4 = catalogo.CLASUNIDADMEDIDA4;
                            query.LOGSUSUARIO = catalogo.LOGSUSUARIO;
                            query.LOGSFECHA = DateTime.Now;
                        }

                        cx.SubmitChanges();                        
                        await Task.Run(() => dbContextTransaction.Commit());
                        resp.Status = Configuracion.StatusOk;
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al actualizar catalogo " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al actualizar catalogo " + ex.Message);
                    }
                }
            }

            return resp;
        }

        /// <summary>
        /// Inserta el elemento del catalogo
        /// </summary>
        /// <param name="catalogo">Objeto Catalogo</param>
        /// <returns>Codigo del elemento del catalogo </returns>
        public async Task<ResponseStatus> InsertCatalogo(PONECATALOGO catalogo)
        {
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var existeCatalogo = cx.PONECATALOGOs.Where(c => c.CATACODCATALOGO == catalogo.CATACODCATALOGO).SingleOrDefault();

                        if(existeCatalogo != null)
                        {
                            resp.Status = Configuracion.StatusError;
                            resp.Message = $"Ya existe un registro con el código de catálogo { catalogo.CATACODCATALOGO }.";
                        }
                        else
                        {
                            catalogo.CATACATALOGO = _utilidades.GetSecuencia("SECU_PONECATALOGO", cx);                            
                            catalogo.LOGSFECHA = DateTime.Now; 
                            catalogo.LOGSUSUARIO = catalogo.LOGSUSUARIO;

                            cx.PONECATALOGOs.InsertOnSubmit(catalogo);
                            cx.SubmitChanges();
                            await Task.Run(() => dbContextTransaction.Commit());
                            resp.Status = Configuracion.StatusOk;
                        }                        
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al actualizar catalogo " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al actualizar catalogo " + ex.Message);
                    }
                }
            }

            return resp;
        }

        /// <summary>
        /// Elimina el elemento del catalogo
        /// </summary>
        /// <param name="id">Codigo del elemento del catalogo</param>
        /// <returns>Elemento del Catalogo recientemente eliminado</returns>
        public Catalogo DeleteCatalogo(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        Catalogo catalogo = new Catalogo();
                        var query = cx.PONECATALOGOs.Where(x => x.CATACATALOGO == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException("No se encontro el dato");
                        }
                        else
                        {
                            cx.PONECATALOGOs.DeleteOnSubmit(query);
                        }

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        return GetModelObject(query);
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al eliminar catalogo " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al eliminar catalogo " + ex.Message);
                    }

                }
            }
        }

        #endregion

        #region Metodos Privados

        /// <summary>
        /// Obtiene el objeto del modelo dado el objeto tabla
        /// </summary>
        /// <param name="obj">Objeto tabla</param>
        /// <returns>Objeto Modelo</returns>
        private Catalogo GetModelObject(PONECATALOGO obj)
        {
            Catalogo c = new Catalogo();
            c.CodigoInterno = (int)obj.CATACATALOGO;
            c.CodigoCatalogo = obj.CATACODCATALOGO;
            c.Estado = obj.CATAESTADO;
            c.Nombre = obj.CATANOMBRE;
            c.Tipo = obj.CATATIPO;
            c.UnidadMedida = (int)obj.CLASUNIDADMEDIDA4;
            


            return c;
        }

        /// <summary>
        /// Obtiene el objeto de tabla dado el objeto modelo
        /// </summary>
        /// <param name="obj">Objeto modelo</param>
        /// <returns>Objeto Tabla</returns>
        private PONECATALOGO GetDBObject(Catalogo obj)
        {
            PONECATALOGO c = new PONECATALOGO();
            c.CATACATALOGO = obj.CodigoInterno;
            c.CATACODCATALOGO = obj.CodigoCatalogo; 
            c.CATAESTADO = obj.Estado;
            c.CATANOMBRE = obj.Nombre;
            c.CATATIPO = obj.Tipo;
            c.CLASUNIDADMEDIDA4 = obj.UnidadMedida;

            return c;
        }

        #endregion

    }
}
