using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Negocio.Data;
using Negocio.Model;

namespace Negocio.Business
{
    public class UsuarioBusiness : IUsuario
    {   
        private readonly IConfiguration _configuration;
        private readonly IUtilidades _utilidades;
        
        public UsuarioBusiness(IConfiguration configuration, IUtilidades utilidades)
        {            
            _configuration = configuration;
            _utilidades = utilidades;
        }

        #region Metodos Publicos

        /// <summary>
        /// Obtiene Todos los usuarios
        /// </summary>
        /// <returns>Lista de Usuarios</returns>
        public async Task<List<Usuario>> GetUsuario()
        {

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() => (from p in cx.POGEUSUARIOs
                                                         select GetModelObject(p)).ToList());                
            }

        }

        /// <summary>
        /// Obtiene el elemento especifico del Catalogo dado el Id
        /// </summary>
        /// <param name="id">Codigo del Catalogo</param>
        /// <returns>elemento de catalogo</returns>
        public Usuario GetUsuario(decimal id)
        {

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var usuario = (from p in cx.POGEUSUARIOs
                                    where p.USUAUSUARIO == id
                                    select GetModelObject(p)).SingleOrDefault();

                return usuario;
            }

        }

        /// <summary>
        /// Actualiza el usuario
        /// </summary>
        /// <param name="id">Codigo del Usuario</param>
        /// <param name="usuario">Objeto Usuario</param>
        public async Task UpdateUsuario(decimal id, POGEUSUARIO usuario)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var query = cx.POGEUSUARIOs.Where(x => x.USUAUSUARIO == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException($"No se encontro el usuario con el id: { id }");
                        }
                        else
                        {
                            query.USUANOMBRE = usuario.USUANOMBRE;
                            query.USUAIDENTIFICACION = usuario.USUAIDENTIFICACION;
                            query.USUAESTADO = usuario.USUAESTADO;
                            query.LOGSUSUARIO = usuario.LOGSUSUARIO;
                            query.LOGSFECHA = DateTime.Now;
                            query.USUACORREO = usuario.USUACORREO;
                            query.USUATIPO = usuario.USUATIPO;
                            query.USUAURLDEFECTO = usuario.USUAURLDEFECTO;
                            query.ROLEROL = usuario.ROLEROL;
                            query.CLASAREA2 = usuario.CLASAREA2;
                        }                               
                        
                        cx.SubmitChanges();
                        await Task.Run(() => dbContextTransaction.Commit());
                    }
                    catch (KeyNotFoundException dx)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        throw new KeyNotFoundException("Error al actualizar usuario " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        throw new Exception("Error al actualizar usuario " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Inserta el usuario
        /// </summary>
        /// <param name="usuario">Objeto Usuario</param>
        /// <returns>Codigo del usuario</returns>
        public async Task<ResponseStatus> InsertUsuario(POGEUSUARIO usuario)
        {
            //int codigo = 0;
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();

                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        //Valido si el usuario ya existe
                        var existeUsuario = cx.POGEUSUARIOs.Where(u => u.USUAIDENTIFICADOR.Trim().ToUpper() == usuario.USUAIDENTIFICADOR.Trim().ToUpper()).SingleOrDefault();

                        if(existeUsuario != null)
                        {
                            resp.Status = Configuracion.StatusError;
                            resp.Message = "Identificador de usuario ya existe.";
                        }
                        else
                        {
                            //Valido si la identificacion del usuario ya existe
                            var existeIdentificacion = cx.POGEUSUARIOs.Where(u => u.USUAIDENTIFICACION.Trim() == usuario.USUAIDENTIFICACION.Trim()).SingleOrDefault();

                            if(existeIdentificacion != null)
                            {
                                resp.Status = Configuracion.StatusError;
                                resp.Message = "Ya existe un usuario con el numero de identificación indicado.";
                            }
                            else
                            {
                                usuario.USUAUSUARIO = _utilidades.GetSecuencia("SECU_POGEUSUARIO", cx);
                                usuario.LOGSFECHA = DateTime.Now;
                                cx.POGEUSUARIOs.InsertOnSubmit(usuario);
                                cx.SubmitChanges();
                                await Task.Run(() => dbContextTransaction.Commit());  

                                resp.Status = Configuracion.StatusOk;
                            }
                        }
                    }
                    catch (Exception ex)
                    {                     
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar usuario { ex.Message }";                        
                    }
                }
            }

            return resp;
        }


        /// <summary>
        /// Crea un usuario tipo proveedor
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ResponseStatus CrearUsuarioProveedor(POGEUSUARIO usuario, PORTALNEGOCIODataContext cx)
        {
            ResponseStatus resp = new ResponseStatus();
            cx.POGEUSUARIOs.InsertOnSubmit(usuario);

            try
            {
                cx.SubmitChanges();
            }
            catch (Exception e)
            {
                if (ExisteObjeto(usuario.USUAUSUARIO, cx))
                {
                    resp.Status = "ERROR";
                    resp.Message = "Ya existe el usuario";
                    return resp;
                }
                else
                {
                    resp.Status = "ERROR";
                    resp.Message = e.Message;
                    return resp;
                }
            }

            return resp;

        }


        /// <summary>
        /// Elimina el usuario
        /// </summary>
        /// <param name="id">Codigo del usuario</param>
        /// <returns>Usuario recientemente eliminado</returns>
        public Usuario DeleteUsuario(decimal id)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        Usuario usuario = new Usuario();
                        var query = cx.POGEUSUARIOs.Where(x => x.USUAUSUARIO == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException("No se encontro el dato");
                        }
                        else
                        {
                            cx.POGEUSUARIOs.DeleteOnSubmit(query);
                        }

                        cx.SubmitChanges();
                        dbContextTransaction.Commit();

                        return GetModelObject(query);
                    }
                    catch (KeyNotFoundException dx)
                    {
                        dbContextTransaction.Rollback();
                        throw new KeyNotFoundException("Error al eliminar usuario " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        throw new Exception("Error al eliminar usuario " + ex.Message);
                    }

                }
            }
        }

        /// <summary>
        /// Cambia la clave del usuario
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ResponseStatus CambiarClaveUsuario(CambioClave request)
        {
            ResponseStatus resp = new ResponseStatus();

            try
            {
                using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
                {
                    var query = (from p in cx.POGEUSUARIOs
                                 where p.USUAIDENTIFICADOR == request.Usuario
                                 select p).FirstOrDefault();

                    //Si la clave actual es igual a la clave almacenada
                    if (query.USUACLAVE == _utilidades.GetStringEncriptado(request.ClaveAnterior, _configuration.GetSection("EncryptedKey").Value))
                    {
                        query.USUACLAVE = request.NuevaClave;

                        cx.SubmitChanges();
                        resp.Status = "OK";
                    }
                    else
                    {
                        resp.Message = "La clave anterior no coincide con la almacenada";
                        resp.Status = "BAD";
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return resp;
        }


        public async Task<ResponseStatus> ResetClave(int idUsuario, int diasVenceClave)
        {
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                POGEUSUARIO usu = cx.POGEUSUARIOs.Where(u => u.USUAUSUARIO == idUsuario).SingleOrDefault();

                if(usu != null)
                {
                    string claveAleatoria = _utilidades.GetRandomKey();
                    string claveEncriptada = _utilidades.Encriptar(claveAleatoria, _configuration.GetSection("EncryptedKey").Value);

                    usu.USUACLAVE = claveEncriptada;
                    usu.USUACAMBIARCLAVE = Configuracion.ValorSI;                 
                    usu.USUAFECHAVENCE = DateTime.Now.AddDays(diasVenceClave);

                    await Task.Run(() => cx.SubmitChanges());

                    resp.Status = Configuracion.StatusOk;
                    resp.Message = claveAleatoria;                   
                }
                else
                {
                    resp.Status = Configuracion.StatusError;
                    resp.Message = $"No existe el usuario { idUsuario }";
                }
            }

            return resp;
        }

        #endregion

        #region Metodos Privados


        private bool ExisteObjeto(decimal id, PORTALNEGOCIODataContext db)
        {
            return db.POGEUSUARIOs.Where(x => x.USUAUSUARIO == id).Count() > 0;
        }

        /// <summary>
        /// Obtiene el objeto del modelo dado el objeto tabla
        /// </summary>
        /// <param name="obj">Objeto tabla</param>
        /// <returns>Objeto Modelo</returns>
        private Usuario GetModelObject(POGEUSUARIO obj)
        {
            Usuario u = new Usuario();
            u.IdArea = (int?)obj.CLASAREA2;
            u.IdUsuario = (int)obj.USUAUSUARIO;
            u.LogsUsuario = (int?)obj.LOGSUSUARIO;
            u.LogsFecha = Convert.ToDateTime(obj.LOGSFECHA);
            u.IdRol = (int?)obj.ROLEROL;
            u.IdProveedor = (int?)obj.PROVPROVEEDOR;
            u.Clave = obj.USUACLAVE;
            u.Identificacion = obj.USUAIDENTIFICACION;
            u.Identificador = obj.USUAIDENTIFICADOR;
            u.Email = obj.USUACORREO;
            u.UrlDefecto = obj.USUAURLDEFECTO;
            u.Estado = obj.USUAESTADO;
            u.Tipo = obj.USUATIPO;
            u.Nombres = obj.USUANOMBRE;


            return u;
        }

        /// <summary>
        /// Obtiene el objeto de tabla dado el objeto modelo
        /// </summary>
        /// <param name="obj">Objeto modelo</param>
        /// <returns>Objeto Tabla</returns>
        private POGEUSUARIO GetDBObject(Usuario obj)
        {
            POGEUSUARIO u = new POGEUSUARIO();
            u.USUAUSUARIO = obj.IdUsuario;
            u.USUAIDENTIFICADOR = obj.Identificador;
            u.USUANOMBRE = obj.Nombres;
            u.USUAIDENTIFICACION = obj.Identificacion;
            u.USUAESTADO = obj.Estado;
            u.USUACLAVE = obj.Clave;
            u.LOGSUSUARIO = obj.LogsUsuario;
            u.USUACORREO = obj.Email;
            u.USUATIPO = obj.Tipo;
            u.USUAURLDEFECTO = obj.UrlDefecto;
            u.ROLEROL = obj.IdRol;
            u.PROVPROVEEDOR = obj.IdProveedor;
            u.CLASAREA2 = obj.IdArea;         

            return u;
        }

        #endregion

    }


}
