using Negocio.Data;
using Negocio.Model;
using Stubble.Core.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Negocio.Business
{
    public static class Utilidades
    {


        /// <summary>
        /// Obtiene Texto encriptado sha512
        /// </summary>
        /// <param name="texto">cadena de texto que se desea encriptar</param>
        /// <param name="ctx">Contexto de Base de Datos</param>
        /// <returns>Cadena Encriptada</returns>
        public static string GetStringEncriptado(string texto, string machineKey)
        {
            //return ctx.ExecuteQuery<string>(string.Format("SELECT F_ENCRIPTAR('{0}') FROM DUAL", texto)).FirstOrDefault();
            byte[] pasBinario = Encoding.Unicode.GetBytes(texto);
            byte[] keyBinario = Encoding.Unicode.GetBytes(machineKey);
            byte[] dst = new byte[keyBinario.Length + pasBinario.Length];
            Buffer.BlockCopy(keyBinario, 0, dst, 0, keyBinario.Length);
            Buffer.BlockCopy(pasBinario, 0, dst, keyBinario.Length, pasBinario.Length);
            using (SHA512 algorithm = new SHA512Managed())
            {
                return Convert.ToBase64String(algorithm.ComputeHash(dst));
            }
        }


        /// <summary>
        /// Metodo para obtener la secuencia de BD
        /// </summary>
        /// <param name="nombreSecuencia">Nombre de la secuencia de Base de Datos</param>
        /// <param name="ctx">Contexto de Base de Datos</param>
        /// <returns>Valor de la secuencia</returns>
        public static int GetSecuencia(string nombreSecuencia, PORTALNEGOCIODataContext ctx)
        {
            return ctx.ExecuteQuery<int>(string.Format("SELECT {0}.NEXTVAL FROM DUAL", nombreSecuencia)).FirstOrDefault();
        }

        /// <summary>
        /// Convierte String en B64 a Array de Bytes
        /// </summary>
        /// <param name="b64"></param>
        /// <returns></returns>
        public static byte[] DecodificarArchivo(string b64)
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
        public static string GetConstante(string nombreConstante, PORTALNEGOCIODataContext ctx)
        {
            return ctx.ExecuteQuery<string>(string.Format("SELECT CONS_VALOR FROM POGE_CONSTANTE WHERE CONS_REFERENCIA='{0}'", nombreConstante)).FirstOrDefault();
        }

        /// <summary>
        /// Envia los correos
        /// </summary>
        /// <param name="listaCorreos"></param>
        /// <param name="asunto"></param>
        /// <param name="mensaje"></param>
        /// <param name="ctx"></param>
        public static void SendMail(List<string> listaCorreos, string asunto, string mensaje, PORTALNEGOCIODataContext ctx, bool bcc = false)
        {
            try
            {
                MailMessage mail = new MailMessage();
                string servidorMail = GetConstante("serv_mail", ctx);
                string sslMail = GetConstante("ssl_mail", ctx); 
                string pwdMail = GetConstante("pwd_mail", ctx); 
                string usrMail = GetConstante("usr_mail", ctx);
                string sendMail = GetConstante("send_mail", ctx);
                int portMail = Convert.ToInt32(GetConstante("port_mail", ctx));

                SmtpClient SmtpServer = new SmtpClient(servidorMail);

                mail.From = new MailAddress(sendMail);

                if (bcc)
                {
                    listaCorreos.ForEach(delegate(string correo)
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
        public static string ConvertirMensaje(string mensaje, string parametros)
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


       
        public static string ObtenerBlob(int idBlob, PORTALNEGOCIODataContext cx)
        {
            byte[] buffer = (from p in cx.PONEBLOBs
                                where p.BLOBBLOB == idBlob
                                select p.BLOBDATO).FirstOrDefault();
            return Convert.ToBase64String(buffer);
            
        }

        public static void InsertLogErrorBD() { }

        public static decimal? isDecimal(string strNumber)
        {
            decimal numero;
            bool esNumero = decimal.TryParse(strNumber, out numero);
            if (esNumero)
            {
                return numero;
            } else
            {
                return null;
            }
        }

        public static DateTime TruncateToDayStart(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }
    }
}
