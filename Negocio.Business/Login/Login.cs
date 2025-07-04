using Microsoft.Extensions.Configuration;
using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class LoginBusiness: ILogin
    {
        private readonly IUtilidades _utilidades;        

        public LoginBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;         
        }

        #region Metodos Publicos

        /// <summary>
        /// Autentica el usuario por medio de token JWT
        /// </summary>
        /// <param name="login">Usuario y Password</param>
        /// <param name="configuration"></param>
        /// <returns>Objeto Usuario con Token</returns>
        public Response<Usuario> Authenticate(LoginRequest login, IConfiguration configuration)
        {
            Response<Usuario> resp = new Response<Usuario>();
            PORTALNEGOCIODataContext dc = new PORTALNEGOCIODataContext();

            //using (var cmd = dc.Connection.CreateCommand())
            //{
                Usuario usr = new Usuario();

                //dc.Connection.Open();

                var usuario = (from u in dc.POGEUSUARIOs
                               where u.USUAIDENTIFICADOR.ToUpper() == login.Username.ToUpper()
                                  && ((login.Origen == Configuracion.TipoUsuarioProveedor) ? u.PROVPROVEEDOR != null : u.USUATIPO == login.Origen)
                               select u).SingleOrDefault();

                if (usuario != null)
                {
                    string claveEncriptada = _utilidades.Encriptar(login.Password, configuration.GetSection("EncryptedKey").Value);
                    if (usuario.USUACLAVE != claveEncriptada)
                    {
                        usr.ResultadoLogin = -2; //Contrase;a invalida                        
                    }
                    else
                    {
                        if (usuario.USUACAMBIARCLAVE == "S")
                        {
                            usr.ResultadoLogin = 2;
                            usr.Identificador = usuario.USUAIDENTIFICADOR;
                        }
                        else
                        if(usuario.USUAVENCECLAVE == "S" && usuario.USUAFECHAVENCE < DateTime.Now)
                        {
                            usr.ResultadoLogin = 3; // Clave vencida
                            usr.Identificador = usuario.USUAIDENTIFICADOR;
                        }
                        else
                        if (usuario.USUAESTADO == "I")
                        {
                            usr.ResultadoLogin = -1; //Retorna esta inactivo
                            usr.Estado = usuario.USUAESTADO;
                        }
                        else //Si lleva hasta aca esta correctamente logueado                       
                        {
                            usr.ResultadoLogin = 1;
                            usr.IdUsuario = (int)usuario.USUAUSUARIO;
                            usr.Nombres = usuario.USUANOMBRE;
                            usr.Identificacion = usuario.USUAIDENTIFICACION;
                            usr.Identificador = usuario.USUAIDENTIFICADOR;
                            usr.Email = usuario.USUACORREO;
                            usr.IdRol = (int?)usuario.ROLEROL;
                            usr.IdProveedor = (int?)usuario.PROVPROVEEDOR;
                            usr.IdArea = (int?)usuario.CLASAREA2;
                            usr.UrlDefecto = usuario.USUAURLDEFECTO;
                            //usr.Clave =  Convert.ToString(rdr["usua_clave"]);
                            usr.Estado = usuario.USUAESTADO;

                            if (usr.IdProveedor != null)
                                usr.Proveedor = GetModelObject(dc.PONEPROVEEDORs.Where(x => x.PROVPROVEEDOR == usr.IdProveedor).FirstOrDefault());

                            if (usr.IdRol != null) //Si tiene rol asignado y esta correctamente logueado retorna opcines de menu
                            {
                                usr.Opciones = (from o in dc.POGEOPCIONs
                                                join r in dc.POGEOPCIONXROLs on o.OPCIOPCION equals r.OPCIOPCION
                                                where r.ROLEROL == usr.IdRol && o.OPCIESTADO == "A"
                                                orderby o.OPCIORDEN
                                                select new Opcion
                                                {
                                                    IdOpcion = (int)o.OPCIOPCION,
                                                    Nombre = o.OPCINOMBRE,
                                                    Url = o.OPCIURL,
                                                    Orden = (int)o.OPCIORDEN,
                                                    Padre = (o.OPCIPADRE == null) ? (int?)null : (int)o.OPCIPADRE,
                                                    EsTitulo = o.OPCIESTITULO,
                                                    Estado = o.OPCIESTADO,
                                                    Icono = o.OPCIICONO
                                                }).ToList();
                            }
                            
                            //TODO generar token JWT
                            usr.Token = TokenGenerator.GenerateTokenJwt(usr, configuration);
                        }
                    }
                }//Usuario no existe o no es del tipo correcto
                else
                {
                    usr.ResultadoLogin = -2;
                }

            //}

            resp.Data = usr;
            resp.Status = new ResponseStatus { Status = HttpStatusCode.OK.ToString(), Message = "" };

            return resp;

            /*
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = string.Format(@"SELECT TREAT(usr AS usuario).usua_usuario usua_usuario,
                                                     TREAT(usr AS usuario).usua_identificador usua_identificador, 
                                                     TREAT(usr AS usuario).usua_nombre usua_nombre, 
                                                     TREAT(usr AS usuario).usua_identificacion usua_identificacion,
                                                     TREAT(usr AS usuario).usua_correo usua_correo,
                                                     TREAT(usr AS usuario).role_rol role_rol,
                                                     TREAT(usr AS usuario).prov_proveedor prov_proveedor,
                                                     TREAT(usr AS usuario).clas_area2 clas_area2,
                                                     TREAT(usr AS usuario).usua_urldefecto usua_urldefecto,
                                                     TREAT(usr AS usuario).usua_clave usua_clave,
                                                     TREAT(usr AS usuario).result_login result_login,
                                                     TREAT(usr AS usuario).usua_estado usua_estado
                                                FROM (SELECT * FROM (SELECT F_VALIDAR_LOGIN('{0}', '{1}') AS usr FROM DUAL ))", login.Username, login.Password);

            var rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                rdr.Read();

                usr.ResultadoLogin = Convert.ToInt32(rdr["result_login"]);

                if (usr.ResultadoLogin > 0)
                {
                    usr.IdUsuario = Convert.ToInt32(rdr["usua_usuario"]);
                    usr.Nombres = Convert.ToString(rdr["usua_nombre"]);
                    usr.Identificacion = Convert.ToString(rdr["usua_identificacion"]);
                    usr.Identificador = Convert.ToString(rdr["usua_identificador"]);
                    usr.Email = Convert.ToString(rdr["usua_correo"]);
                    usr.IdRol = (rdr["role_rol"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rdr["role_rol"]);
                    usr.IdProveedor = (rdr["prov_proveedor"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rdr["prov_proveedor"]);
                    usr.IdArea = (rdr["clas_area2"] == DBNull.Value) ? (int?)null : Convert.ToInt32(rdr["clas_area2"]);
                    usr.UrlDefecto = Convert.ToString(rdr["usua_urldefecto"]);
                    usr.Clave = Convert.ToString(rdr["usua_clave"]);
                    usr.Estado = Convert.ToString(rdr["usua_estado"]);

                    if(usr.IdProveedor != null)
                        usr.Proveedor = GetModelObject(dc.PONEPROVEEDORs.Where(x => x.PROVPROVEEDOR == usr.IdProveedor).FirstOrDefault());

                    if (usr.IdRol != null && usr.ResultadoLogin == 1) //Si tiene rol asignado y esta correctamente logueado retorna opcines de menu
                    {
                        usr.Opciones = (from o in dc.POGEOPCIONs
                                        join r in dc.POGEOPCIONXROLs on o.OPCIOPCION equals r.OPCIOPCION
                                        where r.ROLEROL == usr.IdRol && o.OPCIESTADO == "A" 
                                        orderby o.OPCIORDEN
                                        select new Opcion
                                        {
                                            IdOpcion = (int)o.OPCIOPCION,
                                            Nombre = o.OPCINOMBRE,
                                            Url = o.OPCIURL,
                                            Orden = (int)o.OPCIORDEN,
                                            Padre = (o.OPCIPADRE == null) ? (int?)null : (int)o.OPCIPADRE,
                                            EsTitulo = o.OPCIESTITULO,
                                            Estado = o.OPCIESTADO,
                                            Icono = o.OPCIICONO
                                        }).ToList();
                    }

                    if (usr.ResultadoLogin == 1 || usr.ResultadoLogin == 2)
                    {
                        //TODO generar token JWT
                        usr.Token = TokenGenerator.GenerateTokenJwt(usr, configuration);
                    }

                }

                resp.Data = usr;
            }
            else
            {
                usr.ResultadoLogin = -2;
                resp.Data = usr;
                resp.Status = new ResponseStatus { Status = HttpStatusCode.OK.ToString() , Message = "" };
            }*/
        }


        public ResponseStatus ChangePassword(ChangePasswordRequest changePassword, IConfiguration configuration)
        {
            ResponseStatus resp = new ResponseStatus();
            PORTALNEGOCIODataContext dc = new PORTALNEGOCIODataContext();

            Response<Usuario> usua = Authenticate(new LoginRequest { Username = changePassword.Username, Password = changePassword.Password, Origen = changePassword.Origen }, configuration);

            if(usua != null)
            {
                //Si el resultado del login es 1, 2 o 3 quiere decir que esta correctamente logueado y procede con el cambio de contrase;a
                if (usua.Data.ResultadoLogin == 1 || usua.Data.ResultadoLogin == 2 || usua.Data.ResultadoLogin == 3)
                {
                    string resPolitica = PoliticasContrasena.Validar(changePassword.NewPassword, changePassword.Username);
                                        
                    string nuevaClave = _utilidades.Encriptar(changePassword.NewPassword, configuration.GetSection("EncryptedKey").Value);
                    string actualClave = _utilidades.Encriptar(changePassword.Password, configuration.GetSection("EncryptedKey").Value);
                    //string nuevaClave = dc.FENCRIPTAR(changePassword.NewPassword);
                    //string actualClave = dc.FENCRIPTAR(changePassword.Password);

                    if (nuevaClave == actualClave)
                    {
                        resp.Status = "ERROR";
                        resp.Message = "La contraseña indicada es igual a la actual, intente otra";
                    }
                    else
                        //Si el resultado de validar la nueva contrase;a, es diferente de empty
                        //quiere decir que no paso la validacion de politicas de contrase;a
                        if (!string.IsNullOrEmpty(resPolitica))
                        {
                            resp.Status = "ERROR";
                            resp.Message = resPolitica;
                        }
                        else //Paso todas las validaciones realiza el cambio de clave                
                        {
                            Nullable<decimal> res = dc.FCAMBIARCLAVE(changePassword.Username, nuevaClave);

                            if(res != null)
                            {
                                if(res == 1)
                                {
                                    resp.Status = Configuracion.StatusOk;
                                    resp.Message = string.Empty;
                                }
                                else
                                {
                                    resp.Status = Configuracion.StatusError;
                                    resp.Message = "Usuario o contraseña actual no valido";
                                }
                            }
                        }
                }  
                else
                {
                    resp.Status = Configuracion.StatusError;
                    resp.Message = "Usuario o contraseña actual no valido";
                }
            }
            else
            {
                resp.Status = Configuracion.StatusError;
                resp.Message = "Usuario o contraseña actual no valido";
            }
            
            return resp;
        }


        public ResponseStatus ResetPassword(ResetPassRequest req, IConfiguration configuration)
        {
            ResponseStatus resp = new ResponseStatus();
            PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();

            var tblUsuario = (from u in cx.POGEUSUARIOs
                              where u.USUAIDENTIFICADOR == req.Username && u.USUAESTADO == Configuracion.EstadoActivo                               
                              select u).FirstOrDefault();

            if(tblUsuario == null)
            {
                resp.Status = Configuracion.StatusError;
                resp.Message = "No existe el usuario";
            }
            else
            {
                if(tblUsuario.USUACORREO.ToUpper() == req.Email.ToUpper())
                {
                    //string claveGenerada = cx.GENERARCLAVEALEATORIA();
                    //string claveEncriptada = cx.FENCRIPTAR(claveGenerada);
                    string claveGenerada = _utilidades.GetRandomKey();
                    string claveEncriptada = _utilidades.Encriptar(claveGenerada, configuration.GetSection("EncryptedKey").Value);

                    Usuario usr = new Usuario
                    {
                        Identificador = tblUsuario.USUAIDENTIFICADOR,
                        Nombres = tblUsuario.USUANOMBRE,                       
                        Clave = claveGenerada,
                        Email = tblUsuario.USUACORREO
                    };

                    tblUsuario.USUAESTADO = Configuracion.EstadoActivo;
                    tblUsuario.USUACLAVE = claveEncriptada;
                    tblUsuario.USUACAMBIARCLAVE = Configuracion.ValorSI;

                    cx.SubmitChanges();

                    (new NotificacionBusiness(_utilidades)).GenerarNotificacion(Configuracion.NotificacionResetPassword, usr);

                    resp.Status = Configuracion.StatusOk;
                    resp.Message = "Se envio correo electrónico para reestablecer la contraseña.";
                }
                else
                {
                    resp.Status = Configuracion.StatusError;
                    resp.Message = "El correo electrónico no corresponde al registrado para el usuario.";
                }
            }

            return resp;
        }

        /// <summary>
        /// Obtiene el objeto del modelo dado el objeto tabla
        /// </summary>
        /// <param name="obj">Objeto tabla</param>
        /// <returns>Objeto Modelo</returns>
        private Proveedor GetModelObject(PONEPROVEEDOR obj)
        {
            Proveedor p = new Proveedor
            {
                TipoPersona = (int)obj.CLASTIPOPERSONAL1,
                CodigoProveedor = (int)obj.PROVPROVEEDOR
            };
            return p;
        }

        #endregion
    }
}
