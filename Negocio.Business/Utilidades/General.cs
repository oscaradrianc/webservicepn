using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class UtilidadesBusiness: IUtilidades
    {
        #region Metodos Publicos

        /// <summary>
        /// Obtiene la lista de actividades economicas
        /// </summary>
        /// <returns></returns>
        public List<ActividadEconomica> ObtenerActividadEconomica()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lta = (from a in cx.POGEACTIVIDADECONOMICAs
                           select new ActividadEconomica
                           {
                               CodigoActividad = a.ACECCODIGOACTIVIDAD,
                               Nombre = a.ACECNOMBRE,
                               Estado = a.ACECESTADO
                           }).ToList();
                return lta;
            }
        }

        public async Task<ActividadEconomica> ObtenerActividadEconomica(string codigoCIIU)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() => (from a in cx.POGEACTIVIDADECONOMICAs
                                             where a.ACECCODIGOACTIVIDAD == codigoCIIU
                                               select new ActividadEconomica
                                               {
                                                   CodigoActividad = a.ACECCODIGOACTIVIDAD,
                                                   Nombre = a.ACECNOMBRE,
                                                   Estado = a.ACECESTADO
                                               }).SingleOrDefault());                
            }

        }

        /// <summary>
        /// Obtiene elementos del catalogo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Catalogo> ObtenerCatalogo(string id = null)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lta = (from p in cx.PONECATALOGOs
                           join q in cx.POGECLASEVALORs on p.CLASUNIDADMEDIDA4 equals q.CLVACLASEVALOR
                           where p.CATAESTADO == "A" && (p.CATATIPO == id || id == null)
                           orderby p.CATANOMBRE
                           select new Catalogo { CodigoCatalogo = p.CATACODCATALOGO, CodigoInterno = (int)p.CATACATALOGO, Estado = p.CATAESTADO, Nombre = p.CATANOMBRE, Tipo = p.CATATIPO, UnidadMedida = (int)p.CLASUNIDADMEDIDA4, Medida = q.CLVAVALOR }).ToList();
                return lta;
            }
        }

        /// <summary>
        /// Obtiene los elementos Parameros de Clase Valor dada una clase
        /// </summary>
        /// <param name="idClase">Identificador de clase</param>
        /// <returns></returns>
        public async Task<List<ClaseValor>> ObtenerClaseValor(int idClase)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lta = (from c in cx.POGECLASEs
                           join v in cx.POGECLASEVALORs on c.CLASCLASE equals v.CLASCLASE
                           where c.CLASCLASE == idClase && v.CLVAESTADO == "A"
                           orderby v.CLVAVALOR
                           select new ClaseValor { Clase = (int)c.CLASCLASE, IdClaseValor = (int)v.CLVACLASEVALOR, CodigoValor = (int)v.CLVACODIGOVALOR, Valor = v.CLVAVALOR, Estado = v.CLVAESTADO, Descripcion = v.CLVADESCRIPCION }).ToList();

                return await Task.Run(() => lta );               
            }
        }

        /// <summary>
        /// Obtiene la lista de municipios
        /// </summary>
        /// <returns></returns>
        public List<Municipio> ObtenerMunicipios()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {

                var lta = (from m in cx.POGEMUNICIPIOs
                           select new Municipio
                           {
                               Codigo = (int)m.MUNICODIGO,
                               Codigomunicio = m.MUNICODMUNICIPIO,
                               Nombre = m.MUNINOMBRE,
                               Codigodepto = (int)m.DEPACODIGO
                           }).ToList();

                return lta;
            }
        }

        /// <summary>
        /// Encripta el texto enviado
        /// </summary>
        /// <param name="texto"></param>
        /// <returns></returns>
        public string Encriptar(string texto, string machineKey)
        {
            try
            {
                using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
                {
                    return GetStringEncriptado(texto, machineKey);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al encriptar: " + ex.Message);
            }            
        }

        /// <summary>
        /// Otiene Clave de manera aleatoria
        /// </summary>
        /// <returns></returns>
        public string GetRandomKey()
        {
            return PasswordGenerator.GeneratePassword(true, true, true, true, false, 9);
        }

        /// <summary>
        /// Obtiene la lista de areas de la empresa
        /// </summary>
        /// <returns></returns>
        public List<Areas> ObtenerAreas()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return cx.PONEVAREAs.Select(x => new Areas {
                    CodArea = x.CODAREA,
                    Estado = (int)x.ESTADO,
                    Gerencia = x.GERENCIA,
                    Nombre = x.NOMBRE
                }).ToList();
            }
        }


        /// <summary>
        /// Obtiene la lista de gerencias de la empresa
        /// </summary>
        /// <returns></returns>
        public List<Gerencias> ObtenerGerencias()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return cx.PONEVGERENCIAs.Select(x => new Gerencias
                {
                    CodGerencia = x.ID,                  
                    Nombre = x.NOMBRE
                }).ToList();
            }
        }

        /// <summary>
        /// Obtiene una constante del sistema dado el nombre de la constante
        /// </summary>
        /// <param name="constante"></param>
        /// <returns></returns>
        public string GetConstante(string constante)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                try
                {
                    //return GetConstante(constante, cx);
                    return cx.ExecuteQuery<string>(string.Format("SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", constante)).FirstOrDefault();

                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener Constante: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Obtiene la lista de documentos por persona
        /// </summary>
        /// <returns></returns>
        public List<DocumentosxPersona> ObtenerDocumentos()
        {
            
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                //List<DocumentosxPersona> ltaDocSarLaft = new List<DocumentosxPersona>();
                //ltaDocSarLaft.Add(new DocumentosxPersona { CLASTIPODOCUMENTO8 = Configuracion.ClaveValorDocSarlaft, CLASTIPOPERSONA1 = Configuracion.TipoPersonaNatural, DOPEOBLIGATORIO = "N", DOPESECUENCIA = 30 });
                //ltaDocSarLaft.Add(new DocumentosxPersona { CLASTIPODOCUMENTO8 = Configuracion.ClaveValorDocSarlaft, CLASTIPOPERSONA1 = Configuracion.TipoPersonaJuridica, DOPEOBLIGATORIO = "N", DOPESECUENCIA = 31 });
                
                var lta = cx.PONEDOCUMENTOXPERSONAs.Select(x=> new DocumentosxPersona { 
                        CLASTIPODOCUMENTO8 = x.CLASTIPODOCUMENTO8,
                        CLASTIPOPERSONA1 = x.CLASTIPOPERSONA1,
                        DOPEOBLIGATORIO = x.DOPEOBLIGATORIO,
                        DOPESECUENCIA = x.DOPESECUENCIA
                }).ToList();

                lta.Add(new DocumentosxPersona { CLASTIPODOCUMENTO8 = Configuracion.ClaveValorDocSarlaft, CLASTIPOPERSONA1 = Configuracion.TipoPersonaNatural, DOPEOBLIGATORIO = "N", DOPESECUENCIA = 30 });
                lta.Add(new DocumentosxPersona { CLASTIPODOCUMENTO8 = Configuracion.ClaveValorDocSarlaft, CLASTIPOPERSONA1 = Configuracion.TipoPersonaJuridica, DOPEOBLIGATORIO = "N", DOPESECUENCIA = 31 });

                return lta;
            }
        }

        /// <summary>
        /// Obtiene la lista de paises
        /// </summary>
        /// <returns></returns>
        public List<Pais> ObtenerPais()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lta = cx.POGEPAIs.Select(x=> new Pais {
                    PAISCODIGO = x.PAISCODIGO,
                    PAISCODPAIS = x.PAISCODPAIS,
                    PAISESTADO = x.PAISESTADO,
                    PAISNOMBRE = x.PAISNOMBRE
                }).ToList();
                return lta;
            }
        }

        /// <summary>
        /// Obtiene la lista de departamentos
        /// </summary>
        /// <returns></returns>
        public List<Departamento> ObtenerDepartamento()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var lta = cx.POGEDEPARTAMENTOs.Select(x => new Departamento
                {
                    DEPACODDEPARTAMENTO = x.DEPACODDEPARTAMENTO,
                    DEPACODIGO = x.DEPACODIGO,
                    DEPANOMBRE = x.DEPANOMBRE,
                    PAISCODIGO = x.PAISCODIGO
                    
                }).ToList();
                return lta;
            }
        }

        public string GetStringEncriptado(string texto, string machineKey)
        {
            //return ctx.ExecuteQuery<string>(string.Format("SELECT F_ENCRIPTAR('{0}') FROM DUAL", texto)).FirstOrDefault();
            byte[] pasBinario = Encoding.Unicode.GetBytes(texto);
            byte[] keyBinario = Encoding.Unicode.GetBytes(machineKey);
            byte[] dst = new byte[keyBinario.Length + pasBinario.Length];
            Buffer.BlockCopy(keyBinario, 0, dst, 0, keyBinario.Length);
            Buffer.BlockCopy(pasBinario, 0, dst, keyBinario.Length, pasBinario.Length);

            return CreateSHA512(dst);

            /*
            using (SHA512 algorithm = new SHA512Managed())
            {
                return Convert.ToBase64String(algorithm.ComputeHash(dst));
            }*/
        }



        public static string CreateSHA512(byte[] strData)
        { 
            using var alg = SHA512.Create(); 
            var hashValue = alg.ComputeHash(strData);
            return Convert.ToBase64String(alg.ComputeHash(hashValue));          
        }

        /// <summary>
        /// Metodo para obtener la secuencia de BD
        /// </summary>
        /// <param name="nombreSecuencia">Nombre de la secuencia de Base de Datos</param>
        /// <param name="ctx">Contexto de Base de Datos</param>
        /// <returns>Valor de la secuencia</returns>
        public int GetSecuencia(string nombreSecuencia, PORTALNEGOCIODataContext ctx)
        {
            return ctx.ExecuteQuery<int>(string.Format("SELECT {0}.NEXTVAL FROM DUAL", nombreSecuencia)).FirstOrDefault();
        }

        /// <summary>
        /// Convierte String en B64 a Array de Bytes
        /// </summary>
        /// <param name="b64"></param>
        /// <returns></returns>
        public byte[] DecodificarArchivo(string b64)
        {
            string[] splitData = b64.Split(',');
            string base64Encoded = splitData.Length > 1 ? splitData[1] : b64;
            byte[] data = System.Convert.FromBase64String(base64Encoded);
            return data;
        }

        /// <summary>
        /// Obtiene las constantes del sistema
        /// </summary>
        /// <param name="nombreSecuencia">Nombre de la secuencia de Base de Datos</param>
        /// <param name="ctx">Contexto de Base de Datos</param>
        /// <returns>Valor de la secuencia</returns>
        /*public string GetConstante(string nombreConstante, PORTALNEGOCIODataContext ctx)
        {
            return ctx.ExecuteQuery<string>(string.Format("SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", nombreConstante)).FirstOrDefault();
        }*/

        /// <summary>
        /// Envia los correos
        /// </summary>
        /// <param name="listaCorreos"></param>
        /// <param name="asunto"></param>
        /// <param name="mensaje"></param>
        /// <param name="ctx"></param>
        public void SendMail(List<string> listaCorreos, string asunto, string mensaje, bool bcc = false)
        {
            try
            {
                MailMessage mail = new MailMessage();
                string servidorMail = GetConstante("serv_mail");
                string sslMail = GetConstante("ssl_mail");
                string pwdMail = GetConstante("pwd_mail");
                string usrMail = GetConstante("usr_mail");
                string sendMail = GetConstante("send_mail");
                int portMail = Convert.ToInt32(GetConstante("port_mail"));

                SmtpClient SmtpServer = new SmtpClient(servidorMail);

                mail.From = new MailAddress(sendMail);

                if (bcc)
                {
                    listaCorreos.ForEach(delegate (string correo)
                    {
                        mail.Bcc.Add(correo);
                    });
                }
                else
                {
                    listaCorreos.ForEach(delegate (string correo)
                    {
                        mail.To.Add(correo);
                    });
                }

                mail.Subject = asunto;
                mail.Body = mensaje;
                mail.IsBodyHtml = true;

                SmtpServer.Port = portMail;
                SmtpServer.Credentials = new System.Net.NetworkCredential(usrMail, pwdMail);
                SmtpServer.EnableSsl = Convert.ToBoolean(sslMail);

                SmtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// Recibe la plantilla del mensaje y una cadena con los parametros y remplaza los valores de las variables
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public string ConvertirMensaje(string mensaje, string parametros)
        {
            string result = mensaje;
            string[] param = parametros.Split('|');

            foreach (var item in param)
            {
                if (!item.Equals(String.Empty))
                {
                    string[] variable = item.Split('~');
                    result = result.Replace("{$" + variable[0].ToUpper() + "}", variable[1]);
                }
            }

            return result;
        }



        public string ObtenerBlob(int idBlob, PORTALNEGOCIODataContext cx)
        {
            byte[] buffer = (from p in cx.PONEBLOBs
                             where p.BLOBBLOB == idBlob
                             select p.BLOBDATO).FirstOrDefault();
            return Convert.ToBase64String(buffer);

        }

        public decimal? IsDecimal(string strNumber)
        {
            decimal numero;
            bool esNumero = decimal.TryParse(strNumber, out numero);
            if (esNumero)
            {
                return numero;
            }
            else
            {
                return null;
            }
        }
        public DateTime TruncateToDayStart(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }


        public List<string> ObtenerCorreosProveedor()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var q = (from p in cx.PONEPROVEEDORs
                         where p.PROVESTADO == Configuracion.EstadoActivo
                           && p.PROVEMAIL != null
                         select p.PROVEMAIL).Distinct().ToList();
                return q;
            }
        }

        public List<string> ObtenerCorreoProveedoresSolicitud(int codigoSolicitud)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var q = (from s in cx.PONESOLICITUDCOMPRAs
                         join c in cx.PONECOTIZACIONs on s.SOCOSOLICITUD equals c.SOCOSOLICITUD
                         join p in cx.PONEPROVEEDORs on c.PROVPROVEEDOR equals p.PROVPROVEEDOR
                         where s.SOCOSOLICITUD == codigoSolicitud                           
                         select p.PROVEMAIL).ToList();
                return q;
            }

        }
        #endregion

        public string ObtenerValorClaseValor(int idClaseValor)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var v = (from p in cx.POGECLASEVALORs
                         where p.CLVACLASEVALOR == idClaseValor                        
                         select p.CLVAVALOR).SingleOrDefault();

                return v != null ? v.ToString() : string.Empty;
            }
        }

        public async Task<Municipio> ObtenerMunicipio(int idMunicipio)
        {
            using(PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() => (from mu in cx.POGEMUNICIPIOs
                                         where mu.MUNICODIGO == idMunicipio
                                         select new Municipio
                                         {
                                             Codigo = (int)mu.MUNICODIGO,
                                             Codigomunicio = mu.MUNICODMUNICIPIO,
                                             Nombre = mu.MUNINOMBRE,
                                             Codigodepto = (int)mu.DEPACODIGO
                                         }).SingleOrDefault());

                
            }
        }

        public async Task<PONEDOCUMENTO> ObtenerDocumentoxId(int idDocumento)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() => (from doc in cx.PONEDOCUMENTOs
                                             where doc.DOCUDOCUMENTO == idDocumento
                                             select doc
                                             ).SingleOrDefault());
            }
        }

        /*public byte[] DecodificarArchivo(string b64)
        {
            string[] splitData = b64.Split(',');
            string base64Encoded = splitData.Length > 1 ? splitData[1] : b64;
            byte[] data = System.Convert.FromBase64String(base64Encoded);
            return data;
        }*/

        public (string contentType, byte [] data) DecodificarArchivoContentType(string base64Content)
        {
            var parts = base64Content.Split(',');

            if (parts.Length == 2 && parts[0].StartsWith("data:") && parts[0].Contains(";base64"))
            {
                var contentType = parts[0].Split(':')[1].Split(';')[0];
                var data = Convert.FromBase64String(parts[1]);
                return (contentType, data);
            }

            // Si no hay encabezado MIME, asumimos un tipo genérico
            return ("application/octet-stream", Convert.FromBase64String(base64Content));
        }
    }
}
