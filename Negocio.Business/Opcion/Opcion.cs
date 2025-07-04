using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Negocio.Data;
using Negocio.Model;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class OpcionBusiness: IOpcion
    {
        private readonly IUtilidades _utilidades;
        public OpcionBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }

        #region Metodos Publicos

        /// <summary>
        /// Obtiene Todos los Elementos del Catalogo
        /// </summary>
        /// <returns>Lista de elementos del Catalogo</returns>
        public List<Opcion> GetOpcion()
        {

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_opcion = (from p in cx.POGEOPCIONs
                                  select GetModelObject(p)).ToList();

                return lst_opcion;
            }

        }

        /// <summary>
        /// Obtiene el elemento especifico del Catalogo dado el Id
        /// </summary>
        /// <param name="id">Codigo del Catalogo</param>
        /// <returns>elemento de catalogo</returns>
        public List<Opcion> GetOpcion(decimal id)
        {

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_opcion = (from p in cx.POGEOPCIONs
                                  where p.OPCIOPCION == id
                                  select GetModelObject(p)).ToList();

                return lst_opcion;
            }

        }

        /// <summary>
        /// Actualiza el elemento de la opcion
        /// </summary>
        /// <param name="id">Codigo de la Opcion</param>
        /// <param name="opcion">Objeto Opcion</param>
        public async Task<ResponseStatus> UpdateOpcion(decimal id, Opcion opcion)
        {
            ResponseStatus res = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var query = cx.POGEOPCIONs.Where(x => x.OPCIOPCION == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException("No se encontro el dato");
                        }
                        else
                        {
                            query.OPCINOMBRE = opcion.Nombre;
                            query.OPCIICONO = opcion.Icono;
                            query.OPCIORDEN = opcion.Orden;
                            query.OPCIPADRE = opcion.Padre;
                            query.OPCIURL = opcion.Url;
                            query.OPCIESTADO = opcion.Estado;
                            query.OPCIESTITULO = opcion.EsTitulo;                            
                        }

                        cx.SubmitChanges();
                        await Task.Run(() => dbContextTransaction.Commit());

                        res.Status = Configuracion.StatusOk;
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al actualizar opcion " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al actualizar opcion " + ex.Message);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Inserta el elemento de opcion
        /// </summary>
        /// <param name="opcion">Objeto Opcion</param>
        /// <returns>Codigo del elemento de opcion </returns>
        public async Task<ResponseStatus> InsertOpcion(Opcion opcion)
        {
            int codigo = 0;
            ResponseStatus res = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        codigo = _utilidades.GetSecuencia("SECU_POGEOPCION", cx);
                        opcion.IdOpcion = codigo;
                        cx.POGEOPCIONs.InsertOnSubmit(GetDBObject(opcion));
                        cx.SubmitChanges();
                        await Task.Run(() => dbContextTransaction.Commit());

                        res.Status = Configuracion.StatusOk;
                        
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al insertar opcion " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al insertar opcion " + ex.Message);
                    }

                }
            }

            return res;
        }

        /// <summary>
        /// Elimina el elemento del catalogo
        /// </summary>
        /// <param name="id">Codigo del elemento del catalogo</param>
        /// <returns>Elemento del Catalogo recientemente eliminado</returns>
        public Opcion DeleteOpcion(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        Opcion opcion = new Opcion();
                        var query = cx.POGEOPCIONs.Where(x => x.OPCIOPCION == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException("No se encontro el dato");
                        }
                        else
                        {
                            cx.POGEOPCIONs.DeleteOnSubmit(query);
                        }

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        return GetModelObject(query);
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al eliminar opcion " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al eliminar opcion " + ex.Message);
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
        private Opcion GetModelObject(POGEOPCION obj)
        {
            Opcion o = new Opcion();
            o.Estado = obj.OPCIESTADO;
            o.EsTitulo = obj.OPCIESTITULO;
            o.Icono = obj.OPCIICONO;
            o.IdOpcion = (int)obj.OPCIOPCION;
            o.Nombre = obj.OPCINOMBRE;
            o.Orden = (int)obj.OPCIORDEN;
            o.Padre = (int?)obj.OPCIPADRE;
            o.Url = obj.OPCIURL;


            return o;
        }

        /// <summary>
        /// Obtiene el objeto de tabla dado el objeto modelo
        /// </summary>
        /// <param name="obj">Objeto modelo</param>
        /// <returns>Objeto Tabla</returns>
        private POGEOPCION GetDBObject(Opcion obj)
        {
            POGEOPCION o = new POGEOPCION();
            o.OPCIESTADO = obj.Estado;
            o.OPCIESTITULO = obj.EsTitulo;
            o.OPCIICONO = obj.Icono;
            o.OPCIOPCION = obj.IdOpcion;
            o.OPCINOMBRE = obj.Nombre;
            o.OPCIORDEN = obj.Orden;
            o.OPCIPADRE  = obj.Padre;
            o.OPCIURL = obj.Url;

            return o;
        }
        
        #endregion

    }
}
