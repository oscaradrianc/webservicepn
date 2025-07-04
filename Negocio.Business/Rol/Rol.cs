using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class RolBusiness : IRol
    {
        private readonly IUtilidades _utilidades;
        public RolBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }

        #region Metodos Publicos

        /// <summary>
        /// Obtiene Todos los Elementos del rol
        /// </summary>
        /// <returns>Lista de elementos del rol</returns>
        public async Task<List<POGEROL>> GetRol()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_rol = (from p in cx.POGEROLs
                               select p);
                return await Task.FromResult(lst_rol.ToList());
            }

        }

        /// <summary>
        /// Obtiene Todos los Elementos del rol dado el Id
        /// </summary>
        /// <param name="id">Codigo del rol</param>
        /// <returns>elemento de rol</returns>
        public List<Rol> GetRol(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_rol = (from p in cx.POGEROLs
                                        where p.ROLEROL == id
                                        select GetModelObject(p)).ToList();

                return lst_rol;
            }
        }

        public async Task<List<POGEROL>> GetRoles()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() => (from p in cx.POGEROLs
                                             select p).ToList()
                            );
            }
        }

        /// <summary>
        /// Actualiza el elemento del rol
        /// </summary>
        /// <param name="id">Codigo del rol</param>
        /// <param name="rol">Objeto Rol</param>
        public void UpdateRol(decimal id, Rol rol)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {

                        var query = cx.POGEROLs.Where(x => x.ROLEROL == id).FirstOrDefault();
                        
                        if (query == null)
                        {
                            throw new KeyNotFoundException("No se encontro el dato");
                        }
                        else
                        {
                            query = GetDBObject(rol);

                        }

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al actualizar rol " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al actualizar rol " + ex.Message);
                    }

                }
            }
        }

        /// <summary>
        /// Inserta el elemento del rol
        /// </summary>
        /// <param name="rol">Objeto Rol</param>
        /// <returns>Codigo del elemento del rol </returns>
        public async Task<ResponseStatus> InsertRol(POGEROL rol)
        {
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {   
                        rol.ROLEROL = _utilidades.GetSecuencia("SECU_POGEROL", cx);
                        cx.POGEROLs.InsertOnSubmit(rol);
                        cx.SubmitChanges();
                        
                        await Task.Run(() => dbContextTransaction.Commit());

                        resp.Status = Configuracion.StatusOk;
                    }
                    catch (KeyNotFoundException dx)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar rol: { dx.Message}";
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar rol: { ex.Message}";
                    }
                }
            }

            return resp;
        }

        /// <summary>
        /// Elimina el elemento del rol
        /// </summary>
        /// <param name="id">Codigo del elemento del rol</param>
        /// <returns>Elemento del rol recientemente eliminado</returns>
        public Rol DeleteRol(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        Rol rol = new Rol();
                        var query = cx.POGEROLs.Where(x => x.ROLEROL == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException("No se encontro el dato");
                        }
                        else
                        {
                            cx.POGEROLs.DeleteOnSubmit(query);
                        }

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        return GetModelObject(query);
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al eliminar rol " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al eliminar rol " + ex.Message);
                    }

                }
            }
        }

        #region Opciones x Rol
        public async Task<List<POGEOPCIONXROL>> GetOpcionRol()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_rol = (from p in cx.POGEOPCIONXROLs
                               select p);
                return await Task.FromResult(lst_rol.ToList());
            }
        }

        public async Task<List<POGEOPCIONXROL>> GetOpcionRol(int idRol)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lst_rol = (from p in cx.POGEOPCIONXROLs
                               where p.ROLEROL == idRol
                               select p);
                return await Task.FromResult(lst_rol.ToList());
            }
        }


        public async Task<ResponseStatus> InsertOpcionRol(POGEOPCIONXROL opcionRol)
        {
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        POGEOPCIONXROL opcrol = cx.POGEOPCIONXROLs.Where(x => x.OPCIOPCION == opcionRol.OPCIOPCION && x.ROLEROL == opcionRol.ROLEROL).SingleOrDefault();
                        POGEOPCION opcion = cx.POGEOPCIONs.Where(o => o.OPCIOPCION == opcionRol.OPCIOPCION).SingleOrDefault();

                        if(opcrol == null)
                        {
                            //Si la opcion a agregar a un rol tiene padre, se verifica si existe el padre asigado al usuario, de lo contrario se agrega para poder ver la opcion agregada
                            if(opcion.OPCIPADRE != null)
                            {
                                POGEOPCIONXROL opcrolPadre = cx.POGEOPCIONXROLs.Where(x => x.OPCIOPCION == opcion.OPCIPADRE && x.ROLEROL == opcionRol.ROLEROL).SingleOrDefault();
                                if(opcrolPadre == null)
                                {
                                    opcrolPadre = new POGEOPCIONXROL
                                    {
                                        OPROOPCIONXROL = _utilidades.GetSecuencia("SECU_POGEOPCIONXROL", cx),
                                        OPCIOPCION = Convert.ToInt32(opcion.OPCIPADRE),
                                        ROLEROL = opcionRol.ROLEROL,
                                        LOGSUSUARIO = opcionRol.LOGSUSUARIO
                                    };

                                    cx.POGEOPCIONXROLs.InsertOnSubmit(opcrolPadre);
                                }
                            }
                            
                            opcionRol.OPROOPCIONXROL = _utilidades.GetSecuencia("SECU_POGEOPCIONXROL", cx);
                            cx.POGEOPCIONXROLs.InsertOnSubmit(opcionRol);                            
                            cx.SubmitChanges();

                            await Task.Run(() => dbContextTransaction.Commit());

                            resp.Status = Configuracion.StatusOk;
                        }
                        else
                        {
                            resp.Status = Configuracion.StatusError;
                            resp.Message = $"Ya existe la opción para el rol indicado";
                        }
                    }
                    catch (KeyNotFoundException dx)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar rol: { dx.Message}";
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar rol: { ex.Message}";
                    }
                }
            }

            return resp;
        }


        public async Task<ResponseStatus> DeleteOpcionRol(decimal id)
        {
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        Rol rol = new Rol();
                        var query = cx.POGEOPCIONXROLs.Where(or => or.OPROOPCIONXROL == id).FirstOrDefault();

                        if (query == null)
                        {
                           
                            resp.Status = Configuracion.StatusError;
                            resp.Message = "Error eliminar la opción del rol, no se encontro el registro";
                        }
                        else
                        {
                            cx.POGEOPCIONXROLs.DeleteOnSubmit(query);
                            cx.SubmitChanges();
                            await Task.Run(() => dbContextTransaction.Commit());
                            resp.Status = Configuracion.StatusOk;
                        }
                    }
                    catch (KeyNotFoundException dx)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar rol: { dx.Message}";
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar rol: { ex.Message}";
                    }
                }
            }

            return resp;
        }

        #endregion




        #endregion

        #region Metodos Privados

        /// <summary>
        /// Obtiene el objeto del modelo dado el objeto tabla
        /// </summary>
        /// <param name="obj">Objeto tabla</param>
        /// <returns>Objeto Modelo</returns>
        private Rol GetModelObject(POGEROL obj)
        {
            Rol r = new Rol();
            r.Id = obj.ROLEROL;
            r.Estado = obj.ROLEESTADO;
            r.Nombre = obj.ROLENOMBRE;

            return r;
        }

        /// <summary>
        /// Obtiene el objeto de tabla dado el objeto modelo
        /// </summary>
        /// <param name="obj">Objeto modelo</param>
        /// <returns>Objeto Tabla</returns>
        private POGEROL GetDBObject(Rol obj)
        {
            POGEROL r = new POGEROL();
            r.ROLEROL = obj.Id;
            r.ROLEESTADO = obj.Estado;
            r.ROLENOMBRE = obj.Nombre;

            return r;
        }

        #endregion

    }
}
