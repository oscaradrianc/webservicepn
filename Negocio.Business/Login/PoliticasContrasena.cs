using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Negocio.Business
{
    public class PoliticasContrasena
    {
        private static readonly int kiTamMin = 8;
        private static readonly int kiMayusMin = 1;
        private static readonly int kiMinusMin = 1;
        private static readonly int kiSimbolMin = 1;
        private static readonly int kiNumMin = 1;

        protected PoliticasContrasena()
        {
        }

        /// <summary>
        /// Metodo que valida las reglas para la creacion de una contraseña
        /// </summary>
        /// <param name="pstrContrasena">Contraseña a validar</param>
        /// <param name="pstrCorreo">Correo electronico</param>
        /// <param name="pstrNombres">Nombres</param>
        /// <param name="pstrApellidos">Apellidos</param>
        public static string Validar(string pstrContrasena, string pstrNombres)
        {
            string res = string.Empty;

            //pstrCorreo = pstrCorreo.Trim();
            //pstrNombres = pstrNombres.Trim();

            if (pstrContrasena.Length < kiTamMin)
                res = "La contraseña debe contener mínimo de 8 caracteres";
            else
            if (ContarMayusculas(pstrContrasena) < kiMayusMin)
                res = "La contraseña debe contener mayúsculas y minúsculas";
            else
            if (ContarMinusculas(pstrContrasena) < kiMinusMin)
                res = "La contraseña debe contener mayúsculas y minúsculas";
            else
            if (ContarNumeros(pstrContrasena) < kiNumMin)
                res = "La contraseña debe contener al menos un número";
            else
            if (ContarSimbolos(pstrContrasena) < kiSimbolMin)
                res = "La contraseña debe contener al menos un símbolo";
            //if (CompararCorreo(pstrContrasena, pstrCorreo))
            //   res = "La contraseña no puede ser similar a la dirección de correo";
            /*else
            {
                foreach (string lstrNombre in pstrNombres.Split(' '))
                {
                    if (lstrNombre.Length > 3 && CompararNombre(pstrContrasena, lstrNombre))
                        res = "La contraseña no puede ser similar al nombre del usuario";
                }
            }*/

            return res;
        }

        private static int ContarMayusculas(string pstrContrasena)
        {
            return Regex.Matches(pstrContrasena, "[A-Z]").Count;
        }

        private static int ContarMinusculas(string pstrContrasena)
        {
            return Regex.Matches(pstrContrasena, "[a-z]").Count;
        }

        private static int ContarNumeros(string pstrContrasena)
        {
            return Regex.Matches(pstrContrasena, "[0-9]").Count;
        }

        private static int ContarSimbolos(string pstrContrasena)
        {
            return Regex.Matches(pstrContrasena, @"[^0-9a-zA-Z\._]").Count;
        }

        private static bool CompararCorreo(string pstrContrasena, string psCorreo)
        {
            string lsCorreo = psCorreo.Substring(0, psCorreo.IndexOf('@'));

            return CompararNombre(pstrContrasena, lsCorreo);
        }

        private static bool CompararNombre(string pstrContrasena, string pstrNombre)
        {
            string lstrNombre = pstrNombre.ToUpper();
            string lstrContrasena = pstrContrasena.ToUpper();

            if (lstrContrasena.Equals(lstrNombre))
                return true;

            for (int i = 1; i <= lstrContrasena.Length; i++)
            {
                if ((i + lstrNombre.Length) < lstrContrasena.Length)
                {
                    if (lstrNombre.Equals(lstrContrasena.Substring(i, lstrNombre.Length)))
                        return true;
                }
                else
                {
                    break;
                }
            }

            string lstrPattern = @"[AEIOU1-9]";
            string lstrReplacement = "";

            lstrNombre = Regex.Replace(lstrNombre, lstrPattern, lstrReplacement);
            lstrContrasena = Regex.Replace(lstrContrasena, lstrPattern, lstrReplacement);

            if (lstrContrasena.Equals(lstrNombre))
                return true;

            for (int i = 1; i <= lstrContrasena.Length; i++)
            {
                if ((i + lstrNombre.Length) < lstrContrasena.Length)
                {
                    if (lstrNombre.Equals(lstrContrasena.Substring(i, lstrNombre.Length)))
                        return true;
                }
                else
                {
                    break;
                }
            }

            return false;
        }
    }
}
